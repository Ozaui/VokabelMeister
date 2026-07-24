using FluentAssertions;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class QrLoginSessionRepositoryTests
{
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
