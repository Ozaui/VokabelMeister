// ─────────────────────────────────────────────────────────────────────────────
// UserRepositoryTests.cs
//
// AMAÇ: UserRepository'nin User'a özgü sorgularını (özellikle IgnoreQueryFilters
//       kullanan dört metodu) gerçek bir in-memory EF Core bağlamına karşı doğrulamak.
// NEDEN: Kod kalitesi denetiminde bulunan bir boşluk kapatılıyor — bu sorguların
//        hiçbiri daha önce gerçek/in-memory DB'ye karşı test edilmiyordu (yalnızca
//        Handler testlerinde Mock<IUserRepository> ile taklit ediliyordu); bu, kritik
//        bir detayın (IgnoreQueryFilters) yanlışlıkla silinmesi durumunda hiçbir testin
//        bunu yakalayamayacağı anlamına geliyordu (bkz. Auth_Domain.md "bugfix turu" notu —
//        GetQrLoginStatusCommand'daki gerçek bug tam olarak bu sınıfta EKSİK olan bir metottan
//        kaynaklanmıştı). Odak: soft-delete'li bir kullanıcının GERÇEKTEN bulunabildiğini
//        (grace-period kurtarma/hesap kurtarma akışlarının dayandığı davranış) kanıtlamak.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.UserRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class UserRepositoryTests
{
    /// <summary>
    /// GetByEmailAsync_ActiveUser_ReturnsUser
    ///
    /// AMAÇ: Mutlu yol — soft-delete edilmemiş bir kullanıcının e-postayla bulunduğunu doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetByEmailAsync_SoftDeletedUser_StillReturnsUser
    ///
    /// AMAÇ: Soft-delete edilmiş (grace period içindeki) bir kullanıcının GetByEmailAsync
    ///       ile hâlâ bulunabildiğini doğrulamak — IgnoreQueryFilters() olmadan bu metot
    ///       WordLearnerDbContext'in global soft-delete filtresi yüzünden null dönerdi.
    /// NEDEN kritik: Bu davranış olmadan hesap kurtarma (LoginCompletionService'in
    ///       IsDeleted→false çevirmesi) ve "e-posta zaten kullanımda" kontrolü çalışamaz —
    ///       kullanıcı hesabını sildikten sonra AYNI e-postayla tekrar giriş/kayıt denediğinde
    ///       sistem onu hiç görmemiş gibi davranırdı.
    /// </summary>
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

    /// <summary>
    /// GetByEmailAsync_NotFound_ReturnsNull
    ///
    /// AMAÇ: Hiç kayıtlı olmayan bir e-posta için null döndüğünü (exception fırlatılmadığını) doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetByGoogleIdAsync_SoftDeletedUser_StillReturnsUser
    ///
    /// AMAÇ: GoogleId ile aramanın da (GetByEmailAsync ile aynı gerekçeyle) soft-delete
    ///       filtresini yok saydığını doğrulamak — aksi hâlde Google ile tekrar giriş
    ///       deneyen, hesabını silmiş bir kullanıcı için LoginWithGoogleCommand yanlışlıkla
    ///       "hesap yok" sanıp YENİ bir hesap açardı (aynı GoogleId ile ikinci kayıt = veri bütünlüğü ihlali).
    /// </summary>
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

    /// <summary>
    /// GetByAppleIdAsync_SoftDeletedUser_StillReturnsUser
    ///
    /// AMAÇ: AppleId ile aramanın da soft-delete filtresini yok saydığını doğrulamak
    ///       (bkz. GetByGoogleIdAsync_SoftDeletedUser_StillReturnsUser — aynı gerekçe, Apple girişi).
    /// </summary>
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

    /// <summary>
    /// OriginalEmailHashExistsAsync_HashMatchesAnonymizedUser_ReturnsTrue
    ///
    /// AMAÇ: Daha önce anonimleştirilmiş (30 gün grace period sonrası PII temizlenmiş) bir
    ///       kullanıcının OriginalEmailHash'i eşleşiyorsa true döndüğünü doğrulamak — bu
    ///       kayıt zaten soft-delete'li olduğu için IgnoreQueryFilters() şart.
    /// NEDEN kritik: Bu kontrol olmadan, silinip anonimleştirilmiş bir hesabın eski
    ///       e-postasıyla RegisterCommand'da sınırsızca tekrar kayıt açılabilirdi
    ///       (REFERENCE/SECURITY.md §9 ihlali).
    /// </summary>
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

    /// <summary>
    /// OriginalEmailHashExistsAsync_NoMatch_ReturnsFalse
    ///
    /// AMAÇ: Eşleşen bir kayıt yoksa false döndüğünü (mutlu yol — yeni kayıt engellenmez) doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetByIdIncludingDeletedAsync_SoftDeletedUser_StillReturnsUser
    ///
    /// AMAÇ: Id'ye göre aramanın da (GetByIdAsync'in aksine) soft-delete filtresini yok
    ///       saydığını doğrulamak — 2026-07-11'de QR ile giriş bugfix'inde eklenen metot,
    ///       taban Repository&lt;T&gt;.GetByIdAsync'in filtreli olması yüzünden
    ///       GetQrLoginStatusCommand'ın grace-period kurtarmaya hiç ulaşamadığı gerçek bug'ı düzeltti.
    /// </summary>
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

    /// <summary>
    /// GetByIdIncludingDeletedAsync_NotFound_ReturnsNull
    ///
    /// AMAÇ: Olmayan bir Id için exception değil null döndüğünü doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_SearchAndRoleFilter_ReturnsMatchingUsersOnly
    ///
    /// AMAÇ: A-07 admin liste ekranı — search (Email/FirstName/LastName) VE role
    ///       filtresinin BİRLİKTE uygulandığını, eşleşmeyen kayıtların dönmediğini doğrulamak.
    /// </summary>
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

    /// <summary>
    /// GetPagedAsync_SoftDeletedUser_ExcludedFromList
    ///
    /// AMAÇ: Admin genel listenin soft-delete'li/anonimleştirilmiş hesapları GÖRMEDİĞİNİ
    ///       doğrulamak — GetByEmailAsync'in aksine burada IgnoreQueryFilters YOK (bilinçli
    ///       tercih, bkz. IUserRepository.GetPagedAsync "NEDEN").
    /// </summary>
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

    /// <summary>
    /// GetStatisticsAsync_MixOfActiveAndFrozen_ReturnsCorrectCounts
    /// </summary>
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

    /// <summary>
    /// GetRegistrationDatesAsync_OnlyReturnsDatesWithinWindow
    ///
    /// AMAÇ: `fromUtc`'den ÖNCEKİ bir kaydın (dışarıda kalması gereken) listeye
    ///       SIZMADIĞINI, penceredeki kayıtların HAM (gruplanmamış) döndüğünü doğrulamak
    ///       — gruplama Handler'ın sorumluluğu (bkz. IUserRepository "NEDEN" notu).
    /// </summary>
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
}
