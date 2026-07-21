// ─────────────────────────────────────────────────────────────────────────────
// UpdateWordCommand.cs
//
// AMAÇ: PUT /words/{id} — mevcut çevirileri günceller veya kavrama eksik olan
//       dili ekler (eşleşmemiş bir kavramı eşleştirebilir).
// NEDEN: `Translations` listesindeki her dil, kavramda zaten VARSA güncellenir,
//        YOKSA yeni bir Word olarak eklenir (WordEntityBuilder ile CreateWord'le
//        aynı kurma mantığı) — bu da Icerik.md "Eşleştirme"deki "translations[]
//        ile tek istekte iki dil de girilebilir" akışının bir parçası.
// NEDEN PartOfSpeech/DifficultyLevel ZORUNLU (nullable DEĞİL, CreateWordCommand'la
//        birebir aynı): WordGrammarValidator, translation'ları PartOfSpeech'e göre
//        doğrular — bu alan opsiyonel olsaydı (PATCH-tarzı kısmi güncelleme),
//        validator "hangi türe göre doğrulayayım" sorusuna DB'ye gitmeden CEVAP
//        VEREMEZDİ. PUT bu yüzden concept-seviyesi alanlarda TAM YER DEĞİŞTİRME
//        semantiğine sahip (admin formu zaten mevcut değerleri önceden DOLDURUR).
// NASIL: 1) Kavramı tüm dilleriyle yükle  2) Concept-seviyesi alanları güncelle
//        3) Her translation için: dil zaten varsa alanlarını güncelle, yoksa
//        duplikat kontrolüyle yeni Word ekle  4) UPDATE_WORD ActivityLog'u yaz.
// BAĞIMLILIKLAR: IWordConceptRepository, ILanguageRepository, IActivityLogger,
//                WordEntityBuilder, WordConceptDtoBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public record UpdateWordCommand(
    int Id,
    string PartOfSpeech,
    string DifficultyLevel,
    string? ImageUrl,
    IReadOnlyList<WordTranslationInput> Translations
) : IRequest<WordConceptDetailDto>
{
    // NEDEN init-property: CreateWordCommand.Force ile aynı gerekçe — query string'ten gelir.
    public bool Force { get; init; }

    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class UpdateWordCommandHandler : IRequestHandler<UpdateWordCommand, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateWordCommandHandler(
        IWordConceptRepository wordConceptRepository,
        ILanguageRepository languageRepository,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<WordConceptDetailDto> Handle(UpdateWordCommand request, CancellationToken ct)
    {
        var concept =
            await _wordConceptRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.Id);

        var oldValue = new
        {
            concept.PartOfSpeech,
            concept.DifficultyLevel,
            Translations = concept.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
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
                // NEDEN: Bu dil kavramda henüz yok — yeni ekleniyor (eşleşmemiş bir
                //        kavramı eşleştirmenin bir yolu). Aynı duplikat kontrolü
                //        CreateWordCommand ile birebir aynı (Force ile bypass edilebilir).
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

            // NEDEN yalnızca EKLEME: mevcut örnekleri silme/eşleştirme burada
            // kapsam dışı bırakıldı (YAGNI) — hangi örneğin "aynı" sayılacağı
            // (SentenceText tam eşleşmesi mi, id bazlı mı) API_ENDPOINTS.md §5'te
            // netleşmemiş; admin yeni örnek eklemek istediğinde bu yeterli, mevcut
            // örnekleri düzenlemek/silmek ayrı bir endpoint'e bırakılabilir.
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
