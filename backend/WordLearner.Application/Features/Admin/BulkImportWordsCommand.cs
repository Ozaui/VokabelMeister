// ─────────────────────────────────────────────────────────────────────────────
// BulkImportWordsCommand.cs
//
// AMAÇ: POST /admin/words/import — çok sayıda kelimeyi TEK istekte, satır bazlı
//       (best-effort) olarak içe aktarır.
// NEDEN her satır TEK dilli bir WordConcept: Icerik.md "Eşleştirme" bölümünün
//        kararı — "de ve tr içeriği ayrı ayrı, kendi toplu import akışlarıyla
//        girilir" (795+ satırlık gerçek içerik girişi senaryosu). A-05'teki
//        CreateWordCommand'ın translations[] (1 VEYA 2 dil TEK işlemde) deseninin
//        AKSİNE, bu komut HER satırı BAĞIMSIZ bir WordConcept olarak açar —
//        eşleştirme (pairing), A-05'te zaten yazılan GET /words/unmatched +
//        POST /words/pair akışına SONRADAN bırakılır.
// NEDEN best-effort (bir satır hatalıysa TÜMÜ reddedilmez): 795 satırlık bir
//        importta TEK bir yazım hatası (ör. eksik PartOfSpeech) yüzünden diğer
//        794 satırın da reddedilmesi, admin'in hatayı bulup TÜM dosyayı yeniden
//        yüklemesini gerektirirdi — bunun yerine her satır kendi başına
//        değerlendirilir, başarısız satırlar `BulkImportResultDto.Results`'ta
//        RowIndex+ErrorCode ile raporlanır, admin yalnızca o satırları düzeltir.
// NASIL: Her satır için WordGrammarValidator (A-05) + temel zorunluluk kontrolleri
//        + duplikat kontrolü (Force YOK — bulk import'ta duplikat SESSİZCE
//        atlanır, tek tek ?force=true kararı vermek 795 satırda anlamsız) →
//        başarılıysa WordEntityBuilder.Build (A-05) ile WordConcept+Word ağacı
//        kurulup kaydedilir. Tüm satırlar işlendikten SONRA TEK bir
//        BULK_IMPORT_WORDS ActivityLog kaydı yazılır (795 ayrı CREATE_WORD kaydı
//        DEĞİL — admin panelin aktivite akışını (B-08) BOĞMAMAK için, bkz.
//        TASK/A_admin_panel_backend.md A-07 notu).
// BAĞIMLILIKLAR: IWordConceptRepository, ICategoryRepository, ILanguageRepository,
//                IValidator<WordGrammarInput> (A-05), IActivityLogger, WordEntityBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using FluentValidation;
using MediatR;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Validators.Words;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;
using WordLearner.Application.Features.Words;

namespace WordLearner.Application.Features.Admin;

// AMAÇ: Bir içe aktarma satırı — WordConcept-seviyesi alanlar (PartOfSpeech/
//       DifficultyLevel/ImageUrl) + A-05'in WordTranslationInput'u (TEK dil).
// NEDEN WordTranslationInput YENİDEN KULLANILDI (yeni bir tip AÇILMADI): Text/
//       Definition/WordDetail/Examples alanları CreateWordCommand'daki ile
//       BİREBİR aynı — WordEntityBuilder.Build de zaten bu tipi bekliyor.
public record BulkImportWordRow(
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    WordTranslationInput Translation,
    IReadOnlyList<int>? CategoryIds = null
);

// AMAÇ: Bir satırın içe aktarma sonucu — admin panelin hangi satırın neden
//       BAŞARISIZ olduğunu gösterebilmesi için LanguageCode/Text de taşınır.
public record BulkImportRowResultDto(int RowIndex, string LanguageCode, string Text, bool Success, string? ErrorCode);

// AMAÇ: `POST /admin/words/import` yanıtı — toplam/başarılı/atlanan sayısı + satır bazlı detay.
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

    // AMAÇ: Tek bir satırı içe aktarmayı dener. Başarılıysa null, değilse bir hata
    //       kodu döner — CreateWordCommandHandler'ın AKSİNE hiçbir exception
    //       FIRLATMAZ (bir satırın hatası diğer satırların işlenmesini DURDURMAMALI).
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
