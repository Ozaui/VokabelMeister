// ─────────────────────────────────────────────────────────────────────────────
// IPasswordService.cs
//
// AMAÇ: Şifre hash'leme/doğrulama ve token hash'leme sözleşmesi.
// NEDEN: AuthService (A-03) somut BCrypt implementasyonuna değil bu arayüze
//        bağımlı olmalı — testlerde mock enjekte edilebilir, ileride hash
//        algoritması değişirse (ör. Argon2) çağıran kod etkilenmez.
// BAĞIMLILIKLAR: Yok — saf sözleşme.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

public interface IPasswordService
{
    // AMAÇ: Düz metin şifreyi BCrypt ile hash'ler.
    // NEDEN: PasswordHash sütununa yalnızca hash yazılır, plaintext asla saklanmaz
    //        (REFERENCE/SECURITY.md §1 — Şifre Kuralları).
    string Hash(string password);

    // AMAÇ: Düz metin şifreyi mevcut hash ile karşılaştırır.
    // NEDEN: BCrypt.Verify constant-time çalışır; timing attack riskini azaltır.
    bool Verify(string password, string hash);

    // AMAÇ: Refresh token / OTP kodu gibi rastgele üretilen token'ları SHA-256 ile hash'ler.
    // NEDEN: RefreshTokens.TokenHash ve Users.PendingOtpCodeHash DB'de yalnızca hash
    //        tutar — ham token/OTP sızıntı durumunda bile kullanılamaz.
    string HashToken(string token);
}
