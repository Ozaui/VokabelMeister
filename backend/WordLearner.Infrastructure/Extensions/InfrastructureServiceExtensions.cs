using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Infrastructure.Data;
using WordLearner.Infrastructure.Repositories;

namespace WordLearner.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<WordLearnerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        // Scoped — DbContext ile aynı yaşam süresinde olmalı (request başına bir instance).
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IQrLoginSessionRepository, QrLoginSessionRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
        services.AddScoped<ISecurityLogRepository, SecurityLogRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IWordConceptRepository, WordConceptRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISmtpSettingsRepository, SmtpSettingsRepository>();

        return services;
    }
}
