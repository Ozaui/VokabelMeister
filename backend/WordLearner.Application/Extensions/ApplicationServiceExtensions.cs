// ─────────────────────────────────────────────────────────────────────────────
// ApplicationServiceExtensions.cs
//
// AMAÇ: Application katmanına ait tüm servisleri (MediatR, AutoMapper, FluentValidation)
//       DI konteynerine kaydetmek için Program.cs'den çağrılan tek extension metot.
// NEDEN: Program.cs'i temiz tutar; InfrastructureServiceExtensions'ın Application
//        katmanındaki karşılığıdır — yeni bir validator/handler/mapping profili
//        eklenince yalnızca assembly taraması sayesinde otomatik kaydolur, Program.cs'e dokunulmaz.
// BAĞIMLILIKLAR: MediatR, AutoMapper, FluentValidation.DependencyInjectionExtensions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;

namespace WordLearner.Application.Extensions;

public static class ApplicationServiceExtensions
{
    // AMAÇ: MediatR handler'larını, AutoMapper profillerini ve FluentValidation
    //       validator'larını Application katmanı assembly'sinden tarayıp kaydeder.
    // NEDEN: Somut bir tip yerine bu extension'ın bulunduğu assembly referans alınır;
    //        A-03'teki 13 Auth Command+Handler'ı (Application/Features/Auth/) ve
    //        gelecekteki her yeni CreateWordCommand/CreateWordCommandValidator vb.
    //        otomatik olarak bulunur, tek tek kayıt gerekmez.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationServiceExtensions).Assembly;

        // NEDEN MediatR: Controller'lar servisleri doğrudan çağırmak yerine
        //       IMediator.Send(command/query) ile çağırır; controller ince katman kalır.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

        // NEDEN AutoMapper: Entity ↔ DTO dönüşümlerini elle yazmak yerine profil
        //       sınıflarına (Profile) devreder; her feature kendi profilini ekler.
        services.AddAutoMapper(applicationAssembly);

        // NEDEN FluentValidation: Request DTO'larının doğrulama kuralları (RuleFor)
        //       controller'a girmeden, ASP.NET Core'un model binding aşamasında çalışır.
        services.AddValidatorsFromAssembly(applicationAssembly);

        // NEDEN Scoped: Stateless bir servis olduğu için Singleton da olabilirdi, ama
        //       diğer Application servisleriyle (DbContext'e bağımlı olacaklar) tutarlı
        //       yaşam süresi için Scoped seçildi (A-03 — Auth API).
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, DevEmailService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        // NEDEN AddHttpClient<T>: AppleTokenValidator her doğrulamada Apple'ın JWKS'sini
        //       (https://appleid.apple.com/auth/keys) HTTP ile çeker — tek tek `new HttpClient()`
        //       yerine IHttpClientFactory kullanmak soket tükenmesi (socket exhaustion) riskini
        //       önler ve testlerde HttpClient'ın mock'lanmasını kolaylaştırır.
        services.AddHttpClient<IAppleTokenValidator, AppleTokenValidator>();

        // NEDEN Scoped: Auth Command Handler'larının (Application/Features/Auth/) paylaştığı
        //       OTP üretimi/doğrulanması ve login tamamlama mantığı — handler'lar MediatR
        //       üzerinden birbirini çağıramadığı için bu iki servise çıkarıldı.
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ILoginCompletionService, LoginCompletionService>();

        // NEDEN Scoped: DbContext bağımlı repository'lerle aynı yaşam süresi; A-04 — Loglama Sistemi.
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<ISecurityLogger, SecurityLogger>();

        // NEDEN Scoped: Stateless bir servis (Singleton da olabilirdi), ama diğer Application
        //       servisleriyle tutarlı yaşam süresi için Scoped seçildi (A-08 — Medya API).
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
