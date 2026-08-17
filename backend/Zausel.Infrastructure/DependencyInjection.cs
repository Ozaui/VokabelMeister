using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zausel.Application.Interfaces.Repositories;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Infrastructure.Data;
using Zausel.Infrastructure.Repositories;
using Zausel.Infrastructure.Repositories.Auth;
using Zausel.Infrastructure.Repositories.Content;
using Zausel.Infrastructure.Repositories.Logging;

namespace Zausel.Infrastructure;

// Repo + DbContext kaydı TEK bu metotta toplanır — Program.cs iki ayrı çağrı yerine tek satır yazar.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ZauselDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IQrLoginSessionRepository, QrLoginSessionRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ISecurityLogRepository, SecurityLogRepository>();
        services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IWordConceptRepository, WordConceptRepository>();

        return services;
    }
}
