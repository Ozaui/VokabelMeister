using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class AesEncryptionService : IEncryptionService
{
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

        // Anahtar burada (ilk kullanımda değil) doğrulanır — yanlış boyutlu bir anahtarla
        // uygulama sessizce ayakta kalıp yalnızca SMTP kaydedilirken patlamak yerine, DI bu
        // servisi ilk çözdüğü anda hata görünür olsun diye.
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
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // IV + cipher tek Base64 dizide — IV gizli değildir, yalnızca çözerken aynı IV
        // gerekir; ayrı kolon yerine cipher'ın başına eklemek tek NVARCHAR(MAX) yeterli kılar.
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

        // AES blok boyutu (ve IV uzunluğu) her zaman 16 bayt — anahtar boyutundan bağımsız.
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
