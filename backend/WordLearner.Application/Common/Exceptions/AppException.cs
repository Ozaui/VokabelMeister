// ─────────────────────────────────────────────────────────────────────────────
// AppException.cs
//
// AMAÇ: İstemciye dönecek mesajı kendi içinde SABİTLEMEYEN, yalnızca dil bağımsız
//       bir hata kodu (Code) taşıyan taban exception.
// NEDEN: Bir hata kodunun (ör. GECERSIZ_KIMLIK) istemciye gösterilecek metni,
//        isteğin diline göre değişebilmeli (Accept-Language). Bu yüzden mesaj
//        exception'ın kendisinde değil, WordLearner.Application.Common.Localization.
//        ErrorMessages sözlüğünde durur; ExceptionHandlingMiddleware Code'u bu
//        sözlükten çözer. .Message yalnızca log/geliştirici için sabit Türkçe açıklamadır
//        (CODING_STANDARDS.md §1 — tüm log/exception mesajları Türkçe kuralı budur;
//        istemciye giden metin ayrı bir kanaldan, ErrorMessages'ten gelir).
//        EntityNotFoundException bilinçli olarak bundan TÜRETİLMEZ — mesajı entity
//        adı gibi dinamik veri içerir, sabit kod sözlüğüne uymaz.
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    // AMAÇ: İstemciye dönecek, dilden bağımsız hata kodu (ör. "GECERSIZ_KIMLIK").
    // NEDEN: ErrorMessages sözlüğünün anahtarıdır; ExceptionHandlingMiddleware bu kodu
    //        isteğin diline göre insan-okunur bir mesaja çevirir.
    public string Code { get; }

    // AMAÇ: Alt sınıfların kod + sabit Türkçe log mesajını taban sınıfa iletmesini sağlar.
    protected AppException(string code, string developerMessage)
        : base(developerMessage)
    {
        Code = code;
    }
}
