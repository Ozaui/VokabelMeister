using System.Security.Cryptography;
using System.Text;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class PasswordService : IPasswordService
{
    // 10'dan güvenli, 14+'ten hızlı — her login isteğinde kabul edilebilir bir gecikme (~200-300ms) verir.
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    // Şifre değildir (zaten yüksek entropili rastgele veri) — BCrypt'in yavaş/salt'lı
    // tasarımına ihtiyaç yok, SHA-256 hızlı ve yeterli.
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
