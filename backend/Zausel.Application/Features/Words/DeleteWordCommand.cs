using MediatR;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

public record DeleteWordCommand(int WordConceptId, int? UserId, string? ActorRole) : IRequest<Unit>;

public class DeleteWordCommandHandler : IRequestHandler<DeleteWordCommand, Unit>
{
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly IActivityLogger _activityLogger;

    public DeleteWordCommandHandler(IWordConceptRepository wordConceptRepository, IActivityLogger activityLogger)
    {
        _wordConceptRepository = wordConceptRepository;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteWordCommand request, CancellationToken cancellationToken)
    {
        // Silinmeden ÖNCEKİ hâli yakalanıyor — soft-delete'ten SONRA GetAggregateAsync (soft-delete
        // filtresi yüzünden) bu kavramı BULAMAZ, ActivityLog'un OldValue'su bu yüzden ÖNCE okunuyor.
        var beforeAggregate = await _wordConceptRepository.GetAggregateAsync(request.WordConceptId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={request.WordConceptId}");
        var beforeResponse = WordMapping.ToResponse(beforeAggregate);

        // Kavram + tüm dillerdeki Word/WordDetail/WordExample TEK bir kademeli soft-delete'te —
        // WordConceptId FK'i Words'te CASCADE (gerçek DELETE için), ama bu soft-delete, EF'in
        // otomatik soft-delete filtresi CASCADE'i TAKLİT ETMEZ, repository elle yürütüyor.
        await _wordConceptRepository.SoftDeleteConceptCascadeAsync(request.WordConceptId, request.UserId, cancellationToken);
        await _wordConceptRepository.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "DELETE_WORD", entityType: "Word", entityId: request.WordConceptId,
            oldValue: beforeResponse, newValue: null, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
