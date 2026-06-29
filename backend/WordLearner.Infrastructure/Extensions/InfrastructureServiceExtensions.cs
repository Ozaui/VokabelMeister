// ─────────────────────────────────────────────────────────────────────────────
// InfrastructureServiceExtensions.cs
//
// AMAÇ: Infrastructure katmanına ait tüm servisleri DI konteynerine kaydetmek için
//       Program.cs'den çağrılan tek extension metot.
// NEDEN: Program.cs'i temiz tutar; yeni bir repository veya servis eklenince
//        yalnızca bu dosya değişir, Program.cs'e dokunulmaz.
// BAĞIMLILIKLAR: WordLearnerDbContext, Microsoft.EntityFrameworkCore.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    // AMAÇ: DbContext ve infrastructure servislerini IServiceCollection'a ekler.
    // NEDEN: Program.cs'de tek satır (builder.Services.AddInfrastructureServices(config))
    //        ile tüm altyapı hazır hâle gelir; composition root temiz kalır.
    // NASIL: Configuration'dan connection string okunur, DbContext Scoped olarak kaydedilir.
    //        Yeni repository'ler ilerleyen task'larda (A-03+) buraya eklenir.
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // NEDEN Scoped: DbContext request başına bir instance olmalı;
        //       Singleton olsaydı eş zamanlı istekler aynı context'i paylaşırdı (thread-safety sorunu).
        services.AddDbContext<WordLearnerDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // NOT: Feature repository'ler (IUserRepository, IWordRepository vb.) kendi task'larında
        //      (A-03, A-05 ...) bu metoda eklenecek.

        return services;
    }
}
