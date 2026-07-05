// ─────────────────────────────────────────────────────────────────────────────
// PasswordServiceTests.cs
//
// AMAÇ: PasswordService'in BCrypt (şifre) + SHA-256 (token) davranışlarını doğrulamak.
// NEDEN: Auth API'nin tüm login/register/OTP akışları bu servise dayanır; hash/verify
//        arasında bir tutarsızlık olursa hiçbir kullanıcı giriş yapamaz hâle gelir.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, WordLearner.Application.Services.PasswordService.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Application.Services;

namespace WordLearner.Tests.Services;

public class PasswordServiceTests
{
    /// <summary>
    /// Hash_ValidPassword_ProducesHashThatVerifyAccepts
    ///
    /// AMAÇ: Hash edilen bir şifrenin, aynı servisin Verify metoduyla doğru kabul edildiğini doğrulamak.
    /// NEDEN: Hash/Verify arasındaki uyumsuzluk, register'da hash'lenen şifreyle login'de
    ///        hiçbir kullanıcının giriş yapamaması anlamına gelir — en kritik mutlu yol.
    /// </summary>
    [Fact]
    public void Hash_ValidPassword_ProducesHashThatVerifyAccepts()
    {
        // ARRANGE
        var servis = new PasswordService();
        var sifre = "Deneme123!@#";

        // ACT
        var hash = servis.Hash(sifre);

        // ASSERT — hash BCrypt formatında olmalı ve orijinal şifreyle doğrulanabilmeli
        hash.Should().NotBe(sifre);
        servis.Verify(sifre, hash).Should().BeTrue();
    }

    /// <summary>
    /// Hash_SamePasswordCalledTwice_ProducesDifferentHashes
    ///
    /// AMAÇ: Aynı şifrenin iki kez hash'lenmesinin farklı sonuç ürettiğini doğrulamak.
    /// NEDEN: BCrypt her çağrıda rastgele bir salt üretir — aynı hash çıkarsa salt
    ///        üretilmiyor demektir, bu da rainbow table saldırılarına karşı korumayı yok eder.
    /// </summary>
    [Fact]
    public void Hash_SamePasswordCalledTwice_ProducesDifferentHashes()
    {
        // ARRANGE
        var servis = new PasswordService();
        var sifre = "Deneme123!@#";

        // ACT
        var hash1 = servis.Hash(sifre);
        var hash2 = servis.Hash(sifre);

        // ASSERT — farklı salt yüzünden hash'ler farklı olmalı, ikisi de aynı şifreyi doğrulamalı
        hash1.Should().NotBe(hash2);
        servis.Verify(sifre, hash1).Should().BeTrue();
        servis.Verify(sifre, hash2).Should().BeTrue();
    }

    /// <summary>
    /// Verify_WrongPassword_ReturnsFalse
    ///
    /// AMAÇ: Yanlış bir şifrenin doğru hash'e karşı false döndüğünü doğrulamak.
    /// NEDEN: AuthService.LoginAsync bu false değerine göre InvalidCredentialsException fırlatır.
    /// </summary>
    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // ARRANGE
        var servis = new PasswordService();
        var hash = servis.Hash("DogruSifre123!@#");

        // ACT
        var sonuc = servis.Verify("YanlisSifre123!@#", hash);

        // ASSERT
        sonuc.Should().BeFalse();
    }

    /// <summary>
    /// HashToken_SameInputCalledTwice_ProducesSameHash
    ///
    /// AMAÇ: Aynı token/OTP kodunun iki kez hash'lenmesinin AYNI sonucu ürettiğini doğrulamak.
    /// NEDEN: BCrypt'in aksine SHA-256 deterministik olmalı — RefreshTokens.TokenHash ve
    ///        Users.PendingOtpCodeHash bu değeri DB'de arayarak eşleştirir (GetByTokenHashAsync);
    ///        deterministik olmasaydı hiçbir refresh/OTP doğrulaması eşleşmezdi.
    /// </summary>
    [Fact]
    public void HashToken_SameInputCalledTwice_ProducesSameHash()
    {
        // ARRANGE
        var servis = new PasswordService();
        var token = "123456";

        // ACT
        var hash1 = servis.HashToken(token);
        var hash2 = servis.HashToken(token);

        // ASSERT
        hash1.Should().Be(hash2);
    }

    /// <summary>
    /// HashToken_DifferentInputs_ProducesDifferentHashes
    ///
    /// AMAÇ: Farklı iki token'ın farklı hash ürettiğini doğrulamak.
    /// NEDEN: Aksi hâlde farklı OTP kodları/refresh token'lar DB'de çakışıp yanlış kayıtla eşleşebilir.
    /// </summary>
    [Fact]
    public void HashToken_DifferentInputs_ProducesDifferentHashes()
    {
        // ARRANGE
        var servis = new PasswordService();

        // ACT
        var hash1 = servis.HashToken("111111");
        var hash2 = servis.HashToken("222222");

        // ASSERT
        hash1.Should().NotBe(hash2);
    }
}
