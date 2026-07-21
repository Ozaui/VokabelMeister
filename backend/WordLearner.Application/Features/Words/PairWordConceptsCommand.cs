// ─────────────────────────────────────────────────────────────────────────────
// PairWordConceptsCommand.cs
//
// AMAÇ: POST /words/pair — `otherConceptId`'nin tek Word'ünü `primaryId`'ye
//       taşıyarak iki eşleşmemiş kavramı tek (2 dilli) kavrama birleştirir.
// NEDEN: `primaryId` = admin'in eşleştirmeyi başlattığı taraf. **Bloklayıcı hata
//        yok** — PartOfSpeech/Category/DifficultyLevel çakışsa bile primaryId'ninki
//        sessizce kazanır (diller arası tür kayması dilin doğası, veri hatası
//        değil — bkz. Icerik.md "Eşleştirme").
// NASIL: 1) İki kavramı da tüm dilleriyle yükle (oldValue snapshot'ı için)
//        2) IWordConceptRepository.PairAsync ile taşı+soft-delete  3) PAIR_WORD_CONCEPTS
//        ActivityLog'u yaz  4) Birleşmiş kavramın detay DTO'sunu dön.
// BAĞIMLILIKLAR: IWordConceptRepository, IActivityLogger, WordConceptDtoBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public record PairWordConceptsCommand(int PrimaryId, int OtherConceptId) : IRequest<WordConceptDetailDto>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class PairWordConceptsCommandHandler : IRequestHandler<PairWordConceptsCommand, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly IActivityLogger _activityLogger;

    public PairWordConceptsCommandHandler(
        IWordConceptRepository wordConceptRepository,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
        _activityLogger = activityLogger;
    }

    public async Task<WordConceptDetailDto> Handle(PairWordConceptsCommand request, CancellationToken ct)
    {
        var primaryBefore =
            await _wordConceptRepository.GetWithTranslationsAsync(request.PrimaryId, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.PrimaryId);
        var otherBefore =
            await _wordConceptRepository.GetWithTranslationsAsync(request.OtherConceptId, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.OtherConceptId);

        var oldValue = new
        {
            Primary = new
            {
                ConceptId = primaryBefore.Id,
                primaryBefore.PartOfSpeech,
                primaryBefore.DifficultyLevel,
                Translations = primaryBefore.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
            },
            Other = new
            {
                ConceptId = otherBefore.Id,
                otherBefore.PartOfSpeech,
                otherBefore.DifficultyLevel,
                Translations = otherBefore.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
            },
        };

        var merged = await _wordConceptRepository.PairAsync(
            request.PrimaryId,
            request.OtherConceptId,
            request.UserId,
            ct
        );

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "PAIR_WORD_CONCEPTS",
            entityType: "WordConcept",
            entityId: merged.Id,
            oldValue: oldValue,
            newValue: new
            {
                merged.PartOfSpeech,
                merged.DifficultyLevel,
                Translations = merged.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
                MergedConceptId = request.OtherConceptId,
            },
            ct: ct
        );

        return WordConceptDtoBuilder.BuildDetail(merged);
    }
}
