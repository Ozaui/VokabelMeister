using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Admin;

public record UpdateUserRoleCommand(int Id, string Role) : IRequest<Unit>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
    public string? IpAddress { get; init; }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogger _activityLogger;
    private readonly ISecurityLogger _securityLogger;

    public UpdateUserRoleCommandHandler(
        IUserRepository userRepository,
        IActivityLogger activityLogger,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _activityLogger = activityLogger;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(UpdateUserRoleCommand request, CancellationToken ct)
    {
        if (request.Id == request.UserId)
            throw new SelfAdminActionNotAllowedException();

        var user =
            await _userRepository.GetByIdAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.Id);

        var oldRole = user.Role;
        user.Role = request.Role;
        await _userRepository.UpdateAsync(user, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "UPDATE_USER_ROLE",
            entityType: "User",
            entityId: user.Id,
            oldValue: new { Role = oldRole },
            newValue: new { user.Role },
            ipAddress: request.IpAddress,
            ct: ct
        );

        await _securityLogger.LogAsync(
            LogEventType.AdminAction,
            userId: user.Id,
            ipAddress: request.IpAddress,
            detail: "USER_ROLE_CHANGED",
            ct: ct
        );

        return Unit.Value;
    }
}
