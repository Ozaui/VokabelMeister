namespace WordLearner.Application.Interfaces.Services;

public interface IPasswordService
{
    string Hash(string password);

    // BCrypt.Verify constant-time çalışır; timing attack riskini azaltır.
    bool Verify(string password, string hash);

    // Refresh token / OTP kodu gibi rastgele üretilen değerleri SHA-256 ile hash'ler.
    string HashToken(string token);
}
