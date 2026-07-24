using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

// PartOfSpeech/DifficultyLevel zorunlu (nullable değil) — WordGrammarValidator translation'ları
// PartOfSpeech'e göre doğrular, opsiyonel olsaydı validator hangi türe göre doğrulayacağını bilemezdi.
// PUT bu yüzden concept-seviyesi alanlarda tam yer değiştirme semantiğine sahiptir.
public record UpdateWordCommand(
    int Id,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationInput> Translations,
    // Null = dokunma, boş liste = tümünü kaldır.
    IReadOnlyList<int>? CategoryIds = null
) : IRequest<WordConceptDetailDto>
{
    public bool Force { get; init; }
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class UpdateWordCommandHandler : IRequestHandler<UpdateWordCommand, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateWordCommandHandler(
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

    public async Task<WordConceptDetailDto> Handle(UpdateWordCommand request, CancellationToken ct)
    {
        var concept =
            await _wordConceptRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.Id);

        // .ToList() — Select deferred'dır; aşağıdaki döngü aynı Word nesnelerini mutasyona uğratır.
        // Materyalize edilmezse LogAsync'teki JsonSerializer bu listeyi mutasyonlardan SONRA
        // enumerate eder ve "eski" değer olarak yeni değerleri yazardı.
        var oldValue = new
        {
            concept.PartOfSpeech,
            concept.DifficultyLevel,
            Translations = concept.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }).ToList(),
        };

        concept.PartOfSpeech = request.PartOfSpeech;
        concept.DifficultyLevel = request.DifficultyLevel;
        if (request.ImageUrl is not null)
            concept.ImageUrl = request.ImageUrl;

        foreach (var translation in request.Translations)
        {
            var language =
                await _languageRepository.GetByCodeAsync(translation.LanguageCode, ct)
                ?? throw new EntityNotFoundException(typeof(Language), translation.LanguageCode);

            var existingWord = concept.Words.FirstOrDefault(w => w.LanguageId == language.Id);

            if (existingWord is null)
            {
                // Bu dil kavramda henüz yok — eşleşmemiş bir kavramı eşleştirmenin yolu.
                if (
                    !request.Force
                    && await _wordConceptRepository.ExistsWordTextAsync(language.Id, translation.Text, ct)
                )
                    throw new DuplicateWordException();

                concept.Words.Add(WordEntityBuilder.Build(translation, language, request.UserId));
                continue;
            }

            existingWord.Text = translation.Text;
            existingWord.Definition = translation.Definition;
            existingWord.UpdatedByUserId = request.UserId;

            if (translation.WordDetail is not null)
            {
                if (existingWord.WordDetail is null)
                    existingWord.WordDetail = WordEntityBuilder.BuildWordDetail(translation.WordDetail, request.UserId);
                else
                {
                    existingWord.WordDetail.Pronunciation = translation.WordDetail.Pronunciation;
                    existingWord.WordDetail.AudioUrl = translation.WordDetail.AudioUrl;
                    existingWord.WordDetail.Notes = translation.WordDetail.Notes;
                    existingWord.WordDetail.CommonMistakes = translation.WordDetail.CommonMistakes;
                    existingWord.WordDetail.GrammarData = translation.WordDetail.GrammarData?.GetRawText();
                    existingWord.WordDetail.UpdatedByUserId = request.UserId;
                }
            }

            // Yalnızca ekleme — mevcut örnekleri silme/eşleştirme kapsam dışı (YAGNI).
            if (translation.Examples is not null)
            {
                var displayOrder = existingWord.WordExamples.Count;
                foreach (var example in translation.Examples)
                {
                    existingWord.WordExamples.Add(
                        new WordExample
                        {
                            SentenceText = example.SentenceText,
                            Level = example.Level,
                            ExampleType = example.ExampleType,
                            DisplayOrder = displayOrder++,
                            CreatedByUserId = request.UserId,
                            UpdatedByUserId = request.UserId,
                        }
                    );
                }
            }
        }

        // Tam yer değiştirme — yeni listede olmayan mevcut bağlar kaldırılır, olmayanlar eklenir.
        // .Distinct() — tekrarlanan bir Id iki kez eklenmeye çalışılmasını (ve UNIQUE index'ten
        // yakalanmayan 500'ü) önler.
        if (request.CategoryIds is not null)
        {
            var newCategoryIds = request.CategoryIds.Distinct().ToList();
            var toRemove = concept.WordCategories.Where(wc => !newCategoryIds.Contains(wc.CategoryId)).ToList();
            foreach (var wordCategory in toRemove)
                concept.WordCategories.Remove(wordCategory);

            var existingCategoryIds = concept.WordCategories.Select(wc => wc.CategoryId).ToHashSet();
            foreach (var categoryId in newCategoryIds.Where(id => !existingCategoryIds.Contains(id)))
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
        }

        await _wordConceptRepository.UpdateAsync(concept, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "UPDATE_WORD",
            entityType: "WordConcept",
            entityId: concept.Id,
            oldValue: oldValue,
            newValue: new
            {
                concept.PartOfSpeech,
                concept.DifficultyLevel,
                Translations = concept.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
            },
            ct: ct
        );

        return WordConceptDtoBuilder.BuildDetail(concept);
    }
}
