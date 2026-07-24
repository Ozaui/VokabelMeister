using MediatR;
using WordLearner.Application.DTOs.Smtp;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Smtp;

public record GetSmtpSettingsQuery : IRequest<SmtpSettingsDto>;

public class GetSmtpSettingsQueryHandler : IRequestHandler<GetSmtpSettingsQuery, SmtpSettingsDto>
{
    private const int DefaultPort = 587;

    // UpdateSmtpSettingsCommandHandler'daki aynı isimli sabitle değer olarak eşleşmeli.
    private const string MaskedPassword = "***";

    private readonly ISmtpSettingsRepository _smtpSettingsRepository;

    public GetSmtpSettingsQueryHandler(ISmtpSettingsRepository smtpSettingsRepository) =>
        _smtpSettingsRepository = smtpSettingsRepository;

    public async Task<SmtpSettingsDto> Handle(GetSmtpSettingsQuery request, CancellationToken ct)
    {
        var settings = await _smtpSettingsRepository.GetCurrentAsync(ct);

        // Hiç kayıt yoksa (ilk kurulum) boş bir DTO döner, 404 fırlatılmaz — admin panel her zaman bir form gösterir.
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
