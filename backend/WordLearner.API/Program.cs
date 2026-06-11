/// <summary>
/// Program.cs
///
/// AMAÇ:
///   Uygulamanın giriş noktası. Tüm servisler, middleware'ler ve
///   yapılandırmalar burada kayıt edilir ve uygulama başlatılır.
///
/// NEDEN:
///   .NET 9 Minimal API yaklaşımı — Program.cs, Startup.cs yerine
///   tek dosyada servis kayıtlarını ve middleware pipeline'ını yönetir.
///
/// BAĞIMLILIKLAR:
///   - WordLearner.Application (servisler, MediatR komutlar)
///   - WordLearner.Infrastructure (veritabanı, repository'ler)
///   - Serilog (loglama)
///   - JWT (kimlik doğrulama)
/// </summary>
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WordLearner.Infrastructure.Extensions;

// ─── Serilog başlangıç logger'ı ───
// NEDEN: Uygulama tamamen başlamadan önce de hatalar oluşabilir, bu logger onları yakalar
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("WordLearner API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog'u ASP.NET Core host'una entegre et ───
    builder.Host.UseSerilog(
        (context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore.Database.Command",
                    Serilog.Events.LogEventLevel.Information
                )
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/wordlearner-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30
                )
    );

    // ─── Temel servisler ───
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(
            "v1",
            new()
            {
                Title = "WordLearner API",
                Version = "v1",
                Description = "Türkçe-Almanca kelime öğrenme uygulaması backend API'si",
            }
        );

        // Swagger'a JWT desteği — "Authorize" butonu çıkar
        options.AddSecurityDefinition(
            "Bearer",
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT token girin. Örnek: Bearer {token}",
            }
        );

        options.AddSecurityRequirement(
            new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    });

    // ─── JWT Kimlik Doğrulama ───
    // NEDEN: ASP.NET Identity kullanmıyoruz — JWT tamamen manuel yazılıyor
    //        Token doğrulama burada, token üretimi Application katmanında
    var jwtKey =
        builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey yapılandırılmamış!");

    builder
        .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                // NEDEN Zero: Token süresi dolunca anında geçersiz olsun, tolerans yok
                ClockSkew = TimeSpan.Zero,
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT doğrulama başarısız: {Hata}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Log.Debug(
                        "JWT token doğrulandı. Kullanıcı: {Kullanici}",
                        context.Principal?.Identity?.Name
                    );
                    return Task.CompletedTask;
                },
            };
        });

    builder.Services.AddAuthorization();

    // ─── CORS ───
    // NEDEN: Mobil (Expo) ve Admin panel farklı origin'lerden API'ye erişir
    //        Sadece izin verilen origin'lere izin verilir — * değil
    var allowedOrigins =
        builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
        options.AddPolicy(
            "Varsayilan",
            policy =>
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
        )
    );

    // ─── Infrastructure servisleri — DbContext + tüm repository'ler ───
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WordLearner API v1");
            options.RoutePrefix = string.Empty; // Swagger kök URL'de açılsın
        });
    }

    // NEDEN bu sıra önemli: Serilog → HTTPS → CORS → Auth → Authorization → Controllers
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0000} ms)";
    });

    app.UseHttpsRedirection();
    app.UseCors("Varsayilan");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("WordLearner API hazır. Ortam: {Ortam}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WordLearner API başlatılamadı!");
}
finally
{
    Log.CloseAndFlush();
}
