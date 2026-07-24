// ─────────────────────────────────────────────────────────────────────────────
// UpdateSmtpSettingsCommand.cs
//
// AMAÇ: PUT /admin/smtp-settings — SMTP ayarlarını kaydeder (ilk kayıtsa oluşturur,
//       varsa günceller — upsert, çünkü tabloda her zaman TEK satır olur).
// NEDEN: SMTP ayarları CLAUDE.md "Kimlik & güvenlik"nin kapsadığı hassas bir admin
//        işlemi (SMTP kimlik bilgisi değişimi) — CLAUDE.md "İçerik değiştiren her
//        CRUD..." kuralına göre HEM IActivityLogger (UPDATE_SMTP_SETTINGS) HEM
//        ISecurityLogger (LogEventType.AdminAction) çağrılır. PasswordEncrypted
//        diff'ten (OldValue/NewValue) HARİÇ tutulur — CLAUDE.md "şifre/hash gibi
//        hassas alanlar diff'ten hariç tutulur" kuralı.
// NEDEN MaskedPassword ("***"): GET /admin/smtp-settings gerçek şifreyi ASLA
//        döndürmez (GetSmtpSettingsQuery.cs), admin panel formu şifre alanını hiç
//        değiştirmeden PUT'a geri gönderirse bu SABİT değer gelir — Handler bunu
//        "şifreyi DEĞİŞTİRME, eskisini KORU" sinyali olarak okur. Bu sabit,
//        GetSmtpSettingsQueryHandler'daki AYNI isimli sabitle DEĞER olarak
//        BİREBİR eşleşmelidir (ikisi ayrı dosyada çünkü Query/Command ayrı
//        dikey dilimler — CLAUDE.md §3, ama SÖZLEŞME olarak tek bir string).
// BAĞIMLILIKLAR: ISmtpSettingsRepository, IEncryptionService, IActivityLogger, ISecurityLogger.
// ─────────────────────────────────────────────────────────────────────────────

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
    // NEDEN: bkz. dosya başı "NEDEN MaskedPassword" — GetSmtpSettingsQueryHandler'daki
    //        AYNI isimli sabitle değer olarak eşleşmek ZORUNDA.
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

        // NEDEN bu kontrol (kod denetiminde bulundu): ilk kayıtta (existing null)
        //       "koru"nacak eski bir şifre YOK — maske literal'i ("***") bu durumda
        //       BİR ANLAM TAŞIMAZ. Bu kontrol OLMASAYDI, aşağıdaki satır "***" stringinin
        //       KENDİSİNİ şifreleyip DB'ye gerçek SMTP şifresi yerine yazardı (sessiz
        //       bir yanlış-yapılandırma) — admin panel normalde bu durumu GET'in boş
        //       Password dönmesi sayesinde önler, ama bu ikinci, sunucu-taraflı bir
        //       savunma katmanıdır (istemciye güvenmemek).
        if (request.Password == MaskedPassword && existing is null)
            throw new SmtpPasswordRequiredException();

        var passwordEncrypted =
            request.Password == MaskedPassword && existing is not null
                ? existing.PasswordEncrypted
                : _encryptionService.Encrypt(request.Password);

        // NEDEN oldValue'da PasswordEncrypted/Password YOK: CLAUDE.md "şifre/hash gibi
        //       hassas alanlar diff'ten hariç tutulur" kuralı.
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
