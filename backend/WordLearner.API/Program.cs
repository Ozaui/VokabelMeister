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
// NOT (A-04 — Loglama Sistemi): Serilog'un MSSqlServer (ApplicationLog) sink'i eklendi
//   (ActivityLog/SecurityLog Serilog ile DEĞİL, IActivityLogger/ISecurityLogger ile yazılır).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using WordLearner.API.Filters;
using WordLearner.API.Logging;
using WordLearner.API.Middleware;
using WordLearner.Application.Extensions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Logging;
using WordLearner.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ADIM 1: Serilog — konsol + dosya + DB (ApplicationLog) sink.
// NEDEN: _logger.LogInformation/.LogError çağrıları artık ASP.NET Core'un varsayılan
//        konsol logger'ı yerine Serilog üzerinden akar.
// NEDEN Override: appsettings.json'daki Logging:LogLevel:Microsoft.AspNetCore=Warning
//        ayarı yalnızca ASP.NET Core'un kendi builtin logger'ı içindir; Serilog kod
//        üzerinden yapılandırıldığı için aynı susturma burada elle tekrarlanır —
//        aksi halde framework'ün "Request starting/finished" logları RequestResponseLoggingMiddleware
//        ile çakışıp konsolu ikiye katlar.
// NEDEN AutoCreateSqlTable=false: Tablo zaten AddLoggingTables migration'ıyla
//        (ApplicationLogConfiguration'daki gerçek şemayla) oluşturuldu; sink'in kendi
//        varsayılan şemasıyla tekrar oluşturmaya çalışması migration'la çakışır.
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
//       "OTP 3 yanlış" gibi BAŞARISIZ deneme sayaçları AYRI bir mekanizma (OtpService/
//       LoginCommandHandler'ın kendi SecurityLog tabanlı sayaçları — henüz yazılmadı,
//       bu bilinçli bir sonraki adım) — burada eklenen yalnızca genel RateLimitHit
//       (429 döndüren HERHANGİ bir policy) A-04'te SecurityLog'a bağlandı.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // NEDEN OnRejected (A-04): Herhangi bir policy (anonymous/authenticated/qrGenerate/
    //       qrStatus) 429 döndürdüğünde SecurityLog'a RateLimitHit yazılır — DI konteynerine
    //       burada (middleware pipeline dışı bir yapılandırma callback'i) doğrudan erişim
    //       olmadığı için RequestServices üzerinden scope içi ISecurityLogger çözülür.
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

    // NEDEN IP başına PARTITIONED ("anonymous"ın aksine): GET /auth/qr/{token}/status
    // web tarafından ~2sn'de bir çağrılan bir polling endpoint'i (~30 istek/dk).
    // Paylaşımlı "anonymous" limitini (10/dk, TÜM anonim trafik ortak) kullanırsa
    // tek bir açık QR ekranı ~20 saniye içinde bu bütçeyi tüketip sunucudaki TÜM
    // anonim kullanıcıları (register/login/forgot-password dahil) kilitler — polling
    // kendi kendini ve başkalarını kilitlemiş olurdu. IP başına ayrı pencere (qrGenerate
    // ile aynı gerekçe) bu yan etkiyi önler; limit polling hızının (30/dk) üstünde
    // bir tampon payıyla belirlendi.
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

// ADIM 8: İstek hattı (pipeline) — yalnızca geliştirmede Swagger arayüzü açılır.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VokabelMeister API v1"));
}

// NEDEN UseHttpsRedirection EN BAŞTA (2026-07-12'de düzeltildi — önceden loglama/güvenlik
// başlıkları/exception middleware'lerinden SONRA çağrılıyordu): düz HTTP ile gelen bir
// istek hiçbir iş yapılmadan (loglanmadan, başlık eklenmeden) doğrudan HTTPS'e yönlendirilmeli
// — ASP.NET Core'un standart konvansiyonu budur. Bu proje geliştirme ortamında pratikte nadiren
// tetiklenir (prod'da genelde ters proxy TLS'i sonlandırır) ama sıralama artık konvansiyona uygun.
app.UseHttpsRedirection();

// NEDEN SIRALAMA: Loglama en dışta durur ki exception fırlasa bile (ExceptionHandlingMiddleware
// onu yakalayıp 500'e çevirse bile) gerçek süre ve nihai durum kodu loglanabilsin.
// Güvenlik başlıkları, hata yanıtı da dahil her yanıta eklenmesi için exception
// middleware'inden önce (dışında) durur.
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// NEDEN AUTH'TAN ÖNCE: /uploads altındaki dosyalar (kelime kartı görselleri) herkese
// açık statik varlıklar — REFERENCE/ENV.md §7. `wwwroot/uploads` (LocalFileStorageService'in
// yazdığı klasör) varsayılan `UseStaticFiles()` konvansiyonuyla `/uploads` yolunda servis
// edilir; [Authorize] gerektiren API rotalarından TAMAMEN ayrı bir istek hattı (auth/rate
// limiter bu isteklere hiç uğramaz — görsel görüntülemek için token gerekmemeli).
app.UseStaticFiles();

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

// NEDEN UseAuthorization'dan SONRA: Rate limiter, [EnableRateLimiting] öznitelikli
// endpoint'e ulaşmadan hemen önce devreye girmeli — kimlik/yetki kontrolünden sonra,
// controller'dan hemen önce (A-03).
app.UseRateLimiter();

app.MapControllers();

app.Run();
