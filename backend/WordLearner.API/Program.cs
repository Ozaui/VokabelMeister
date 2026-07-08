// ─────────────────────────────────────────────────────────────────────────────
// Program.cs
//
// AMAÇ: Uygulamanın giriş noktası (composition root). Servisleri DI konteynerine
//       kaydeder ve HTTP istek hattını (middleware pipeline) kurar.
// NEDEN: .NET 9 minimal hosting modelinde tüm başlangıç yapılandırması tek dosyada
//        toplanır; her API/altyapı parçası buraya bağlanır.
// BAĞIMLILIKLAR: ASP.NET Core, Swashbuckle (Swagger UI), Serilog, JWT Bearer,
//                WordLearner.Infrastructure/Application service extension'ları,
//                WordLearner.API.Middleware (SecurityHeaders/ExceptionHandling/RequestResponseLogging).
//
// NOT (A-02 — Ortak Altyapı, tamamlandı): DbContext, JWT auth, CORS, Serilog
//   (konsol+dosya), FluentValidation, MediatR, AutoMapper kayıtları ve güvenlik
//   başlıkları/global exception/istek-yanıt log middleware'leri burada kuruldu.
//   Serilog'un MSSqlServer (ApplicationLog) sink'i A-04'te eklenecek — o tablo
//   henüz bir migration ile oluşturulmadı (bkz. TASK/A_admin_panel_backend.md A-04).
// ─────────────────────────────────────────────────────────────────────────────

using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WordLearner.API.Filters;
using WordLearner.API.Middleware;
using WordLearner.Application.Extensions;
using WordLearner.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ADIM 1: Serilog — konsol + dosya sink. DB (ApplicationLog) sink'i A-04'te eklenir.
// NEDEN: _logger.LogInformation/.LogError çağrıları artık ASP.NET Core'un varsayılan
//        konsol logger'ı yerine Serilog üzerinden akar; ileride tek satırla DB sink eklenebilir.
// NEDEN Override: appsettings.json'daki Logging:LogLevel:Microsoft.AspNetCore=Warning
//        ayarı yalnızca ASP.NET Core'un kendi builtin logger'ı içindir; Serilog kod
//        üzerinden yapılandırıldığı için aynı susturma burada elle tekrarlanır —
//        aksi halde framework'ün "Request starting/finished" logları RequestResponseLoggingMiddleware
//        ile çakışıp konsolu ikiye katlar.
builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

// ADIM 2: Controller'ları ekle — API uç noktaları controller sınıflarında tanımlanır.
// NEDEN ValidationFilter: FluentValidation.AspNetCore paketi kullanılmıyor
//       (TECHNICAL_SPECIFICATIONS.md §1'de yok) — bu global filter, DI'a kayıtlı
//       IValidator<T>'leri action çalışmadan önce otomatik çalıştırır (A-03).
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());

// ADIM 3: Swagger/OpenAPI — yazılan endpoint'ler geliştirme ortamında otomatik
// belgelenir ve http://localhost:5001/swagger adresinden test edilir.
// NEDEN: Junior geliştirici ve manuel test için canlı API dokümantasyonu sağlar.
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

// ADIM 4: Infrastructure (DbContext + repository) ve Application (MediatR,
// AutoMapper, FluentValidation) servislerini tek satırla kaydet.
// NEDEN: Program.cs'i temiz tutar; yeni repository/handler/validator eklenince
//        bu iki extension metodun içeriği değişir, burası değişmez.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// ADIM 5: JWT Authentication — REFERENCE/SECURITY.md §1, TECHNICAL_SPECIFICATIONS.md §5.
// NEDEN: Auth (A-03) henüz yazılmadı ama pipeline'ın doğru yerinde durması ve
//        [Authorize] öznitelikli endpoint'lerin ileride çalışması için burada kurulur.
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
            // NEDEN: Sunucu saatleri arasında tolerans tanımamak için sıfırlanır;
            //        access token 15dk gibi kısa ömürlü olduğundan gevşek tolerans gerekmez.
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ADIM 6: CORS — yalnızca appsettings'teki tanımlı origin'lere izin verir, "*" yok.
// NEDEN: REFERENCE/SECURITY.md §5 — tarayıcıdan yalnızca bilinen Web/Admin/Mobil
//        (Expo) adresleri istek atabilir.
builder.Services.AddCors(options => options.AddPolicy("Default", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
          .AllowAnyMethod()
          .AllowAnyHeader()));

// ADIM 7: Rate Limiting — REFERENCE/SECURITY.md §4: genel 100/dk (kimliği doğrulanmış
// istekler), 10/dk (anonim istekler). Controller/action'lar [EnableRateLimiting("...")]
// ile bu isimli policy'lerden birini seçer (A-03 — AuthController).
// NEDEN sabit pencere (FixedWindow): basit ve öngörülebilir; "login 5/15dk" ve
//       "OTP 3 yanlış" gibi BAŞARISIZ deneme sayaçları bundan ayrıdır — SecurityLog'a
//       bağımlı oldukları için A-04'ten sonraya bırakıldı (bkz. TASK/A_admin_panel_backend.md).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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

    // NEDEN IP başına PARTITIONED (yukarıdaki ikisinin aksine): QR generate
    // kötüye kullanılırsa (ör. toplu oturum açıp DB şişirme) tek bir saldırgan
    // IP'si TÜM kullanıcıların anonim limitini tüketmemeli — her IP kendi
    // 20/saat penceresine sahip olur (TASK/A_admin_panel_backend.md A-03.1).
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
});

var app = builder.Build();

// ADIM 8: İstek hattı (pipeline) — yalnızca geliştirmede Swagger arayüzü açılır.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VokabelMeister API v1"));
}

// NEDEN SIRALAMA: Loglama en dışta durur ki exception fırlasa bile (ExceptionHandlingMiddleware
// onu yakalayıp 500'e çevirse bile) gerçek süre ve nihai durum kodu loglanabilsin.
// Güvenlik başlıkları, hata yanıtı da dahil her yanıta eklenmesi için exception
// middleware'inden önce (dışında) durur.
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

// NEDEN UseAuthorization'dan SONRA: Rate limiter, [EnableRateLimiting] öznitelikli
// endpoint'e ulaşmadan hemen önce devreye girmeli — kimlik/yetki kontrolünden sonra,
// controller'dan hemen önce (A-03).
app.UseRateLimiter();

app.MapControllers();

app.Run();
