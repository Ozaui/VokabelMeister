// ─────────────────────────────────────────────────────────────────────────────
// IAppleTokenValidator.cs
//
// AMAÇ: Apple Sign-In'in istemcide ürettiği identity token'ını doğrulama sözleşmesi.
// NEDEN: AuthService, Apple'ın JWKS/HTTP çağrısına doğrudan bağımlı olmak yerine bu
//        arayüze bağımlı olmalı — testlerde gerçek bir ağ çağrısı yapılmadan mock
//        enjekte edilebilir (CODING_STANDARDS.md §7.4).
// BAĞIMLILIKLAR: Yok — saf sözleşme.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Interfaces.Services;

// AMAÇ: Doğrulanmış Apple token'ından çıkarılan, AuthService'in ihtiyaç duyduğu alanlar.
// NEDEN: Email yalnızca İLK yetkilendirmede gelir (Apple'ın bilinen kısıtı) — sonraki
//        girişlerde null olabilir, AuthService bu durumda DB'deki mevcut email'i korur.
public record AppleTokenPayload(string AppleId, string? Email);

public interface IAppleTokenValidator
{
    // AMAÇ: Ham identity token'ı doğrular; geçersizse (imza/issuer/audience/süre) null döner.
    Task<AppleTokenPayload?> ValidateAsync(string identityToken, CancellationToken ct = default);
}
