using MediatR;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCards;

public record DeleteUserCardCommand(int UserCardId, int UserId, string? ActorRole) : IRequest<Unit>;

public class DeleteUserCardCommandHandler : IRequestHandler<DeleteUserCardCommand, Unit>
{
    private readonly IUserCardRepository _userCardRepository;
    private readonly IActivityLogger _activityLogger;

    public DeleteUserCardCommandHandler(IUserCardRepository userCardRepository, IActivityLogger activityLogger)
    {
        _userCardRepository = userCardRepository;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteUserCardCommand request, CancellationToken cancellationToken)
    {
        // Sahiplik filtresi burada gömülü — başkasının kartı da AYNI 404'ü döner.
        var aggregate = await _userCardRepository.GetByIdForUserAsync(request.UserCardId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found: Id={request.UserCardId}");
        var beforeResponse = UserCardMapping.ToResponse(aggregate);

        await _userCardRepository.SoftDeleteAsync(aggregate.Card, request.UserId, cancellationToken);
        await _userCardRepository.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "DELETE_USER_CARD", entityType: "UserCard", entityId: request.UserCardId,
            oldValue: beforeResponse, newValue: null, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
