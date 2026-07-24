// ─────────────────────────────────────────────────────────────────────────────
// TestSmtpSettingsCommand.cs
//
// AMAÇ: POST /admin/smtp-settings/test — kayıtlı (PUT ile ÖNCEDEN kaydedilmiş)
//       SMTP ayarlarıyla gerçekten bağlanıp isteği yapan admin'in kendi e-posta
//       adresine bir test e-postası gönderir.
// NEDEN request body'de bir SmtpSettings şekli YOK (yalnızca ToEmail):
//       API_ENDPOINTS.md §11 bu endpoint'i parametresiz listeler — test, ADMIN'İN
//       O AN DB'DE KAYITLI ayarlarını doğrular (henüz kaydedilmemiş, formda yazılı
//       ama PUT edilmemiş değerleri DEĞİL) — "önce kaydet, sonra test et" akışı.
// NEDEN ActivityLog/SecurityLog YOK (UpdateSmtpSettingsCommand'ın AKSİNE): bu
//        komut hiçbir veriyi DEĞİŞTİRMEZ, yalnızca OKUR ve dışarıya bir e-posta
//        gönderir — CLAUDE.md'nin "İçerik değiştiren her CRUD" kuralı yalnızca
//        create/update/delete için geçerli, salt-okunur bir doğrulama eylemi değil.
// BAĞIMLILIKLAR: ISmtpSettingsRepository, IEncryptionService, ISmtpTestService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Features.Smtp;

public record TestSmtpSettingsCommand : IRequest<MessageResponse>
{
    public string ToEmail { get; init; } = string.Empty;
    public string? Language { get; init; }
}

public class TestSmtpSettingsCommandHandler : IRequestHandler<TestSmtpSettingsCommand, MessageResponse>
{
    private readonly ISmtpSettingsRepository _smtpSettingsRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ISmtpTestService _smtpTestService;

    public TestSmtpSettingsCommandHandler(
        ISmtpSettingsRepository smtpSettingsRepository,
        IEncryptionService encryptionService,
        ISmtpTestService smtpTestService
    )
    {
        _smtpSettingsRepository = smtpSettingsRepository;
        _encryptionService = encryptionService;
        _smtpTestService = smtpTestService;
    }

    public async Task<MessageResponse> Handle(TestSmtpSettingsCommand request, CancellationToken ct)
    {
        var settings =
            await _smtpSettingsRepository.GetCurrentAsync(ct)
            ?? throw new SmtpSettingsNotConfiguredException();

        var decryptedPassword = _encryptionService.Decrypt(settings.PasswordEncrypted);

        await _smtpTestService.SendTestEmailAsync(settings, decryptedPassword, request.ToEmail, ct);

        return new MessageResponse("SMTP_TEST_EMAIL_SENT", SuccessMessages.Resolve("SMTP_TEST_EMAIL_SENT", request.Language));
    }
}
