// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettings.cs
//
// AMAÇ: Uygulamanın e-posta gönderiminde kullandığı SMTP bağlantı bilgileri —
//       admin panelden yönetilir, tek satırlık (singleton) bir yapılandırma kaydı
//       (bkz. DATABASE_SCHEMA/Sistem.md).
// NEDEN BaseEntity'den türer: CLAUDE.md "Veri katmanı" kuralı — log tabloları
//       hariç her tablo BaseEntity taşır. DATABASE_SCHEMA.md'nin genel notu bu
//       tabloda ad-hoc tanımlı `UpdatedByUserId` kolonunu (ilk taslak) BaseEntity
//       standardıyla birleştirilecek şekilde işaretlemişti — burada o birleştirme
//       yapıldı, ayrı bir `UpdatedBy` alanı EKLENMEDİ (BaseEntity.UpdatedByUserId
//       zaten aynı amacı karşılıyor).
// NEDEN Password DEĞİL PasswordEncrypted: ham şifre asla DB'ye yazılmaz — AES-256-CBC
//       ile şifrelenmiş Base64(IV + cipher) burada durur (REFERENCE/SECURITY.md §3.2).
// BAĞIMLILIKLAR: BaseEntity.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.System;

public class SmtpSettings : BaseEntity
{
    // AMAÇ: SMTP sunucu adresi (ör. "smtp.sendgrid.net").
    public string Host { get; set; } = string.Empty;

    // AMAÇ: SMTP portu — varsayılan 587 (STARTTLS).
    public int Port { get; set; } = 587;

    // AMAÇ: Bağlantının SSL/TLS (STARTTLS) ile kurulup kurulmayacağı.
    public bool EnableSsl { get; set; } = true;

    // AMAÇ: SMTP kimlik doğrulama kullanıcı adı.
    public string Username { get; set; } = string.Empty;

    // AMAÇ: AES-256-CBC ile şifrelenmiş SMTP şifresi — Base64(IV + cipher).
    public string PasswordEncrypted { get; set; } = string.Empty;

    // AMAÇ: Gönderilen e-postalarda "From" adresi.
    public string FromEmail { get; set; } = string.Empty;

    // AMAÇ: Gönderilen e-postalarda görünen gönderici adı.
    public string FromName { get; set; } = string.Empty;
}
