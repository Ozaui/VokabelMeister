using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Infrastructure.Data;
using WordLearner.Infrastructure.Repositories;

namespace WordLearner.Infrastructure;

// Repo + DbContext kaydı TEK bu metotta toplanır — Program.cs iki ayrı çağrı yerine tek satır
// yazar; A-04'te ActivityLogger/SecurityLogger kaydı da buraya eklenecek (aynı gerekçe: Infrastructure
// katmanının DI kaydı Infrastructure'ın kendi sorumluluğu, Program.cs'e sızmaz).
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WordLearnerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
