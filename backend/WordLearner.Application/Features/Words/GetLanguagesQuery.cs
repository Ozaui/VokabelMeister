using MediatR;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Words;

// languageId'ye ihtiyaç duyan istemci uçları (B-03 WordFormModal/WordPairingPage — de/tr
// kod↔id eşlemesi) için — daha önce yalnızca migration seed'inden (1=de, 2=tr) biliniyordu.
public record GetLanguagesQuery : IRequest<IReadOnlyList<LanguageDto>>;

public class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, IReadOnlyList<LanguageDto>>
{
    private readonly ILanguageRepository _languageRepository;

    public GetLanguagesQueryHandler(ILanguageRepository languageRepository) =>
        _languageRepository = languageRepository;

    public async Task<IReadOnlyList<LanguageDto>> Handle(GetLanguagesQuery request, CancellationToken ct)
    {
        var languages = await _languageRepository.GetAllActiveAsync(ct);
        return languages.Select(l => new LanguageDto(l.Id, l.Code, l.Name, l.NativeName)).ToList();
    }
}
