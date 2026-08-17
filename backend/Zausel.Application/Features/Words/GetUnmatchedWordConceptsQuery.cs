using MediatR;
using Zausel.Application.DTOs;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

public record GetUnmatchedWordConceptsQuery(int LanguageId, string? Search, int Page, int PageSize)
    : IRequest<PagedResult<UnmatchedWordResponse>>;

public class GetUnmatchedWordConceptsQueryHandler : IRequestHandler<GetUnmatchedWordConceptsQuery, PagedResult<UnmatchedWordResponse>>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;

    public GetUnmatchedWordConceptsQueryHandler(IWordConceptRepository wordConceptRepository, ILanguageRepository languageRepository)
    {
        _wordConceptRepository = wordConceptRepository;
        _languageRepository = languageRepository;
    }

    public async Task<PagedResult<UnmatchedWordResponse>> Handle(GetUnmatchedWordConceptsQuery request, CancellationToken cancellationToken)
    {
        var languages = await _languageRepository.GetActiveOrderedAsync(cancellationToken);
        var language = languages.FirstOrDefault(l => l.Id == request.LanguageId)
            ?? throw new EntityNotFoundException($"Language not found: Id={request.LanguageId}");

        // Yalnızca İKİ dil desteklendiği için (WordGrammarValidator/CreateWordCommandValidator) "karşı
        // dil" TEK bir aday — üç veya daha fazla dil desteklenmeye başlanırsa (şu an kapsam dışı) bu
        // satır hangi dilin "karşı" olduğunu ARTIK BELİRLEYEMEZ, o zaman ayrı bir tasarım kararı gerekir.
        var oppositeLanguage = languages.FirstOrDefault(l => l.Id != request.LanguageId);
        var oppositePool = oppositeLanguage is null
            ? []
            : await _wordConceptRepository.GetUnmatchedPoolAsync(oppositeLanguage.Id, cancellationToken);

        var page = await _wordConceptRepository.GetUnmatchedAsync(
            request.LanguageId, request.Search, request.Page, request.PageSize, cancellationToken);

        var items = page.Items
            .Select(word => new UnmatchedWordResponse(
                word.WordConceptId, language.Code, word.Text, word.PartOfSpeech.ToString(), word.DifficultyLevel,
                FindSuggestedMatch(word, oppositePool)))
            .ToList();

        return new PagedResult<UnmatchedWordResponse> { Items = items, TotalCount = page.TotalCount, Page = page.Page, PageSize = page.PageSize };
    }

    // İki yönde de dener: (a) bu kelimenin Definition'ındaki karşılıklardan biri karşı adayın
    // Text'iyle eşleşiyor mu, (b) bu kelimenin Text'i karşı adayın Definition'ındaki karşılıklardan
    // biriyle eşleşiyor mu — hangi taraf önce/hangi taraf sonra import edildiğine göre Definition'ın
    // HANGİ satırda dolu olduğu değişebilir, tek yönlü arama önerilerin YARISINI kaçırırdı.
    private static int? FindSuggestedMatch(UnmatchedWordAggregate word, List<UnmatchedWordAggregate> oppositePool)
    {
        var definitionTokens = TokenizeDefinition(word.Definition);
        var byDefinition = oppositePool.FirstOrDefault(candidate => definitionTokens.Contains(candidate.Text, StringComparer.OrdinalIgnoreCase));
        if (byDefinition is not null)
            return byDefinition.WordConceptId;

        var byReverseDefinition = oppositePool.FirstOrDefault(
            candidate => TokenizeDefinition(candidate.Definition).Contains(word.Text, StringComparer.OrdinalIgnoreCase));
        return byReverseDefinition?.WordConceptId;
    }

    private static List<string> TokenizeDefinition(string? definition) =>
        definition is null
            ? []
            : definition.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
}
