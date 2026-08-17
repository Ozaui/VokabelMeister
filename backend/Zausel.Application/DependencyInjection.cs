using Microsoft.Extensions.DependencyInjection;
using Zausel.Application.Interfaces.Services;
using Zausel.Application.Services;

namespace Zausel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ILoginCompletionService, LoginCompletionService>();
        services.AddScoped<IEmailService, DevEmailService>(); // prod: A-20'de SmtpEmailService
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddSingleton<HttpClient>(); // AppleTokenValidator'ın JWKS isteği için
        services.AddScoped<IAppleTokenValidator, AppleTokenValidator>();
        services.AddScoped<IAdminSeedService, AdminSeedService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<ISecurityLogger, SecurityLogger>();

        return services;
    }
}
