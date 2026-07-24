// ─────────────────────────────────────────────────────────────────────────────
// UpdateUserStatusCommand.cs
//
// AMAÇ: PUT /admin/users/{id}/status — bir hesabı dondurur/aktif eder (IsActive).
// NEDEN: UpdateUserRoleCommand ile BİREBİR aynı gerekçeyle hem IActivityLogger
//        (UPDATE_USER_STATUS) hem ISecurityLogger (AdminAction) çağrılır — hesap
//        dondurma da CLAUDE.md'nin "admin'e özel hassas işlem" tanımına girer.
//        `Reason` yalnızca ActivityLog.NewValue'a yazılır (serbest metin admin notu,
//        kullanıcıya gösterilmez — API_ENDPOINTS.md §11 `{ isActive, reason }`).
//        UpdateUserRoleCommand'daki AYNI gerekçeyle: hedef Id kendi Id'siyle
//        (UserId) AYNIYSA reddedilir (SelfAdminActionNotAllowedException, 400) —
//        bir admin kendi hesabını dondurup kilitlenmesin diye.
// BAĞIMLILIKLAR: IUserRepository, IActivityLogger, ISecurityLogger.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Admin;

public record UpdateUserStatusCommand(int Id, bool IsActive, string? Reason) : IRequest<Unit>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
    public string? IpAddress { get; init; }
}

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogger _activityLogger;
    private readonly ISecurityLogger _securityLogger;

    public UpdateUserStatusCommandHandler(
        IUserRepository userRepository,
        IActivityLogger activityLogger,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _activityLogger = activityLogger;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(UpdateUserStatusCommand request, CancellationToken ct)
    {
        if (request.Id == request.UserId)
            throw new SelfAdminActionNotAllowedException();

        var user =
            await _userRepository.GetByIdAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.Id);

        var wasActive = user.IsActive;
        user.IsActive = request.IsActive;
        await _userRepository.UpdateAsync(user, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "UPDATE_USER_STATUS",
            entityType: "User",
            entityId: user.Id,
            oldValue: new { IsActive = wasActive },
            newValue: new { user.IsActive, request.Reason },
            ipAddress: request.IpAddress,
            ct: ct
        );

        await _securityLogger.LogAsync(
            LogEventType.AdminAction,
            userId: user.Id,
            ipAddress: request.IpAddress,
            detail: request.IsActive ? "USER_ACCOUNT_REACTIVATED" : "USER_ACCOUNT_FROZEN",
            ct: ct
        );

        return Unit.Value;
    }
}
