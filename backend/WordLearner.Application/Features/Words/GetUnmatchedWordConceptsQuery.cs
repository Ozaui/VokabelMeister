// ─────────────────────────────────────────────────────────────────────────────
// GetUnmatchedWordConceptsQuery.cs
//
// AMAÇ: GET /words/unmatched — `languageId` bazlı eşleşmemiş (tek dilli) kavram
//       listesi + karşı dilin havuzunda önerilen eşleşme adayı (`suggestedMatchConceptId`).
// NEDEN: Öneri, `Icerik.md` "Eşleştirme" bölümüne göre 795+ satırı admin'in elle
//        taramasını önlemek için var — WordMatchSuggestionResolver tek doğruluk kaynağı.
// NASIL: 1) `languageId`'de eşleşmemiş kavramları sayfalı çek  2) karşı dillerin
//        eşleşmemiş TÜM havuzunu çek  3) her sayfa öğesi için havuza karşı öneri ara.
// BAĞIMLILIKLAR: IWordConceptRepository, WordMatchSuggestionResolver, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Words;

public record GetUnmatchedWordConceptsQuery(int LanguageId, string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<UnmatchedWordConceptDto>>;

public class GetUnmatchedWordConceptsQueryHandler
    : IRequestHandler<GetUnmatchedWordConceptsQuery, PagedResult<UnmatchedWordConceptDto>>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetUnmatchedWordConceptsQueryHandler(IWordConceptRepository wordConceptRepository) =>
        _wordConceptRepository = wordConceptRepository;

    public async Task<PagedResult<UnmatchedWordConceptDto>> Handle(
        GetUnmatchedWordConceptsQuery request,
        CancellationToken ct
    )
    {
        var paged = await _wordConceptRepository.GetUnmatchedPagedAsync(
            request.LanguageId,
            request.Search,
            request.Page,
            request.PageSize,
            ct
        );
        var otherLanguagePool = await _wordConceptRepository.GetUnmatchedOtherLanguagePoolAsync(
            request.LanguageId,
            ct
        );

        var items = paged
            .Items.Select(concept =>
            {
                var word = concept.Words.Single();
                return new UnmatchedWordConceptDto(
                    concept.Id,
                    word.Language.Code,
                    word.Text,
                    word.Definition,
                    concept.PartOfSpeech,
                    concept.DifficultyLevel,
                    WordMatchSuggestionResolver.FindSuggestion(word, otherLanguagePool)
                );
            })
            .ToList();

        return new PagedResult<UnmatchedWordConceptDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
