using System.Text.Json;
using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public record WordExampleInput(string SentenceText, string Level, string ExampleType = "Normal");

// GrammarData ham JSON olarak taşınır — WordGrammarValidator dile/türe göre doğrular.
public record WordDetailInput(
    string? Pronunciation,
    string? AudioUrl,
    string? Notes,
    string? CommonMistakes,
    JsonElement? GrammarData
);

public record WordTranslationInput(
    string LanguageCode,
    string Text,
    string? Definition,
    WordDetailInput? WordDetail,
    IReadOnlyList<WordExampleInput>? Examples
);

public record CreateWordCommand(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationInput> Translations,
    // Null = kavram kategorisiz oluşturulur (B-04'ün "önce kelime, sonra kategorile" akışı için).
    IReadOnlyList<int>? CategoryIds = null
) : IRequest<WordConceptDetailDto>
{
    // Query string'ten gelir (?force=true) — controller `with { Force = force }` ile ekler.
    public bool Force { get; init; }

    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public CreateWordCommandHandler(
        IWordConceptRepository wordConceptRepository,
        ICategoryRepository categoryRepository,
        ILanguageRepository languageRepository,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<WordConceptDetailDto> Handle(CreateWordCommand request, CancellationToken ct)
    {
        var concept = new WordConcept
        {
            PartOfSpeech = request.PartOfSpeech,
            DifficultyLevel = request.DifficultyLevel,
            ImageUrl = request.ImageUrl,
        };

        foreach (var translation in request.Translations)
        {
            var language =
                await _languageRepository.GetByCodeAsync(translation.LanguageCode, ct)
                ?? throw new EntityNotFoundException(typeof(Language), translation.LanguageCode);

            if (!request.Force && await _wordConceptRepository.ExistsWordTextAsync(language.Id, translation.Text, ct))
                throw new DuplicateWordException();

            concept.Words.Add(WordEntityBuilder.Build(translation, language, request.UserId));
        }

        // .Distinct() — istemci categoryIds içinde aynı Id'yi iki kez gönderirse UNIQUE index
        // ihlali SaveChangesAsync'te yakalanmayan bir 500'e yol açardı.
        if (request.CategoryIds is not null)
            foreach (var categoryId in request.CategoryIds.Distinct())
            {
                var category =
                    await _categoryRepository.GetByIdAsync(categoryId, ct)
                    ?? throw new EntityNotFoundException(typeof(Category), categoryId);

                concept.WordCategories.Add(
                    new WordCategory
                    {
                        Category = category,
                        CreatedByUserId = request.UserId,
                        UpdatedByUserId = request.UserId,
                    }
                );
            }

        await _wordConceptRepository.AddAsync(concept, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "CREATE_WORD",
            entityType: "WordConcept",
            entityId: concept.Id,
            newValue: new
            {
                concept.PartOfSpeech,
                concept.DifficultyLevel,
                Translations = request.Translations.Select(t => new { t.LanguageCode, t.Text }),
            },
            ct: ct
        );

        return WordConceptDtoBuilder.BuildDetail(concept);
    }
}
