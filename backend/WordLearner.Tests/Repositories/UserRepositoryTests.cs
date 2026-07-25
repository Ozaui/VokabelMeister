using FluentAssertions;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetByEmailAsync_ActiveUser_ReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User { Email = "aktif@example.com", FirstName = "A", LastName = "B" }
        );

        // ACT
        var bulunan = await repo.GetByEmailAsync("aktif@example.com");

        // ASSERT
        bulunan.Should().NotBeNull();
        bulunan!.Id.Should().Be(eklenen.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_SoftDeletedUser_StillReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User { Email = "silinen@example.com", FirstName = "A", LastName = "B" }
        );
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT
        var bulunan = await repo.GetByEmailAsync("silinen@example.com");

        // ASSERT
        bulunan.Should().NotBeNull();
        bulunan!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_NotFound_ReturnsNull()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);

        // ACT
        var sonuc = await repo.GetByEmailAsync("yok@example.com");

        // ASSERT
        sonuc.Should().BeNull();
    }

    [Fact]
    public async Task GetByGoogleIdAsync_SoftDeletedUser_StillReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User
            {
                Email = "google@example.com",
                GoogleId = "google-sub-123",
                FirstName = "A",
                LastName = "B",
            }
        );
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT
        var bulunan = await repo.GetByGoogleIdAsync("google-sub-123");

        // ASSERT
        bulunan.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAppleIdAsync_SoftDeletedUser_StillReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User
            {
                Email = "apple@example.com",
                AppleId = "apple-sub-456",
                FirstName = "A",
                LastName = "B",
            }
        );
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT
        var bulunan = await repo.GetByAppleIdAsync("apple-sub-456");

        // ASSERT
        bulunan.Should().NotBeNull();
    }

    [Fact]
    public async Task OriginalEmailHashExistsAsync_HashMatchesAnonymizedUser_ReturnsTrue()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User
            {
                Email = "deleted_1@deleted.invalid",
                OriginalEmailHash = "hash-of-original-email",
                IsAnonymized = true,
                FirstName = "Silindi",
                LastName = "Silindi",
            }
        );
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT
        var sonuc = await repo.OriginalEmailHashExistsAsync("hash-of-original-email");

        // ASSERT
        sonuc.Should().BeTrue();
    }

    [Fact]
    public async Task OriginalEmailHashExistsAsync_NoMatch_ReturnsFalse()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);

        // ACT
        var sonuc = await repo.OriginalEmailHashExistsAsync("hic-eslesmeyen-hash");

        // ASSERT
        sonuc.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_SoftDeletedUser_StillReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(
            new User { Email = "qr@example.com", FirstName = "A", LastName = "B" }
        );
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT — hem filtreli hem filtresiz sorguyu karşılaştır
        var filtreliSonuc = await repo.GetByIdAsync(eklenen.Id);
        var filtresizSonuc = await repo.GetByIdIncludingDeletedAsync(eklenen.Id);

        // ASSERT — GetByIdAsync (taban sınıf) soft-delete'li kaydı GÖRMEMELİ,
        //          GetByIdIncludingDeletedAsync GÖRMELİ — bu ikilik, bug'ın kendisiydi.
        filtreliSonuc.Should().BeNull();
        filtresizSonuc.Should().NotBeNull();
        filtresizSonuc!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_NotFound_ReturnsNull()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);

        // ACT
        var sonuc = await repo.GetByIdIncludingDeletedAsync(999);

        // ASSERT
        sonuc.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_SearchAndRoleFilter_ReturnsMatchingUsersOnly()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        await repo.AddAsync(new User { Email = "ada@example.com", FirstName = "Ada", LastName = "Lovelace", Role = "Admin" });
        await repo.AddAsync(new User { Email = "grace@example.com", FirstName = "Grace", LastName = "Hopper", Role = "User" });
        await repo.AddAsync(new User { Email = "ada2@example.com", FirstName = "Ada", LastName = "Byron", Role = "User" });

        // ACT
        var sonuc = await repo.GetPagedAsync("ada", "User", 1, 20);

        // ASSERT
        sonuc.TotalCount.Should().Be(1);
        sonuc.Items.Should().ContainSingle(u => u.Email == "ada2@example.com");
    }

    [Fact]
    public async Task GetPagedAsync_SoftDeletedUser_ExcludedFromList()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eklenen = await repo.AddAsync(new User { Email = "silinen@example.com", FirstName = "A", LastName = "B" });
        await repo.SoftDeleteAsync(eklenen.Id);

        // ACT
        var sonuc = await repo.GetPagedAsync(null, null, 1, 20);

        // ASSERT
        sonuc.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_MixOfActiveAndFrozen_ReturnsCorrectCounts()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        await repo.AddAsync(new User { Email = "aktif1@example.com", FirstName = "A", LastName = "B", IsActive = true });
        await repo.AddAsync(new User { Email = "aktif2@example.com", FirstName = "A", LastName = "B", IsActive = true });
        await repo.AddAsync(new User { Email = "donuk@example.com", FirstName = "A", LastName = "B", IsActive = false });

        // ACT
        var (total, active, frozen) = await repo.GetStatisticsAsync();

        // ASSERT
        total.Should().Be(3);
        active.Should().Be(2);
        frozen.Should().Be(1);
    }

    [Fact]
    public async Task GetRegistrationDatesAsync_OnlyReturnsDatesWithinWindow()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var eskiKullanici = await repo.AddAsync(new User { Email = "eski@example.com", FirstName = "A", LastName = "B" });
        eskiKullanici.CreatedAt = DateTime.UtcNow.AddDays(-100);
        await repo.UpdateAsync(eskiKullanici);
        await repo.AddAsync(new User { Email = "yeni@example.com", FirstName = "A", LastName = "B" });

        // ACT
        var sonuc = await repo.GetRegistrationDatesAsync(DateTime.UtcNow.Date.AddDays(-30));

        // ASSERT
        sonuc.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPendingAnonymizationAsync_GracePeriodExpired_ReturnsUser()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var kullanici = await repo.AddAsync(
            new User { Email = "suresi-dolan@example.com", FirstName = "A", LastName = "B" }
        );
        kullanici.IsDeleted = true;
        kullanici.ScheduledDeletionAt = DateTime.UtcNow.AddDays(-1);
        await repo.UpdateAsync(kullanici);

        // ACT
        var sonuc = await repo.GetPendingAnonymizationAsync(DateTime.UtcNow);

        // ASSERT — soft delete filtresi yok sayılmazsa silinmiş hesap hiç bulunamazdı.
        sonuc.Should().ContainSingle().Which.Id.Should().Be(kullanici.Id);
    }

    [Fact]
    public async Task GetPendingAnonymizationAsync_GracePeriodStillRunning_ReturnsEmpty()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var kullanici = await repo.AddAsync(
            new User { Email = "grace@example.com", FirstName = "A", LastName = "B" }
        );
        kullanici.IsDeleted = true;
        kullanici.ScheduledDeletionAt = DateTime.UtcNow.AddDays(20);
        await repo.UpdateAsync(kullanici);

        // ACT
        var sonuc = await repo.GetPendingAnonymizationAsync(DateTime.UtcNow);

        // ASSERT
        sonuc.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingAnonymizationAsync_AlreadyAnonymized_ReturnsEmpty()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        var kullanici = await repo.AddAsync(
            new User { Email = "anonim@example.com", FirstName = "A", LastName = "B" }
        );
        kullanici.IsDeleted = true;
        kullanici.IsAnonymized = true;
        kullanici.ScheduledDeletionAt = DateTime.UtcNow.AddDays(-40);
        await repo.UpdateAsync(kullanici);

        // ACT — aksi hâlde aynı hesap her gece tekrar tekrar anonimleştirilir, her seferinde log yazılırdı.
        var sonuc = await repo.GetPendingAnonymizationAsync(DateTime.UtcNow);

        // ASSERT
        sonuc.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingAnonymizationAsync_ActiveUser_ReturnsEmpty()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new UserRepository(context);
        await repo.AddAsync(new User { Email = "aktif2@example.com", FirstName = "A", LastName = "B" });

        // ACT
        var sonuc = await repo.GetPendingAnonymizationAsync(DateTime.UtcNow);

        // ASSERT
        sonuc.Should().BeEmpty();
    }
}
