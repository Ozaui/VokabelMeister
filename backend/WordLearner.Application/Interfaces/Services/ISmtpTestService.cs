using WordLearner.Domain.Entities.System;

namespace WordLearner.Application.Interfaces.Services;

// IEmailService'ten AYRI — o kullanıcıya giden şablonlu e-postaları temsil eder, bu servis
// yalnızca admin panelin "Test Et" butonunun ham bağlantı+gönderim denemesidir.
public interface ISmtpTestService
{
    // decryptedPassword ayrı parametre — bu servis şifre çözme mantığını (IEncryptionService)
    // bilmemeli, çağıran Handler zaten çözülmüş düz metni taşır.
    Task SendTestEmailAsync(
        SmtpSettings settings,
        string decryptedPassword,
        string toEmail,
        CancellationToken ct = default
    );
}
