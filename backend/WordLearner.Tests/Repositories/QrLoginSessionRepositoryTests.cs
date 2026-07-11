// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSessionRepositoryTests.cs
//
// AMAÇ: QrLoginSessionRepository'nin QrTokenHash'e göre arama sorgusunu gerçek bir
//       in-memory EF Core bağlamına karşı doğrulamak.
// NEDEN: bkz. UserRepositoryTests.cs dosya başı.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.QrLoginSessionRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class QrLoginSessionRepositoryTests
{
    /// <summary>
    /// GetByTokenHashAsync_RecordExists_ReturnsSession
    ///
    /// AMAÇ: Mutlu yol — hash'e göre QR oturumunun bulunduğunu doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetByTokenHashAsync_RecordExists_ReturnsSession()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new QrLoginSessionRepository(context);
        var eklenen = await repo.AddAsync(
            new QrLoginSession
            {
                QrTokenHash = "qr-hash-abc",
                PairingCode = "1234",
                ExpiresAt = DateTime.UtcNow.AddMinutes(2),
            }
        );

        // ACT
        var bulunan = await repo.GetByTokenHashAsync("qr-hash-abc");

        // ASSERT
        bulunan.Should().NotBeNull();
        bulunan!.Id.Should().Be(eklenen.Id);
    }

    /// <summary>
    /// GetByTokenHashAsync_NotFound_ReturnsNull
    ///
    /// AMAÇ: Eşleşen bir hash yoksa null döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetByTokenHashAsync_NotFound_ReturnsNull()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new QrLoginSessionRepository(context);

        // ACT
        var sonuc = await repo.GetByTokenHashAsync("hic-yok-hash");

        // ASSERT
        sonuc.Should().BeNull();
    }
}
