// ─────────────────────────────────────────────────────────────────────────────
// InMemoryDbContextFactory.cs
//
// AMAÇ: Feature repository testlerinde (UserRepository, RefreshTokenRepository,
//       QrLoginSessionRepository) kullanılan, izole bir in-memory WordLearnerDbContext üretir.
// NEDEN: RepositoryTests.cs'teki CreateContext() yalnızca TestEntity için özel bir
//        TestDbContext üretiyordu (Repository<T> taban sınıfını gerçek entity'ler
//        olmadan test etmek için); feature repository'lerin gerçek sorguları
//        (IgnoreQueryFilters dahil) doğrudan gerçek WordLearnerDbContext'e karşı
//        çalıştırılmalı — bu üç test dosyasının paylaştığı ortak kurulum burada.
// BAĞIMLILIKLAR: Microsoft.EntityFrameworkCore.InMemory, WordLearnerDbContext.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Tests.Common;

internal static class InMemoryDbContextFactory
{
    // AMAÇ: Her çağrıda benzersiz isimli, izole bir in-memory veritabanı bağlamı üretir.
    // NEDEN: Testler paralel/sırayla çalışırken aynı veritabanı adını paylaşırsa
    //        kayıtlar birbirine karışır; Guid ile her test kendi temiz DB'sinde çalışır.
    public static WordLearnerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WordLearnerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WordLearnerDbContext(options);
    }
}
