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
