using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;

namespace WordLearner.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddAutoMapper(applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, DevEmailService>();
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

        return services;
    }
}
