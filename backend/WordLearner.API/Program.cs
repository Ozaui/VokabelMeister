using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using WordLearner.API.BackgroundServices;
using WordLearner.API.Filters;
using WordLearner.API.Logging;
using WordLearner.API.Middleware;
using WordLearner.Application.Extensions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Override — appsettings.json'daki Logging:LogLevel ayarı yalnızca ASP.NET Core'un builtin
// logger'ı içindir; Serilog kod üzerinden yapılandırıldığı için aynı susturma burada tekrarlanır,
// aksi halde framework'ün "Request starting/finished" logları RequestResponseLoggingMiddleware ile çakışır.
// AutoCreateSqlTable=false — tablo zaten AddLoggingTables migration'ıyla oluşturuldu.
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.MSSqlServer(
        connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions { TableName = "ApplicationLogs", AutoCreateSqlTable = false },
        columnOptions: ApplicationLogColumnOptions.Build()));

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "VokabelMeister API",
            Version = "v1",
            Description = "Almanca-Türkçe kelime öğrenme uygulaması Web API'si.",
        }
    );
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Environment.IsDevelopment());
builder.Services.AddHostedService<AccountCleanupBackgroundService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("Default", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
          .AllowAnyMethod()
          .AllowAnyHeader()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // DI konteynerine burada (middleware pipeline dışı bir yapılandırma callback'i) doğrudan
    // erişim olmadığı için RequestServices üzerinden scope içi ISecurityLogger çözülür.
    options.OnRejected = async (context, ct) =>
    {
        var securityLogger = context
            .HttpContext.RequestServices.GetRequiredService<ISecurityLogger>();
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;

        await securityLogger.LogAsync(
            LogEventType.RateLimitHit,
            userId,
            ipAddress: context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            detail: context.HttpContext.Request.Path.ToString(),
            ct: ct
        );
    };

    options.AddFixedWindowLimiter("anonymous", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("authenticated", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    // IP başına partitioned — QR generate kötüye kullanılırsa tek bir saldırgan IP tüm
    // kullanıcıların anonim limitini tüketmemeli.
    options.AddPolicy(
        "qrGenerate",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromHours(1),
                    QueueLimit = 0,
                }
            )
    );

    // IP başına partitioned — paylaşımlı "anonymous" limitini kullansaydı, ~2sn'de bir
    // çağrılan bu polling endpoint'i ~20 saniyede tüm anonim trafiği (register/login dahil) kilitlerdi.
    options.AddPolicy(
        "qrStatus",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 40,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }
            )
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VokabelMeister API v1"));
}

// UseHttpsRedirection en başta — düz HTTP ile gelen bir istek hiçbir iş yapılmadan
// (loglanmadan, başlık eklenmeden) doğrudan HTTPS'e yönlendirilmeli.
app.UseHttpsRedirection();

// Loglama en dışta durur ki exception fırlasa bile gerçek süre ve nihai durum kodu loglanabilsin.
// Güvenlik başlıkları hata yanıtı dahil her yanıta eklensin diye exception middleware'inden önce durur.
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Auth'tan önce — /uploads altındaki görseller herkese açık, token gerekmemeli.
app.UseStaticFiles();

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

// UseAuthorization'dan sonra — rate limiter kimlik/yetki kontrolünden sonra, controller'dan hemen önce devreye girmeli.
app.UseRateLimiter();

app.MapControllers();

app.Run();
