using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Enums.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

public record UpdateWordCommand(
    int WordConceptId,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    List<WordTranslationRequest> Translations,
    List<int> CategoryIds,
    bool Force,
    int? UserId,
    string? ActorRole) : IRequest<WordResponse>;

public class UpdateWordCommandHandler : IRequestHandler<UpdateWordCommand, WordResponse>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateWordCommandHandler(
        IWordConceptRepository wordConceptRepository, ILanguageRepository languageRepository,
        ICategoryRepository categoryRepository, IActivityLogger activityLogger)
    {
        _wordConceptRepository = wordConceptRepository;
        _languageRepository = languageRepository;
        _categoryRepository = categoryRepository;
        _activityLogger = activityLogger;
    }

    public async Task<WordResponse> Handle(UpdateWordCommand request, CancellationToken cancellationToken)
    {
        var beforeAggregate = await _wordConceptRepository.GetAggregateAsync(request.WordConceptId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={request.WordConceptId}");
        var beforeResponse = WordMapping.ToResponse(beforeAggregate);
        var concept = beforeAggregate.Concept;

        if (request.CategoryIds.Count > 0 && !await _categoryRepository.AllExistAsync(request.CategoryIds, cancellationToken))
            throw new EntityNotFoundException($"One or more categories not found: Ids={string.Join(",", request.CategoryIds)}");

        concept.PartOfSpeech = Enum.Parse<PartOfSpeech>(request.PartOfSpeech);
        concept.DifficultyLevel = request.DifficultyLevel;
        concept.ImageUrl = request.ImageUrl;
        await _wordConceptRepository.UpdateConceptAsync(concept, request.UserId, cancellationToken);

        foreach (var translation in request.Translations)
        {
            var language = await _languageRepository.GetByCodeAsync(translation.LanguageCode, cancellationToken)
                ?? throw new EntityNotFoundException($"Language not found: Code={translation.LanguageCode}");

            var existingWord = await _wordConceptRepository.FindWordAsync(concept.Id, language.Id, cancellationToken);

            if (!request.Force)
            {
                var duplicate = await _wordConceptRepository.FindWordByLanguageAndTextAsync(
                    language.Id, translation.Text, excludeWordId: existingWord?.Id, cancellationToken);
                if (duplicate is not null)
                    throw new WordDuplicateException(translation.LanguageCode, translation.Text);
            }

            var word = existingWord ?? new Word { WordConceptId = concept.Id, LanguageId = language.Id };
            word.Text = translation.Text;
            word.Definition = translation.Definition;

            if (existingWord is null)
            {
                await _wordConceptRepository.AddWordAsync(word, request.UserId, cancellationToken);
                await _wordConceptRepository.SaveChangesAsync(cancellationToken); // word.Id gerekli (yeni dil ekleniyor)
            }
            else
            {
                await _wordConceptRepository.UpdateWordAsync(word, request.UserId, cancellationToken);
            }

            await UpsertDetailAsync(word.Id, translation.WordDetail, request.UserId, cancellationToken);

            if (translation.Examples is not null)
            {
                var newExamples = translation.Examples.Select(example => new WordExample
                {
                    WordId = word.Id,
                    SentenceText = example.SentenceText,
                    Level = example.Level,
                    ExampleType = Enum.Parse<ExampleType>(example.ExampleType ?? nameof(ExampleType.Normal))
                }).ToList();
                await _wordConceptRepository.ReplaceExamplesAsync(word.Id, newExamples, request.UserId, cancellationToken);
            }
        }

        await _wordConceptRepository.ReplaceWordCategoriesAsync(concept.Id, request.CategoryIds, cancellationToken);
        await _wordConceptRepository.SaveChangesAsync(cancellationToken);

        var aggregate = await _wordConceptRepository.GetAggregateAsync(concept.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found after update: Id={concept.Id}");
        var response = WordMapping.ToResponse(aggregate);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPDATE_WORD", entityType: "Word", entityId: concept.Id,
            oldValue: beforeResponse, newValue: response, cancellationToken: cancellationToken);

        return response;
    }

    private async Task UpsertDetailAsync(int wordId, WordDetailRequest? request, int? userId, CancellationToken cancellationToken)
    {
        if (request is null)
            return;

        var existingDetail = await _wordConceptRepository.GetDetailByWordIdAsync(wordId, cancellationToken);
        var detail = existingDetail ?? new WordDetail { WordId = wordId };
        detail.Pronunciation = request.Pronunciation;
        detail.Notes = request.Notes;
        detail.CommonMistakes = request.CommonMistakes;
        detail.GrammarData = request.GrammarData?.GetRawText();

        if (existingDetail is null)
            await _wordConceptRepository.AddDetailAsync(detail, userId, cancellationToken);
        else
            await _wordConceptRepository.UpdateDetailAsync(detail, userId, cancellationToken);
    }
}
