// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettingsNotConfiguredException.cs
//
// AMAÇ: `POST /admin/smtp-settings/test` çağrıldığında henüz hiç SMTP ayarı
//       kaydedilmemişse (SmtpSettings tablosu boşsa) fırlatılır.
// NEDEN: Test edilecek bir ayar yoksa MailKit'e bağlanmaya ÇALIŞMAK anlamsız —
//        400 döner (varsayılan AppException statüsü), admin önce PUT ile ayarları
//        kaydetmesi gerektiğini anlar.
// BAĞIMLILIKLAR: AppException.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Exceptions;

public class SmtpSettingsNotConfiguredException : AppException
{
    public SmtpSettingsNotConfiguredException()
        : base("SMTP_SETTINGS_NOT_CONFIGURED", "SMTP test attempt: no SMTP settings have been saved yet.")
    { }
}
