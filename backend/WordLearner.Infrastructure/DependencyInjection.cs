using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Repositories.Logging;
using WordLearner.Infrastructure.Data;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Infrastructure.Repositories.Auth;
using WordLearner.Infrastructure.Repositories.Logging;

namespace WordLearner.Infrastructure;

// Repo + DbContext kaydı TEK bu metotta toplanır — Program.cs iki ayrı çağrı yerine tek satır yazar.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WordLearnerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IQrLoginSessionRepository, QrLoginSessionRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ISecurityLogRepository, SecurityLogRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();

        return services;
    }
}
