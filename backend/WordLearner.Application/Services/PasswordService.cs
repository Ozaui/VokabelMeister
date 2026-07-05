// ─────────────────────────────────────────────────────────────────────────────
// PasswordService.cs
//
// AMAÇ: IPasswordService'in BCrypt (şifre) + SHA-256 (token) tabanlı implementasyonu.
// NEDEN: ASP.NET Identity kullanılmadığı için (REFERENCE/SECURITY.md §1) şifre
//        hash'leme manuel yazılır; work factor 12, saldırganın brute-force
//        maliyetini kabul edilebilir ölçüde artırırken login gecikmesini makul tutar.
// BAĞIMLILIKLAR: BCrypt.Net-Next, System.Security.Cryptography (SHA256).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Cryptography;
using System.Text;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class PasswordService : IPasswordService
{
    // NEDEN 12: REFERENCE/TECHNICAL_SPECIFICATIONS.md §6'da pinlenen değer — 10'dan güvenli,
    //        14+'ten hızlı; her login isteğinde kabul edilebilir bir gecikme (~200-300ms) verir.
    private const int WorkFactor = 12;

    // AMAÇ: Düz metin şifreyi BCrypt ile hash'ler (60 karakter, otomatik rastgele salt).
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

    // AMAÇ: Düz metin şifreyi mevcut hash ile constant-time karşılaştırır.
    // NEDEN: BCrypt.Verify salt'ı hash içinden kendi çıkarır, work factor'e bakılmaksızın çalışır.
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    // AMAÇ: Refresh token / OTP kodu gibi rastgele üretilen değerleri SHA-256 ile hash'ler.
    // NEDEN: Bu değerler şifre değildir (zaten yüksek entropili rastgele veridir), bu yüzden
    //        BCrypt'in yavaş/salt'lı tasarımına ihtiyaç yoktur — SHA-256 hızlı ve yeterlidir;
    //        RefreshTokens.TokenHash ve Users.PendingOtpCodeHash bu metotla üretilir.
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
