// ─────────────────────────────────────────────────────────────────────────────
// LogEventType.cs
//
// AMAÇ: SecurityLog.EventType kolonunun alabileceği sabit değer kümesi.
// NEDEN: Serbest metin yerine enum kullanmak yazım hatalarını derleme zamanında
//        yakalar; ISecurityLogger çağıran her yer bu kümeden seçim yapar.
// BAĞIMLILIKLAR: Yok — saf enum.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Enums.Logging;

public enum LogEventType
{
    // AMAÇ: Login ADIM 1'de BCrypt doğrulaması/hesap durumu kontrolü başarısız oldu.
    LoginFailed,

    // AMAÇ: OTP doğrulaması (login/kayıt/şifre sıfırlama/hesap silme) başarısız oldu.
    OtpFailed,

    // AMAÇ: Rate limiting eşiği aşıldı (login 5/15dk, OTP 3 yanlış vb.).
    RateLimitHit,

    // AMAÇ: Yetkisiz bir kaynağa erişim denemesi (ör. başkasının kaydına 403).
    UnauthorizedAccess,

    // AMAÇ: Refresh Token Family Pattern — zaten kullanılmış bir refresh token tekrar kullanıldı.
    TokenReplay,

    // AMAÇ: Şifre sıfırlama akışı tamamlandı (forgot/reset-password).
    PasswordReset,

    // AMAÇ: Hesap silme talebi onaylandı (ConfirmAccountDeletion).
    AccountDeletion,

    // AMAÇ: Bir admin'in yetki gerektiren işlemi (rol değiştirme, hesap dondurma vb.).
    AdminAction,

    // AMAÇ: QR ile giriş onaylandı.
    QrLoginConfirmed,

    // AMAÇ: QR ile giriş reddedildi.
    QrLoginDenied,
}
