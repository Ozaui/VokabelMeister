using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;

namespace WordLearner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ILoginCompletionService, LoginCompletionService>();

        return services;
    }
}
