using System.Collections.ObjectModel;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using WordLearner.API.Common;
using WordLearner.Application.Validators.Words;
using WordLearner.API.Conventions;
using WordLearner.API.Middleware;
using WordLearner.Application;
using WordLearner.Application.Common;
using WordLearner.Application.Common.Behaviors;
using WordLearner.Application.DTOs;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure;
using WordLearner.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    // ApplicationLogs (A-04, Domain/Entities/Logging/ApplicationLog.cs) tablosuyla birebir eşleşen
    // kolon eşlemesi — MessageTemplate/varsayılan XML Properties bu tabloda yok, LogEvent'i JSON
    // olarak "Properties" kolonuna yönlendiriyoruz.
    var columnOptions = new ColumnOptions();
    columnOptions.Store.Remove(StandardColumn.MessageTemplate);
    columnOptions.Store.Remove(StandardColumn.Properties);
    columnOptions.Store.Add(StandardColumn.LogEvent);
    columnOptions.LogEvent.ColumnName = "Properties";
    columnOptions.LogEvent.ExcludeAdditionalProperties = true;
    columnOptions.AdditionalColumns = new Collection<SqlColumn>
    {
        new SqlColumn { ColumnName = "SourceContext", DataType = SqlDbType.NVarChar, DataLength = 255 },
        new SqlColumn { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 500 },
        new SqlColumn { ColumnName = "UserId", DataType = SqlDbType.Int }
    };

    configuration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.MSSqlServer(
            connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
            sinkOptions: new MSSqlServerSinkOptions { TableName = "ApplicationLogs", AutoCreateSqlTable = false },
            columnOptions: columnOptions);
});

builder.Services.AddControllers(options => options.Conventions.Add(new RoutePrefixConvention("api/v1")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddHealthChecks().AddDbContextCheck<WordLearnerDbContext>();

// IRepository<> marker olarak kullanılıyor çünkü o assembly'de kesin var olan, kararlı bir tür.
// WordGrammarValidator FİLTRELENİR — bir Command'a değil WordGrammarInput'a bağlı, constructor'ı
// (languageCode, partOfSpeech) DI'nin çözemeyeceği primitive parametreler alıyor; Command
// validator'ları onu ELLE örnekliyor (bkz. CreateWordCommandValidator). Filtrelenmeseydi
// Development ortamının ValidateOnBuild kontrolü uygulamayı AÇILIŞTA çökertirdi.
builder.Services.AddValidatorsFromAssembly(typeof(IRepository<>).Assembly,
    filter: result => result.ValidatorType != typeof(WordGrammarValidator));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IRepository<>).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("Default", policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyMethod()
    .AllowAnyHeader()));

// 5 isimli policy — her Controller action'ı [EnableRateLimiting("...")] ile hangisini kullanacağını
// seçer; burada isimlendirilmeyen bir endpoint (ör. /health) HİÇ limitlenmez (global bir varsayılan
// policy set edilmedi, bilerek — health check'in izlenebilir olması gerekiyor).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("general", context => RateLimitPartition.GetFixedWindowLimiter(
        GeneralPartitionKey(context),
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = 100 }));

    options.AddPolicy("anonymous", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(context),
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = 10 }));

    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(context),
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(15), PermitLimit = 5 }));

    options.AddPolicy("qrGenerate", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(context),
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromHours(1), PermitLimit = 20 }));

    options.AddPolicy("qrStatus", context => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(context),
        _ => new FixedWindowRateLimiterOptions { Window = TimeSpan.FromMinutes(1), PermitLimit = 40 }));

    options.OnRejected = async (rejectedContext, cancellationToken) =>
    {
        var httpContext = rejectedContext.HttpContext;
        var securityLogger = httpContext.RequestServices.GetRequiredService<ISecurityLogger>();
        await securityLogger.LogAsync(LogEventType.RateLimitHit,
            ipAddress: ClientIp(httpContext), userAgent: httpContext.Request.Headers.UserAgent.ToString(),
            detail: "RATE_LIMIT_EXCEEDED", cancellationToken: cancellationToken);

        var message = ErrorMessages.Resolve("RATE_LIMIT_EXCEEDED", httpContext.GetLanguage());
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(false, new ApiErrorDetail("RATE_LIMIT_EXCEEDED", message)), cancellationToken);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();
// Authorization'dan SONRA — "general" policy'nin partition anahtarı (aşağıdaki GeneralPartitionKey)
// context.User'ın (JWT claim'leri) doldurulmuş olmasına bağlı.
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteHealthCheckResponseAsync });

// AdminController (A-18) ve [Authorize(Roles="Admin")] uçları için ilk Admin'i garantiler — idempotent,
// her başlangıçta güvenle tekrar çalışır (INITIAL_ADMIN_EMAIL/PASSWORD tanımlı değilse sessizce atlar).
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IAdminSeedService>().SeedInitialAdminAsync();
}

app.Run();

static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        databaseConnected = report.Status == HealthStatus.Healthy,
        timestampUtc = DateTime.UtcNow
    });
}

// "general" policy [Authorize] uçlarda kullanılıyor — kimliği doğrulanmış her kullanıcı KENDİ
// bütçesini tüketir (IP DEĞİL, ör. aynı NAT arkasındaki iki kullanıcı birbirinin limitini YEMEZ).
static string GeneralPartitionKey(HttpContext context) =>
    context.User.Identity?.IsAuthenticated == true
        ? context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ClientIp(context)
        : ClientIp(context);

static string ClientIp(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
