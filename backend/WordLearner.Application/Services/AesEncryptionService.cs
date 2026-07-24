// ─────────────────────────────────────────────────────────────────────────────
// AesEncryptionService.cs
//
// AMAÇ: IEncryptionService'in AES-256-CBC implementasyonu — SMTP şifresini DB'ye
//       yazmadan önce şifreler (REFERENCE/SECURITY.md §3.2, ENV.md §5).
// NEDEN IConfiguration doğrudan enjekte edilir (IOptions<T> DEĞİL): JwtTokenService/
//       LocalFileStorageService ile AYNI proje geneli desen — `AES_ENCRYPTION_KEY`
//       düz bir ortam değişkeni (Jwt:SecretKey gibi iç içe bir bölüm DEĞİL), ASP.NET
//       Core'un configuration sağlayıcıları ortam değişkenlerini otomatik okur.
// NEDEN anahtar constructor'da doğrulanır (ilk kullanımda DEĞİL): yanlış yapılandırılmış
//       bir anahtarla (32 bayt değil) uygulamanın SESSİZCE ayakta kalıp yalnızca SMTP
//       ayarları kaydedilmeye ÇALIŞILDIĞINDA patlaması yerine, DI konteyneri bu servisi
//       İLK çözdüğü anda (uygulama başlarken, AddApplicationServices Scoped kaydı ilk
//       HTTP isteğinde) hatanın hemen görünür olması tercih edilir.
// BAĞIMLILIKLAR: System.Security.Cryptography, Microsoft.Extensions.Configuration.
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class AesEncryptionService : IEncryptionService
{
    // AMAÇ: AES-256 için zorunlu anahtar uzunluğu.
    private const int KeySizeBytes = 32;

    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyBase64 =
            configuration["AES_ENCRYPTION_KEY"]
            ?? throw new InvalidOperationException(
                "AES_ENCRYPTION_KEY environment variable is not set."
            );

        _key = Convert.FromBase64String(keyBase64);

        // NEDEN: Yanlış boyutta bir anahtar (ör. rastgele bir metin, 16/24 baytlık bir
        //        AES-128/192 anahtarı) AES-256-CBC ile UYUMSUZ — sessizce yanlış bir
        //        boyutla devam etmek yerine (Aes.Key setter'ının kendi exception'ı
        //        belirsiz olabilir) burada net bir hata mesajıyla erken durdurulur.
        if (_key.Length != KeySizeBytes)
            throw new InvalidOperationException(
                $"AES_ENCRYPTION_KEY must decode to exactly {KeySizeBytes} bytes, got {_key.Length}."
            );
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        // NEDEN GenerateIV: her şifreleme rastgele bir IV üretir — aynı düz metin iki
        //       kez şifrelense bile aynı sonucu ÜRETMEZ (IV tekrarı desen sızdırır).
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // NEDEN IV + cipher TEK Base64 dizide: IV gizli değildir (şifreleme anahtarı
        //       gizlidir), yalnızca çözme sırasında AYNI IV'ye ihtiyaç duyulur — ayrı
        //       bir kolonda tutmak yerine cipher'ın BAŞINA eklemek, DB şemasında tek
        //       bir NVARCHAR(MAX) kolonu (PasswordEncrypted) yeterli kılar.
        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        var combined = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // NEDEN BlockSize/8: AES'in blok boyutu (ve dolayısıyla IV uzunluğu) her zaman
        //       16 bayttır (128 bit) — anahtar boyutundan (256 bit) BAĞIMSIZDIR.
        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        Buffer.BlockCopy(combined, 0, iv, 0, ivLength);
        aes.IV = iv;

        var cipherBytes = new byte[combined.Length - ivLength];
        Buffer.BlockCopy(combined, ivLength, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
