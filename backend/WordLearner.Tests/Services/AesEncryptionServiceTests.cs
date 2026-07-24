// ─────────────────────────────────────────────────────────────────────────────
// AesEncryptionServiceTests.cs
//
// AMAÇ: AesEncryptionService'in Encrypt/Decrypt round-trip'ini ve yanlış boyutlu
//       bir AES_ENCRYPTION_KEY ile başlatıldığında erken hata verdiğini doğrulamak.
// NEDEN: Bu servis SMTP şifresinin DB'de düz metin olarak SIZMASINI önleyen tek
//        katman — round-trip bozulursa admin panelin kaydettiği şifre asla doğru
//        çözülemez, e-posta gönderimi (A-10) sessizce başarısız olur.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.Extensions.Configuration,
//                WordLearner.Application.Services.AesEncryptionService.
// ─────────────────────────────────────────────────────────────────────────────

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

    /// <summary>
    /// Encrypt_ValidPlainText_ProducesCipherThatDecryptReturnsOriginal
    ///
    /// AMAÇ: Encrypt edilen bir metnin, aynı servisin Decrypt metoduyla orijinaline
    ///       döndüğünü doğrulamak — en kritik mutlu yol (round-trip).
    /// </summary>
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

    /// <summary>
    /// Encrypt_SamePlainTextCalledTwice_ProducesDifferentCipherText
    ///
    /// AMAÇ: Aynı düz metnin iki kez şifrelenmesinin farklı sonuç ürettiğini
    ///       doğrulamak — her çağrının rastgele bir IV ürettiğinin kanıtı.
    /// </summary>
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

    /// <summary>
    /// Constructor_KeyIsNot32Bytes_ThrowsInvalidOperationException
    ///
    /// AMAÇ: 32 bayttan FARKLI (ör. 16 baytlık bir AES-128 anahtarı) bir
    ///       AES_ENCRYPTION_KEY ile servis oluşturulmaya çalışıldığında,
    ///       ilk şifreleme çağrısını BEKLEMEDEN, constructor'da hata verdiğini doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Constructor_KeyIsMissing_ThrowsInvalidOperationException
    ///
    /// AMAÇ: AES_ENCRYPTION_KEY ortam değişkeni hiç ayarlanmamışsa (ENV.md §5'in
    ///       "kaybolursa DB'deki SMTP şifreleri çözülemez" uyarısının başka bir yüzü)
    ///       servisin net bir hata ile başlatılamadığını doğrulamak.
    /// </summary>
    [Fact]
    public void Constructor_KeyIsMissing_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var act = () => new AesEncryptionService(CreateConfiguration(null));

        // ACT & ASSERT
        act.Should().Throw<InvalidOperationException>();
    }
}
