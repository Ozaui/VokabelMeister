// ─────────────────────────────────────────────────────────────────────────────
// QrLoginStatus.cs
//
// AMAÇ: QrLoginSession kaydının hangi aşamada olduğunu gösteren durum enum'u.
// NEDEN: QR ile giriş akışı (generate→scan→confirm/deny→status) 4 farklı client'ın
//        (web, mobil, polling) aynı kaydı farklı anlarda okuyup yazması üzerine kurulu;
//        bu enum olmadan "şu an hangi adımdayız" sorusu güvenli şekilde cevaplanamaz.
// BAĞIMLILIKLAR: Yok — saf C# enum.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Enums.Auth;

public enum QrLoginStatus
{
    // AMAÇ: Oturum oluşturuldu, henüz hiçbir mobil cihaz taramadı.
    Pending,

    // AMAÇ: Mobil cihaz QR'ı taradı, kullanıcı henüz onaylamadı/reddetmedi.
    Scanned,

    // AMAÇ: Kullanıcı mobil cihazda girişi onayladı — token üretimi bekleniyor.
    Confirmed,

    // AMAÇ: Web tarafı token'ları bir kez okudu; oturum artık tekrar kullanılamaz.
    Consumed,

    // AMAÇ: Kullanıcı mobil cihazda girişi reddetti.
    Denied,

    // AMAÇ: Oturum süresi (ExpiresAt) doldu, hiçbir işlem yapılmadan geçersizleşti.
    Expired,
}
