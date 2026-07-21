// ─────────────────────────────────────────────────────────────────────────────
// DeleteWordCommand.cs
//
// AMAÇ: DELETE /words/{id} — WordConcept'i ve ona bağlı TÜM Word satırlarını
//       tek işlemde soft-delete eder.
// NEDEN: Yalnızca WordConcept soft-delete edilseydi, Word satırları aktif kalır
//        ve WordConcept'in global soft-delete filtresi Word'leri KAPSAMAZDI (ayrı
//        tablo) — bu tutarsızlığı önlemek için A-05 planı karar #2'ye göre
//        `IWordConceptRepository.SoftDeleteWithWordsAsync` kullanılır.
// BAĞIMLILIKLAR: IWordConceptRepository, IActivityLogger.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public record DeleteWordCommand(int Id) : IRequest<Unit>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class DeleteWordCommandHandler : IRequestHandler<DeleteWordCommand, Unit>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly IActivityLogger _activityLogger;

    public DeleteWordCommandHandler(
        IWordConceptRepository wordConceptRepository,
        IActivityLogger activityLogger
    )
    {
        _wordConceptRepository = wordConceptRepository;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteWordCommand request, CancellationToken ct)
    {
        var concept =
            await _wordConceptRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.Id);

        await _wordConceptRepository.SoftDeleteWithWordsAsync(request.Id, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "DELETE_WORD",
            entityType: "WordConcept",
            entityId: request.Id,
            oldValue: new
            {
                concept.PartOfSpeech,
                concept.DifficultyLevel,
                Translations = concept.Words.Select(w => new { LanguageCode = w.Language.Code, w.Text }),
            },
            ct: ct
        );

        return Unit.Value;
    }
}
