# ApiErrorResponse

**Özet:** REFERENCE/API_ENDPOINTS.md §1'deki "Standart Yanıt" sözleşmesinin hata kolunun kod karşılığı — `success:false` + `error:{code,message}` şeklinde saf bir DTO. [[Middleware|ExceptionHandlingMiddleware]] her exception'ı bu tipe çevirip JSON olarak döner. A-02'de bu tipin **yanında** `ApiResponse<T>` (başarı zarfı) ve `PagedResult<T>` (sayfalı liste) de yazılmıştı, ama hiçbir controller yokken bu ikisinin gerçek şekli spekülatifti — YAGNI kuralına göre geri alındı (bkz. aşağıdaki not). `ApiErrorResponse` kaldı çünkü gerçek bir tüketicisi (`ExceptionHandlingMiddleware`) var.
**Kütüphaneler:** Yok — saf C#.
**Bağlantılar:** [[Middleware]] · [[Program_cs]] · [[API_Sozlesmesi]] · [[WordLearner_Application]] · [[Gelistirme_Yol_Haritasi]]

## Konum
`backend/WordLearner.Application/Common/Models/ApiErrorResponse.cs`

## Kod
```csharp
public record ApiErrorDetail(string Code, string Message);

public record ApiErrorResponse(ApiErrorDetail Error)
{
    public bool Success => false;   // sabit — bu tip yalnızca hata yolunda kullanılır

    public ApiErrorResponse(string code, string message)
        : this(new ApiErrorDetail(code, message)) { }
}
```
**Düzeltme (2026-07-12, kod kalitesi denetimi):** `class`'tan `record`'a çevrildi — DTOs/'daki
her şey (dahil `ApiErrorDetail`'in kendisi) immutable record'ken bu tip tek istisnaydı. JSON çıktısı
DEĞİŞMEDİ (`{"error":{"code":...,"message":...},"success":false}`, manuel `JsonSerializer` ile
doğrulandı); `(string code, string message)` çağrı şekli de aynı kaldı, `ExceptionHandlingMiddleware`/
`ValidationFilter`'daki çağıranlar dokunulmadan çalışmaya devam ediyor.
`code` alanı sabit, dilden bağımsız bir sözlük gibi davranır (`NOT_FOUND`, `INVALID_CREDENTIALS`,
`INTERNAL_SERVER_ERROR` — bkz. [[Middleware]]); frontend, "message"i doğrudan göstermek yerine "code"a göre
özel davranış tetikleyebilir. **`message` alanı artık `Accept-Language` header'ına göre değişir**
(A-03'te eklendi) — detay → [[AppException]].

## YAGNI Düzeltmesi (2026-07-03) — Neden ApiResponse&lt;T&gt; ve PagedResult&lt;T&gt; Burada Yok
A-02'de ilk yazımda bu dosyanın yanında `ApiResponse<T>` (başarı zarfı: success/data/message/timestamp)
ve `PagedResult<T>` (sayfalı liste: items/currentPage/totalPages/totalItems/pageSize) de yazılmıştı.
Ancak o an **hiçbir controller/endpoint yoktu** — bu iki tipin gerçek şekli tahminden ibaretti;
nitekim `REFERENCE/API_ENDPOINTS.md`'deki somut örnekler (`GET /words`, `POST /auth/register` vb.)
zaten `ApiResponse<T>` zarfını **kullanmıyor**, düz DTO veya `{data, pagination}` dönüyor — yani
tahmin edilen şekil dokümanın kendi örnekleriyle bile tam örtüşmüyordu.

Bu yüzden `docs/TASK.md`'ye **"Spekülatif ortak tip yazılmaz (YAGNI)"** kuralı eklendi: bir ortak tip,
yalnızca onu **fiilen kullanan** ilk gerçek kod parçası (controller, middleware, servis) yazılırken,
o parçanın gerçek ihtiyacına göre yazılır. `ApiErrorResponse` bu kuralın **istisnası** olarak kaldı
çünkü zaten kanıtlanmış bir tüketicisi vardı (`ExceptionHandlingMiddleware`). `ApiResponse<T>` ve
`PagedResult<T>` silindi; ilk gerçek controller (muhtemelen A-03 Auth ya da A-05 Words) yazılırken,
o an neye ihtiyaç duyulduğu görülüp o task içinde yeniden yazılacak.

**Ders:** Bu düğüm hâlâ burada duruyor ki gelecekte biri "ApiResponse<T>/PagedResult<T> A-02'de
zaten yazılmıştı" diye yanlış hatırlamasın — bilinçli olarak geri alındı, unutulmadı.
