using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

// Force — Controller'da ?force=true query string'inden okunur (API_ENDPOINTS.md §5), Request
// DTO'sunun bir alanı DEĞİL, HTTP-bağlamından gelen bir değer (A-03'teki UserId/Language deseniyle AYNI).
public record CreateWordCommand(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    List<WordTranslationRequest> Translations,
    bool Force,
    int? UserId) : IRequest<WordResponse>;

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, WordResponse>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;

    public CreateWordCommandHandler(IWordConceptRepository wordConceptRepository, ILanguageRepository languageRepository)
    {
        _wordConceptRepository = wordConceptRepository;
        _languageRepository = languageRepository;
    }

    public async Task<WordResponse> Handle(CreateWordCommand request, CancellationToken cancellationToken)
    {
        var languagesByCode = await ResolveLanguagesAsync(request.Translations, cancellationToken);

        if (!request.Force)
        {
            foreach (var translation in request.Translations)
            {
                var language = languagesByCode[translation.LanguageCode];
                var duplicate = await _wordConceptRepository.FindWordByLanguageAndTextAsync(
                    language.Id, translation.Text, excludeWordId: null, cancellationToken);
                if (duplicate is not null)
                    throw new WordDuplicateException(translation.LanguageCode, translation.Text);
            }
        }

        var concept = new WordConcept
        {
            PartOfSpeech = Enum.Parse<PartOfSpeech>(request.PartOfSpeech),
            DifficultyLevel = request.DifficultyLevel,
            ImageUrl = request.ImageUrl
        };
        await _wordConceptRepository.AddConceptAsync(concept, request.UserId, cancellationToken);
        await _wordConceptRepository.SaveChangesAsync(cancellationToken);

        foreach (var translation in request.Translations)
        {
            var language = languagesByCode[translation.LanguageCode];
            var word = new Word
            {
                WordConceptId = concept.Id,
                LanguageId = language.Id,
                Text = translation.Text,
                Definition = translation.Definition
            };
            await _wordConceptRepository.AddWordAsync(word, request.UserId, cancellationToken);
            await _wordConceptRepository.SaveChangesAsync(cancellationToken);

            await AddDetailAndExamplesAsync(word.Id, translation, request.UserId, cancellationToken);
        }

        await _wordConceptRepository.SaveChangesAsync(cancellationToken);

        var aggregate = await _wordConceptRepository.GetAggregateAsync(concept.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found after creation: Id={concept.Id}");
        return WordMapping.ToResponse(aggregate);
    }

    private async Task<Dictionary<string, Language>> ResolveLanguagesAsync(
        List<WordTranslationRequest> translations, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, Language>();
        foreach (var translation in translations)
        {
            result[translation.LanguageCode] = await _languageRepository.GetByCodeAsync(translation.LanguageCode, cancellationToken)
                ?? throw new EntityNotFoundException($"Language not found: Code={translation.LanguageCode}");
        }

        return result;
    }

    private async Task AddDetailAndExamplesAsync(int wordId, WordTranslationRequest translation, int? userId, CancellationToken cancellationToken)
    {
        if (translation.WordDetail is not null)
        {
            var detail = new WordDetail
            {
                WordId = wordId,
                Pronunciation = translation.WordDetail.Pronunciation,
                Notes = translation.WordDetail.Notes,
                CommonMistakes = translation.WordDetail.CommonMistakes,
                GrammarData = translation.WordDetail.GrammarData?.GetRawText()
            };
            await _wordConceptRepository.AddDetailAsync(detail, userId, cancellationToken);
        }

        if (translation.Examples is not null)
        {
            var newExamples = translation.Examples.Select(example => new WordExample
            {
                WordId = wordId,
                SentenceText = example.SentenceText,
                Level = example.Level,
                ExampleType = Enum.Parse<ExampleType>(example.ExampleType ?? nameof(ExampleType.Normal))
            }).ToList();

            // Yeni bir Word'de silinecek eski örnek yok — ReplaceExamplesAsync boş bir "existing"
            // bulup yalnızca ekleme yapar, Create/Update AYNI metodu paylaşır.
            await _wordConceptRepository.ReplaceExamplesAsync(wordId, newExamples, userId, cancellationToken);
        }
    }
}
