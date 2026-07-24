// ─────────────────────────────────────────────────────────────────────────────
// GetSmtpSettingsQuery.cs
//
// AMAÇ: GET /admin/smtp-settings — kayıtlı SMTP ayarlarını, şifre alanı maskelenmiş
//       (`***`) hâlde döner.
// NEDEN hiç kayıt yoksa (ilk kurulum) boş alanlarla bir DTO dönülür, 404 FIRLATILMAZ:
//        admin panelin SMTP ayarları ekranı her zaman bir form gösterir — "ayarlar
//        henüz yok" durumu boş bir form olarak temsil edilir, bir hata durumu değildir.
// BAĞIMLILIKLAR: ISmtpSettingsRepository.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.DTOs.Smtp;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Smtp;

public record GetSmtpSettingsQuery : IRequest<SmtpSettingsDto>;

public class GetSmtpSettingsQueryHandler : IRequestHandler<GetSmtpSettingsQuery, SmtpSettingsDto>
{
    // AMAÇ: DB'de hiç ayar yokken PUT formunun varsayılan olarak göstereceği port.
    private const int DefaultPort = 587;

    // AMAÇ: Şifre kayıtlıysa istemciye giden sabit maske — gerçek değer asla dönmez.
    private const string MaskedPassword = "***";

    private readonly ISmtpSettingsRepository _smtpSettingsRepository;

    public GetSmtpSettingsQueryHandler(ISmtpSettingsRepository smtpSettingsRepository) =>
        _smtpSettingsRepository = smtpSettingsRepository;

    public async Task<SmtpSettingsDto> Handle(GetSmtpSettingsQuery request, CancellationToken ct)
    {
        var settings = await _smtpSettingsRepository.GetCurrentAsync(ct);

        if (settings is null)
            return new SmtpSettingsDto(string.Empty, DefaultPort, true, string.Empty, string.Empty, string.Empty, string.Empty);

        return new SmtpSettingsDto(
            settings.Host,
            settings.Port,
            settings.EnableSsl,
            settings.Username,
            MaskedPassword,
            settings.FromEmail,
            settings.FromName
        );
    }
}
