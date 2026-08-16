using MediatR;
using WordLearner.Application.DTOs.Languages;
using WordLearner.Application.Interfaces.Repositories.Content;

namespace WordLearner.Application.Features.Languages;

public record GetLanguagesQuery : IRequest<List<LanguageResponse>>;

public class GetLanguagesQueryHandler : IRequestHandler<GetLanguagesQuery, List<LanguageResponse>>
{
    private readonly ILanguageRepository _languageRepository;

    public GetLanguagesQueryHandler(ILanguageRepository languageRepository) => _languageRepository = languageRepository;

    public async Task<List<LanguageResponse>> Handle(GetLanguagesQuery request, CancellationToken cancellationToken)
    {
        var languages = await _languageRepository.GetActiveOrderedAsync(cancellationToken);
        return languages.Select(l => new LanguageResponse(l.Id, l.Code, l.Name, l.NativeName)).ToList();
    }
}
