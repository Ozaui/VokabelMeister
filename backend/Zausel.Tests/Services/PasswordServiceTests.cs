using FluentAssertions;
using Zausel.Application.Services;

namespace Zausel.Tests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void Hash_ValidPassword_ProducesHashThatVerifiesCorrectly()
    {
        // ARRANGE
        const string password = "S3curePassword!";

        // ACT
        var hash = _service.Hash(password);

        // ASSERT — bcrypt hash'i her zaman 60 karakter, ham şifreyle AYNI DEĞİL
        hash.Should().HaveLength(60);
        hash.Should().NotBe(password);
        _service.Verify(password, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // ARRANGE
        var hash = _service.Hash("DoğruŞifre!1");

        // ACT
        var result = _service.Verify("YanlışŞifre!1", hash);

        // ASSERT
        result.Should().BeFalse();
    }

    [Fact]
    public void HashToken_SameInput_ProducesDeterministicHash()
    {
        // ARRANGE + ACT — SHA-256, bcrypt'in AKSİNE rastgele salt İÇERMEZ, aynı girdi aynı çıktıyı üretmeli
        var first = _service.HashToken("refresh-token-ornegi");
        var second = _service.HashToken("refresh-token-ornegi");

        // ASSERT
        first.Should().Be(second);
        first.Should().HaveLength(44);
    }
}
