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
// NEDEN: "code" alanı sabit bir sözlük gibi davranır (örn. GECERSIZ_KIMLIK, BULUNAMADI) —
//        frontend, mesajı doğrudan göstermek yerine "code"a göre özel davranış (yönlendirme,
//        özel ikon) tetikleyebilir; "message" ise her durumda insan-okunur açıklamadır.
public record ApiErrorDetail(string Code, string Message);

public class ApiErrorResponse
{
    // AMAÇ: İşlemin başarısız olduğunu belirtir. Bu tip yalnızca hata yolunda
    //       kullanıldığı için sabit false'dur.
    public bool Success { get; } = false;

    // AMAÇ: Hatanın kodu ve mesajını taşıyan detay objesi.
    public ApiErrorDetail Error { get; set; }

    public ApiErrorResponse(string code, string message)
    {
        Error = new ApiErrorDetail(code, message);
    }
}
