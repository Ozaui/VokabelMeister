namespace WordLearner.Application.Interfaces.Services;

public interface IEncryptionService
{
    // Base64(rastgele IV + AES-256-CBC cipher) — aynı düz metin iki kez şifrelense bile farklı sonuç üretir.
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
