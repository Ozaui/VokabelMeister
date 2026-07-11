// ─────────────────────────────────────────────────────────────────────────────
// RefreshTokenRepositoryTests.cs
//
// AMAÇ: RefreshTokenRepository'nin Token Family Pattern'i destekleyen sorgularını
//       (hash arama, family/kullanıcı bazlı toplu iptal) gerçek bir in-memory EF Core
//       bağlamına karşı doğrulamak.
// NEDEN: bkz. UserRepositoryTests.cs dosya başı — daha önce bu sorgular yalnızca
//        Handler testlerinde mock'lanıyordu, gerçek LINQ ifadelerinin (Where filtreleri)
//        doğru kayıtları seçtiği hiç doğrulanmamıştı.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.RefreshTokenRepository.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Infrastructure.Repositories;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Repositories;

public class RefreshTokenRepositoryTests
{
    /// <summary>
    /// GetByTokenHashAsync_RecordExists_ReturnsToken
    ///
    /// AMAÇ: Mutlu yol — hash'e göre token'ın bulunduğunu doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetByTokenHashAsync_RecordExists_ReturnsToken()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new RefreshTokenRepository(context);
        var eklenen = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 1,
                TokenHash = "hash-abc",
                TokenFamily = "family-1",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        // ACT
        var bulunan = await repo.GetByTokenHashAsync("hash-abc");

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
        var repo = new RefreshTokenRepository(context);

        // ACT
        var sonuc = await repo.GetByTokenHashAsync("hic-yok-hash");

        // ASSERT
        sonuc.Should().BeNull();
    }

    /// <summary>
    /// RevokeFamilyAsync_MultipleTokensInFamily_RevokesOnlyUnrevokedOnesInThatFamily
    ///
    /// AMAÇ: Aynı TokenFamily'deki TÜM iptal edilmemiş token'ların RevokedAt'inin
    ///       set edildiğini; BAŞKA bir family'deki token'ın VE zaten iptal edilmiş
    ///       (RevokedAt dolu) bir token'ın üzerine yazılmadığını doğrulamak.
    /// NEDEN kritik: Token Family Pattern'in replay savunmasının tam kalbi budur —
    ///       yanlış family'yi iptal etmek ya sahte kullanıcıyı dışarıda bırakır ya da
    ///       gerçek kullanıcıyı gereksiz yere tüm cihazlardan atar.
    /// </summary>
    [Fact]
    public async Task RevokeFamilyAsync_MultipleTokensInFamily_RevokesOnlyUnrevokedOnesInThatFamily()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new RefreshTokenRepository(context);
        var eskiIptalZamani = DateTime.UtcNow.AddDays(-1);
        var ayniFamilyIptalEdilmemis = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 1,
                TokenHash = "hash-1",
                TokenFamily = "family-replay",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );
        var ayniFamilyZatenIptal = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 1,
                TokenHash = "hash-2",
                TokenFamily = "family-replay",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RevokedAt = eskiIptalZamani,
            }
        );
        var farkliFamily = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 2,
                TokenHash = "hash-3",
                TokenFamily = "family-baska",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        // ACT
        await repo.RevokeFamilyAsync("family-replay");

        // ASSERT
        (await repo.GetByIdAsync(ayniFamilyIptalEdilmemis.Id))!.RevokedAt.Should().NotBeNull();
        // NEDEN: Zaten iptal edilmiş token'ın RevokedAt'i (sorgu RevokedAt==null şartı taşıdığı
        //        için) DEĞİŞMEMELİ — replay'in GERÇEKTE ne zaman tespit edildiği kaybolmamalı.
        (await repo.GetByIdAsync(ayniFamilyZatenIptal.Id))!.RevokedAt.Should().Be(eskiIptalZamani);
        (await repo.GetByIdAsync(farkliFamily.Id))!.RevokedAt.Should().BeNull();
    }

    /// <summary>
    /// RevokeAllForUserAsync_MultipleUsers_RevokesOnlyThatUsersTokens
    ///
    /// AMAÇ: Bir kullanıcının (hangi family'den olursa olsun) tüm token'larının iptal
    ///       edildiğini; BAŞKA bir kullanıcının token'ına dokunulmadığını doğrulamak.
    /// NEDEN kritik: Şifre sıfırlama/hesap silme sonrası "tüm cihazlardan çıkış" — yanlış
    ///       kullanıcının token'ları iptal edilirse bir kullanıcı diğerini oturumdan atabilir.
    /// </summary>
    [Fact]
    public async Task RevokeAllForUserAsync_MultipleUsers_RevokesOnlyThatUsersTokens()
    {
        // ARRANGE
        await using var context = InMemoryDbContextFactory.CreateContext();
        var repo = new RefreshTokenRepository(context);
        var kullanici1Token1 = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 1,
                TokenHash = "hash-1",
                TokenFamily = "family-a",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );
        var kullanici1Token2 = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 1,
                TokenHash = "hash-2",
                TokenFamily = "family-b",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );
        var kullanici2Token = await repo.AddAsync(
            new RefreshToken
            {
                UserId = 2,
                TokenHash = "hash-3",
                TokenFamily = "family-c",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        // ACT
        await repo.RevokeAllForUserAsync(1);

        // ASSERT
        (await repo.GetByIdAsync(kullanici1Token1.Id))!.RevokedAt.Should().NotBeNull();
        (await repo.GetByIdAsync(kullanici1Token2.Id))!.RevokedAt.Should().NotBeNull();
        (await repo.GetByIdAsync(kullanici2Token.Id))!.RevokedAt.Should().BeNull();
    }
}
