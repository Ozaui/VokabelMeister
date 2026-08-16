using MediatR;
using WordLearner.Application.Interfaces.Repositories.Content;

namespace WordLearner.Application.Features.Words;

public record DeleteWordCommand(int WordConceptId, int? UserId) : IRequest<Unit>;

public class DeleteWordCommandHandler : IRequestHandler<DeleteWordCommand, Unit>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public DeleteWordCommandHandler(IWordConceptRepository wordConceptRepository) => _wordConceptRepository = wordConceptRepository;

    public async Task<Unit> Handle(DeleteWordCommand request, CancellationToken cancellationToken)
    {
        // Kavram + tüm dillerdeki Word/WordDetail/WordExample TEK bir kademeli soft-delete'te —
        // WordConceptId FK'i Words'te CASCADE (gerçek DELETE için), ama bu soft-delete, EF'in
        // otomatik soft-delete filtresi CASCADE'i TAKLİT ETMEZ, repository elle yürütüyor.
        await _wordConceptRepository.SoftDeleteConceptCascadeAsync(request.WordConceptId, request.UserId, cancellationToken);
        await _wordConceptRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
