// ─────────────────────────────────────────────────────────────────────────────
// CreateWordCommand.cs
//
// AMAÇ: POST /words — bir WordConcept'i 1 veya 2 dilde (translations[]) tek
//       işlemde oluşturur.
// NEDEN: Tek dilse kavram "eşleşmemiş" kalır (bkz. Icerik.md "Eşleştirme") —
//        ayrı bir IsMatched kolonu yok, bu durum COUNT(Words)'ten türetilir.
//        Aynı dilde aynı Text zaten varsa (Force=false) DuplicateWordException
//        (409); Force=true ise çakışmaya rağmen oluşturulur.
// NASIL: 1) Her translation için Language'ı çöz  2) Force değilse duplikat kontrolü
//        3) WordConcept + Word(+WordDetail+WordExample) ağacını kur  4) Tek
//        AddAsync ile kaydet (EF child'ları da cascade insert eder)  5) CREATE_WORD
//        ActivityLog'u yaz  6) Detay DTO'sunu dön.
// BAĞIMLILIKLAR: IWordConceptRepository, ILanguageRepository, IActivityLogger,
//                WordConceptDtoBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;
using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

// AMAÇ: Bir dildeki tek bir örnek cümle girdisi.
public record WordExampleInput(string SentenceText, string Level, string ExampleType = "Normal");

// AMAÇ: Bir dildeki gramer/telaffuz girdisi — GrammarData ham JSON olarak taşınır
//       (WordGrammarValidator bunu dile/türe göre doğrular, bkz. Validators/Words/).
public record WordDetailInput(
    string? Pronunciation,
    string? AudioUrl,
    string? Notes,
    string? CommonMistakes,
    JsonElement? GrammarData
);

// AMAÇ: Bir WordConcept'in tek bir dildeki girdisi.
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
    IReadOnlyList<WordTranslationInput> Translations
) : IRequest<WordConceptDetailDto>
{
    // NEDEN init-property (gövdede DEĞİL): API_ENDPOINTS.md §5 `?force=true`'yu bir
    //        QUERY string parametresi olarak tanımlıyor, body alanı değil — controller
    //        bunu `[FromQuery]`'den okuyup `with { Force = force }` ile ekler.
    public bool Force { get; init; }

    // NEDEN init-property: JWT'den (CurrentUserId/Role) gelir, gövdede yer almaz —
    //        controller model binding'den SONRA `with` ile ekler (LogoutCommand deseni).
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class CreateWordCommandHandler : IRequestHandler<CreateWordCommand, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public CreateWordCommandHandler(
        IWordConceptRepository wordConceptRepository,
        ILanguageRepository languageRepository,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
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
