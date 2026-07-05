// ─────────────────────────────────────────────────────────────────────────────
// RegisterDtos.cs
//
// AMAÇ: POST /auth/register endpoint'inin girdi/çıktı şekilleri.
// NEDEN: Entity (User) doğrudan dışarı verilmez — PasswordHash gibi hassas alanlar
//        gizlenir, yalnızca istemcinin ihtiyaç duyduğu alanlar sözleşmeye girer.
// BAĞIMLILIKLAR: Yok — saf DTO.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Auth;

// AMAÇ: Kayıt formunun gönderdiği ham girdi.
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

// AMAÇ: Kayıt sonrası dönen minimal kullanıcı bilgisi (REFERENCE/API_ENDPOINTS.md §3).
// NEDEN: Token dönmez — kayıt sonrası e-posta doğrulaması gerekir, hemen giriş yapılmaz.
public record RegisterResponse(int Id, string Email, string FirstName, string CurrentLevel);
