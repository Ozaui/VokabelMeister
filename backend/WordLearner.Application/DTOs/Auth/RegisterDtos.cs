// ─────────────────────────────────────────────────────────────────────────────
// RegisterDtos.cs
//
// AMAÇ: POST /auth/register endpoint'inin çıktı şekli.
// NEDEN: Entity (User) doğrudan dışarı verilmez — PasswordHash gibi hassas alanlar
//        gizlenir, yalnızca istemcinin ihtiyaç duyduğu alanlar sözleşmeye girer.
//        Girdi şekli artık RegisterCommand (Features/Auth/RegisterCommand.cs) —
//        MediatR CQRS'te Command'lar doğrudan Request DTO'sunun yerini alır.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Kayıt sonrası dönen minimal kullanıcı bilgisi (REFERENCE/API_ENDPOINTS.md §3).
// NEDEN: Token dönmez — kayıt sonrası e-posta doğrulaması gerekir, hemen giriş yapılmaz.
// NEDEN (CurrentLevel/ThemePreference): İkisi de RegisterCommand'da GİRDİ olarak alınmaz (kayıt
//        anonim, henüz kullanıcı tercihini soracak bir onboarding akışı çalışmadı) — yalnızca
//        DB varsayılanı (A1/System) burada DÖNER, gerçek seçim ilk-login-sonrası onboarding'de
//        (LevelSelectPage, PUT /users/me — C-01) yapılır.
public record RegisterResponse(
    int Id,
    string Email,
    string FirstName,
    string CurrentLevel,
    string ThemePreference
);
