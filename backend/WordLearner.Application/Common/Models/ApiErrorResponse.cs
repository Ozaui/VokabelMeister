// ─────────────────────────────────────────────────────────────────────────────
// ApiErrorResponse.cs
//
// AMAÇ: Tüm hatalı endpoint yanıtlarının sarıldığı standart zarf (envelope).
// NEDEN: REFERENCE/API_ENDPOINTS.md §1'deki "Standart Yanıt" sözleşmesinin hata
//        kolunu somutlaştırır; ExceptionHandlingMiddleware her exception'ı bu
//        tipe çevirip döner — istemci hata objesinin şeklini her zaman bilir.
// BAĞIMLILIKLAR: Yok — saf C# sınıfı.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Models;

// AMAÇ: Hata kodunu (makine tarafından okunabilir) ve mesajı (kullanıcıya gösterilebilir) taşır.
// NEDEN: "code" alanı sabit bir sözlük gibi davranır (örn. INVALID_CREDENTIALS, NOT_FOUND) —
//        frontend, mesajı doğrudan göstermek yerine "code"a göre özel davranış (yönlendirme,
//        özel ikon) tetikleyebilir; "message" ise her durumda insan-okunur açıklamadır.
public record ApiErrorDetail(string Code, string Message);

// NEDEN record (2026-07-12'de class'tan çevrildi): DTOs/'daki her şey (MessageResponse,
//       AuthTokenResponse, ApiErrorDetail'in kendisi vb.) immutable record — bu tip tek
//       istisnaydı, tutarlılık için hizalandı.
public record ApiErrorResponse(ApiErrorDetail Error)
{
    // AMAÇ: İşlemin başarısız olduğunu belirtir. Bu tip yalnızca hata yolunda
    //       kullanıldığı için sabit false'dur.
    public bool Success => false;

    // AMAÇ: (code, message) çağıranlar için kısayol — ExceptionHandlingMiddleware/
    //       ValidationFilter'daki mevcut çağrı şeklini değiştirmeden Error'ı kurar.
    public ApiErrorResponse(string code, string message)
        : this(new ApiErrorDetail(code, message)) { }
}
