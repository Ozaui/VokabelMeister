// ─────────────────────────────────────────────────────────────────────────────
// ISmtpTestService.cs
//
// AMAÇ: `POST /admin/smtp-settings/test`in kaydedilmiş SMTP ayarlarıyla gerçekten
//       bağlanıp bir test e-postası gönderip gönderemediğini doğrulama sözleşmesi.
// NEDEN IEmailService'ten AYRI: IEmailService (A-10, henüz SmtpEmailService yazılmadı)
//       kullanıcıya giden ŞABLONLU e-postaları (OTP, hoş geldin vb.) temsil eder —
//       bu servis yalnızca admin panelin "Test Et" butonunun TEK amacı olan, ham bir
//       bağlantı+gönderim denemesi. İkisini TEK arayüzde birleştirmek IEmailService'e
//       "ayarları DB'den oku" gibi bu görevle ilgisiz bir sorumluluk yükletirdi.
// NEDEN mock'lanabilir bir arayüz (implementasyon değil): CODING_STANDARDS.md §7.4 —
//       dış servisler (burada gerçek bir SMTP sunucusuna ağ bağlantısı) HER ZAMAN
//       mock'lanır; TestSmtpSettingsCommandHandler'ın birim testi gerçek bir SMTP
//       sunucusuna bağlanmadan çalışabilmeli.
// BAĞIMLILIKLAR: WordLearner.Domain.Entities.System.SmtpSettings.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Interfaces.Services;

public interface ISmtpTestService
{
    // AMAÇ: Verilen ayarlarla (şifre ÇÖZÜLMÜŞ hâliyle) SMTP sunucusuna bağlanıp
    //       `toEmail`e kısa bir test mesajı gönderir.
    // NEDEN decryptedPassword AYRI parametre (settings.PasswordEncrypted DEĞİL):
    //       bu servis şifre çözme mantığını (IEncryptionService) BİLMEMELİ — çağıran
    //       Handler zaten SmtpSettingsRepository'den okuyup IEncryptionService.Decrypt
    //       ile çözdüğü düz metni buraya taşır (tek sorumluluk).
    // NASIL: Başarısız olursa (bağlantı/kimlik doğrulama/gönderim hatası)
    //        SmtpTestFailedException fırlatır — projenin TEK hata sözleşmesi
    //        (ApiErrorResponse) dışına çıkılmaz.
    Task SendTestEmailAsync(
        SmtpSettings settings,
        string decryptedPassword,
        string toEmail,
        CancellationToken ct = default
    );
}
