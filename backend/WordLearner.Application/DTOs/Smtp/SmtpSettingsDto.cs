// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettingsDto.cs
//
// AMAÇ: `GET /admin/smtp-settings` yanıtı.
// NEDEN Password STRING (ham şifre DEĞİL): hiç ayar kaydedilmemişse boş string,
//       kaydedilmişse SABİT "***" — gerçek şifre (ne düz metin ne de şifreli hâli)
//       İSTEMCİYE ASLA dönmez (REFERENCE/SECURITY.md §3.2). Admin panel formu bu
//       alanı DEĞİŞTİRİLMEDEN geri gönderirse (PUT), Handler bunu "şifre aynı kalsın"
//       sinyali olarak okur (bkz. UpdateSmtpSettingsCommand.cs "NEDEN MaskedPassword").
// BAĞIMLILIKLAR: Yok (saf DTO).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Smtp;

public record SmtpSettingsDto(
    string Host,
    int Port,
    bool EnableSsl,
    string Username,
    string Password,
    string FromEmail,
    string FromName
);
