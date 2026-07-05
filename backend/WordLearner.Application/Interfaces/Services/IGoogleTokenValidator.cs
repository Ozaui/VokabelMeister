// ─────────────────────────────────────────────────────────────────────────────
// IGoogleTokenValidator.cs
//
// AMAÇ: Google Sign-In'in istemcide ürettiği ID token'ını doğrulama sözleşmesi.
// NEDEN: AuthService, Google.Apis.Auth kütüphanesine doğrudan bağımlı olmak yerine
//        bu arayüze bağımlı olmalı — testlerde gerçek bir Google sunucu çağrısı
//        yapılmadan mock enjekte edilebilir (CODING_STANDARDS.md §7.4).
// BAĞIMLILIKLAR: Yok — saf sözleşme.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

// AMAÇ: Doğrulanmış Google token'ından çıkarılan, AuthService'in ihtiyaç duyduğu alanlar.
public record GoogleTokenPayload(
    string GoogleId,
    string Email,
    string? FirstName,
    string? LastName
);

public interface IGoogleTokenValidator
{
    // AMAÇ: Ham ID token'ı doğrular; geçersizse (imza/audience/süre) null döner.
    // NEDEN: AuthService, doğrulama başarısız olursa InvalidSocialTokenException fırlatır —
    //        bu metot exception fırlatmaz, yalnızca null/dolu döner (akış kontrolü servise ait).
    Task<GoogleTokenPayload?> ValidateAsync(string idToken, CancellationToken ct = default);
}
