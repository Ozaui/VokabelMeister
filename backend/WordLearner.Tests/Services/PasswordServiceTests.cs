using FluentAssertions;
using WordLearner.Application.Services;

namespace WordLearner.Tests.Services;

public class PasswordServiceTests
{
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
