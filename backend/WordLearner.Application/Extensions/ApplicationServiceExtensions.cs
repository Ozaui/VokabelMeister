using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;

namespace WordLearner.Application.Extensions;

public static class ApplicationServiceExtensions
{
    // isDevelopment, IHostEnvironment yerine bool olarak geçilir — Application katmanı
    // Microsoft.Extensions.Hosting'e bağımlı olmamalı, tek ihtiyacı bu ikili karar.
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        bool isDevelopment
    )
    {
        var applicationAssembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddAutoMapper(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Geliştirmede OTP konsola yazılır (SMTP kurulumu gerekmez); üretimde DB'deki şifreli
        // ayarlarla gerçek gönderim yapılır.
        if (isDevelopment)
            services.AddScoped<IEmailService, DevEmailService>();
        else
            services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        // AddHttpClient<T> — AppleTokenValidator her doğrulamada Apple'ın JWKS'sini HTTP ile çeker;
        // IHttpClientFactory kullanmak soket tükenmesi riskini önler.
        services.AddHttpClient<IAppleTokenValidator, AppleTokenValidator>();

        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ILoginCompletionService, LoginCompletionService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<ISecurityLogger, SecurityLogger>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<ISmtpTestService, MailKitSmtpTestService>();
        services.AddScoped<IAccountCleanupService, AccountCleanupService>();

        return services;
    }
}
