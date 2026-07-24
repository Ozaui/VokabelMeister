using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.System;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Smtp;

public record UpdateSmtpSettingsCommand(
    string Host,
    int Port,
    bool EnableSsl,
    string Username,
    string Password,
    string FromEmail,
    string FromName
) : IRequest<Unit>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
    public string? IpAddress { get; init; }
}

public class UpdateSmtpSettingsCommandHandler : IRequestHandler<UpdateSmtpSettingsCommand, Unit>
{
    // GetSmtpSettingsQueryHandler'daki aynı isimli sabitle değer olarak eşleşmek zorunda.
    private const string MaskedPassword = "***";

    private readonly ISmtpSettingsRepository _smtpSettingsRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IActivityLogger _activityLogger;
    private readonly ISecurityLogger _securityLogger;

    public UpdateSmtpSettingsCommandHandler(
        ISmtpSettingsRepository smtpSettingsRepository,
        IEncryptionService encryptionService,
        IActivityLogger activityLogger,
        ISecurityLogger securityLogger
    )
    {
        _smtpSettingsRepository = smtpSettingsRepository;
        _encryptionService = encryptionService;
        _activityLogger = activityLogger;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(UpdateSmtpSettingsCommand request, CancellationToken ct)
    {
        var existing = await _smtpSettingsRepository.GetCurrentAsync(ct);

        // İlk kayıtta (existing null) korunacak eski şifre yok — maske literali burada anlamsız,
        // bu kontrol olmasaydı "***" stringinin kendisi şifrelenip DB'ye yazılırdı.
        if (request.Password == MaskedPassword && existing is null)
            throw new SmtpPasswordRequiredException();

        var passwordEncrypted =
            request.Password == MaskedPassword && existing is not null
                ? existing.PasswordEncrypted
                : _encryptionService.Encrypt(request.Password);

        // Şifre/hash gibi hassas alanlar diff'ten hariç tutulur.
        var oldValue =
            existing is null
                ? null
                : new
                {
                    existing.Host,
                    existing.Port,
                    existing.EnableSsl,
                    existing.Username,
                    existing.FromEmail,
                    existing.FromName,
                };

        int entityId;
        if (existing is null)
        {
            var created = new SmtpSettings
            {
                Host = request.Host,
                Port = request.Port,
                EnableSsl = request.EnableSsl,
                Username = request.Username,
                PasswordEncrypted = passwordEncrypted,
                FromEmail = request.FromEmail,
                FromName = request.FromName,
            };
            created = await _smtpSettingsRepository.AddAsync(created, request.UserId, ct);
            entityId = created.Id;
        }
        else
        {
            existing.Host = request.Host;
            existing.Port = request.Port;
            existing.EnableSsl = request.EnableSsl;
            existing.Username = request.Username;
            existing.PasswordEncrypted = passwordEncrypted;
            existing.FromEmail = request.FromEmail;
            existing.FromName = request.FromName;
            await _smtpSettingsRepository.UpdateAsync(existing, request.UserId, ct);
            entityId = existing.Id;
        }

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "UPDATE_SMTP_SETTINGS",
            entityType: "SmtpSettings",
            entityId: entityId,
            oldValue: oldValue,
            newValue: new
            {
                request.Host,
                request.Port,
                request.EnableSsl,
                request.Username,
                request.FromEmail,
                request.FromName,
            },
            ipAddress: request.IpAddress,
            ct: ct
        );

        await _securityLogger.LogAsync(
            LogEventType.AdminAction,
            userId: request.UserId,
            ipAddress: request.IpAddress,
            detail: "SMTP_SETTINGS_CHANGED",
            ct: ct
        );

        return Unit.Value;
    }
}
