// ─────────────────────────────────────────────────────────────────────────────
// QrLoginSession.cs
//
// AMAÇ: "QR kod ile giriş" akışının tek bir denemesini tutan kayıt.
// NEDEN: QR girişi ayrı bir kimlik doğrulama sistemi değildir — web/desktop tarafın
//        ürettiği bir oturumu, mobil cihazın taraması ve onaylaması sonucunda
//        A-03'te yazılan ITokenService'e bağlar (bkz. REFERENCE/SECURITY.md §1.3).
//        Tek kullanımlıktır: Confirmed durumu bir kez okunduktan sonra Consumed'a
//        geçer, token'lar tekrar döndürülmez.
// BAĞIMLILIKLAR: BaseEntity, QrLoginStatus enum, User (N:1, opsiyonel ilişki).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

public class QrLoginSession : BaseEntity
{
    // AMAÇ: Ham QR token'ının SHA-256 hash'i. Plaintext asla saklanmaz.
    public string QrTokenHash { get; set; } = string.Empty;

    // AMAÇ: Web ve mobil ekranda yan yana gösterilen 4 haneli görsel karşılaştırma kodu.
    // NEDEN: TokenHash yalnızca DB sızıntısına karşı korur; PairingCode ise bir saldırganın
    //        kurbanı kendi QR'ını taratmaya kandırdığı relay/phishing saldırısına karşı
    //        bağımsız bir savunma katmanıdır (kodlar görsel olarak eşleşmez).
    public string PairingCode { get; set; } = string.Empty;

    // AMAÇ: Oturumun şu anki aşaması (Pending→Scanned→Confirmed→Consumed, veya Denied/Expired).
    public QrLoginStatus Status { get; set; } = QrLoginStatus.Pending;

    // AMAÇ: Oturumu tarayıp onaylayan/reddeden kullanıcı. Yalnızca Scanned'e geçince set edilir.
    public int? UserId { get; set; }

    // AMAÇ: Navigation property — EF Core Include() ile kullanıcı bilgisine erişim sağlar.
    // NEDEN: Tek yönlü ilişki — User tarafında karşılık gelen bir koleksiyon eklenmedi çünkü
    //        şu an hiçbir akış "kullanıcının QR oturumları" listesini istemiyor (YAGNI).
    public User? User { get; set; }

    // AMAÇ: Taramayı yapan cihazın IP adresi — audit ve onay ekranında karşılaştırma amaçlı.
    public string? RequesterIp { get; set; }

    // AMAÇ: Taramayı yapan cihazın bilgisi (user-agent vb.) — mobil onay ekranında gösterilir.
    public string? RequesterDeviceInfo { get; set; }

    // AMAÇ: Mobil cihazın QR'ı taradığı an (UTC). Taranmadıysa null.
    public DateTime? ScannedAt { get; set; }

    // AMAÇ: Kullanıcının girişi onayladığı an (UTC). Onaylanmadıysa null.
    public DateTime? ConfirmedAt { get; set; }

    // AMAÇ: Oturumun geçerlilik süresinin dolacağı an (UTC).
    public DateTime ExpiresAt { get; set; }
}
