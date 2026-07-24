using FluentValidation;
using MediatR;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Validators.Words;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Features.Admin;

// Her satır TEK dilli bir WordConcept açar — eşleştirme (pairing) sonradan GET /words/unmatched
// + POST /words/pair akışına bırakılır.
public record BulkImportWordRow(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    WordTranslationInput Translation,
    IReadOnlyList<int>? CategoryIds = null
);

public record BulkImportRowResultDto(int RowIndex, string LanguageCode, string Text, bool Success, string? ErrorCode);

public record BulkImportResultDto(
    int TotalRows,
    int ImportedCount,
    int SkippedCount,
    IReadOnlyList<BulkImportRowResultDto> Results
);

public record BulkImportWordsCommand(IReadOnlyList<BulkImportWordRow> Rows) : IRequest<BulkImportResultDto>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class BulkImportWordsCommandHandler : IRequestHandler<BulkImportWordsCommand, BulkImportResultDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IValidator<WordGrammarInput> _grammarValidator;
    private readonly IActivityLogger _activityLogger;

    public BulkImportWordsCommandHandler(
        IWordConceptRepository wordConceptRepository,
        ICategoryRepository categoryRepository,
        ILanguageRepository languageRepository,
        IValidator<WordGrammarInput> grammarValidator,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _grammarValidator = grammarValidator;
        _activityLogger = activityLogger;
    }

    public async Task<BulkImportResultDto> Handle(BulkImportWordsCommand request, CancellationToken ct)
    {
        var results = new List<BulkImportRowResultDto>(request.Rows.Count);

        for (var i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var errorCode = await TryImportRowAsync(row, request.UserId, ct);
            results.Add(new BulkImportRowResultDto(i, row.Translation.LanguageCode, row.Translation.Text, errorCode is null, errorCode));
        }

        var importedCount = results.Count(r => r.Success);
        var skippedCount = results.Count - importedCount;

        // Tek bir toplu ActivityLog kaydı — 795 ayrı CREATE_WORD kaydı admin panelin
        // aktivite akışını boğardı.
        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "BULK_IMPORT_WORDS",
            entityType: "WordConcept",
            newValue: new { TotalRows = request.Rows.Count, ImportedCount = importedCount, SkippedCount = skippedCount },
            ct: ct
        );

        return new BulkImportResultDto(request.Rows.Count, importedCount, skippedCount, results);
    }

    // Başarılıysa null, değilse hata kodu döner — CreateWordCommandHandler'ın aksine hiçbir
    // exception fırlatmaz, bir satırın hatası diğerlerini durdurmamalı.
    private async Task<string?> TryImportRowAsync(BulkImportWordRow row, int? userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(row.Translation.LanguageCode))
            return "LANGUAGE_CODE_REQUIRED";
        if (string.IsNullOrWhiteSpace(row.Translation.Text))
            return "WORD_TEXT_REQUIRED";
        if (string.IsNullOrWhiteSpace(row.PartOfSpeech))
            return "PART_OF_SPEECH_REQUIRED";
        if (string.IsNullOrWhiteSpace(row.DifficultyLevel))
            return "DIFFICULTY_LEVEL_REQUIRED";

        var grammarResult = _grammarValidator.Validate(
            new WordGrammarInput(row.Translation.LanguageCode, row.PartOfSpeech, row.Translation.WordDetail?.GrammarData?.GetRawText())
        );
        if (!grammarResult.IsValid)
            return grammarResult.Errors[0].ErrorCode;

        var language = await _languageRepository.GetByCodeAsync(row.Translation.LanguageCode, ct);
        if (language is null)
            return "LANGUAGE_NOT_FOUND";

        if (await _wordConceptRepository.ExistsWordTextAsync(language.Id, row.Translation.Text, ct))
            return "WORD_TEXT_ALREADY_EXISTS";

        var concept = new WordConcept
        {
            PartOfSpeech = row.PartOfSpeech,
            DifficultyLevel = row.DifficultyLevel,
            ImageUrl = row.ImageUrl,
        };
        concept.Words.Add(WordEntityBuilder.Build(row.Translation, language, userId));

        if (row.CategoryIds is not null)
            foreach (var categoryId in row.CategoryIds.Distinct())
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId, ct);
                if (category is null)
                    return "CATEGORY_NOT_FOUND";

                concept.WordCategories.Add(
                    new WordCategory
                    {
                        Category = category,
                        CreatedByUserId = userId,
                        UpdatedByUserId = userId,
                    }
                );
            }

        await _wordConceptRepository.AddAsync(concept, userId, ct);
        return null;
    }
}
