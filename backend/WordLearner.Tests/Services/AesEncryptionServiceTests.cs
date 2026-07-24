using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Services;

namespace WordLearner.Tests.Services;

public class AesEncryptionServiceTests
{
    // AMAÇ: Testlerde gerçek ENV okumadan sabit bir AES_ENCRYPTION_KEY sağlar.
    // NEDEN: JwtTokenServiceTests'teki CreateConfiguration ile AYNI desen —
    //        Convert.ToBase64String(RandomNumberGenerator...) yerine sabit bir
    //        değer, testin deterministik olmasını sağlar.
    private static IConfiguration CreateConfiguration(string? keyBase64) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AES_ENCRYPTION_KEY"] = keyBase64 })
            .Build();

    // AMAÇ: Tam 32 bayta (AES-256) çözülen geçerli bir anahtar (`openssl rand -base64 32`).
    private const string Valid32ByteKey = "Z0n/xH/HxOpxJfQO3s5qCMQBhW2yKd+Qo9jDESO5t8Q=";

    [Fact]
    public void Encrypt_ValidPlainText_ProducesCipherThatDecryptReturnsOriginal()
    {
        // ARRANGE
        var servis = new AesEncryptionService(CreateConfiguration(Valid32ByteKey));
        var duzMetin = "SmtpSifresi123!";

        // ACT
        var sifreli = servis.Encrypt(duzMetin);
        var cozulen = servis.Decrypt(sifreli);

        // ASSERT
        sifreli.Should().NotBe(duzMetin);
        cozulen.Should().Be(duzMetin);
    }

    [Fact]
    public void Encrypt_SamePlainTextCalledTwice_ProducesDifferentCipherText()
    {
        // ARRANGE
        var servis = new AesEncryptionService(CreateConfiguration(Valid32ByteKey));

        // ACT
        var birinci = servis.Encrypt("AynıŞifre");
        var ikinci = servis.Encrypt("AynıŞifre");

        // ASSERT
        birinci.Should().NotBe(ikinci);
    }

    [Fact]
    public void Constructor_KeyIsNot32Bytes_ThrowsInvalidOperationException()
    {
        // ARRANGE — 16 baytlık (AES-128) bir anahtar, Base64'e çevrilmiş
        var kisaAnahtar = Convert.ToBase64String(new byte[16]);

        // ACT
        var act = () => new AesEncryptionService(CreateConfiguration(kisaAnahtar));

        // ASSERT
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_KeyIsMissing_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var act = () => new AesEncryptionService(CreateConfiguration(null));

        // ACT & ASSERT
        act.Should().Throw<InvalidOperationException>();
    }
}
