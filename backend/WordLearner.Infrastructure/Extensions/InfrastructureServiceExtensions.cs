/// <summary>
/// InfrastructureServiceExtensions.cs
///
/// AMAÇ: Infrastructure katmanındaki tüm servisleri DI container'a tek metotla kaydetmek.
/// NEDEN: Program.cs'i kalabalıklaştırmamak için; Infrastructure değiştiğinde API katmanına dokunulmaz.
/// BAĞIMLILIKLAR: Tüm Repository implementasyonları, WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Infrastructure.Data;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Infrastructure.Repositories.Auth;
using WordLearner.Infrastructure.Repositories.Learning;
using WordLearner.Infrastructure.Repositories.Social;
using WordLearner.Infrastructure.Repositories.UserContent;
using WordLearner.Infrastructure.Repositories.Vocabulary;

namespace WordLearner.Infrastructure.Extensions;

/// <summary>
/// Infrastructure DI kayıt extension'ı.
///
/// AMAÇ: Program.cs'de tek satırla tüm Infrastructure servislerini kaydetmek.
/// NEDEN: IServiceCollection extension deseni — API katmanı Infrastructure'ı doğrudan tanımaz.
/// NASIL: builder.Services.AddInfrastructureServices(builder.Configuration) şeklinde çağrılır.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// AMAÇ: DbContext ve tüm repository'leri DI container'a kaydeder.
    /// NEDEN: Scoped lifetime — her HTTP isteği için ayrı instance; transaction bütünlüğü korunur.
    /// NASIL: Program.cs'de builder.Services.AddInfrastructureServices(builder.Configuration) ile çağrılır.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── DbContext ────────────────────────────────────────────────────────
        services.AddDbContext<WordLearnerDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("WordLearner.Infrastructure")
            )
        );

        // ─── Auth Repository'leri ─────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ─── Vocabulary Repository'leri ───────────────────────────────────────
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ─── UserContent Repository'leri ──────────────────────────────────────
        services.AddScoped<IUserCardRepository, UserCardRepository>();
        services.AddScoped<IUserCategoryRepository, UserCategoryRepository>();

        // ─── Learning Repository'leri ─────────────────────────────────────────
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<IUserCardProgressRepository, UserCardProgressRepository>();

        // ─── Social Repository'leri ───────────────────────────────────────────
        services.AddScoped<IClassRepository, ClassRepository>();
        // Instructor'ın sınıfa özel kelime repository'si — sistem Words'ten bağımsız
        services.AddScoped<IClassWordRepository, ClassWordRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<ISharedContentRepository, SharedContentRepository>();

        return services;
    }
}
