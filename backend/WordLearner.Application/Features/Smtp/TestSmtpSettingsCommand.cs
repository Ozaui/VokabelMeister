using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Features.Smtp;

// Body'de bir SmtpSettings şekli yok (yalnızca ToEmail) — test, admin'in o an DB'de kayıtlı
// ayarlarını doğrular, henüz PUT edilmemiş form değerlerini değil.
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
