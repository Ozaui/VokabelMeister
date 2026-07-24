// ─────────────────────────────────────────────────────────────────────────────
// UpdateUserRoleCommand.cs
//
// AMAÇ: PUT /admin/users/{id}/role — bir kullanıcının rolünü değiştirir (User↔Admin).
// NEDEN: Rol değişimi CLAUDE.md "Kimlik & güvenlik"nin kapsadığı hassas bir admin
//        işlemi — CLAUDE.md "İçerik değiştiren her CRUD..." kuralına göre HEM
//        IActivityLogger (UPDATE_USER_ROLE, genel "kim ne yaptı" izi) HEM
//        ISecurityLogger (LogEventType.AdminAction, güvenlik olayı izi) çağrılır.
//        Hedef Id, isteği yapan adminin kendi Id'siyle (UserId) AYNIYSA reddedilir
//        (SelfAdminActionNotAllowedException, 400) — kaza sonucu kendi rolünü
//        düşürmek, tek admin'li bir sistemde geri dönüşü olmayan bir kilitlenmeye
//        yol açabilir (kod denetiminde bulunan bir açık tasarım sorusu, kullanıcı
//        onayıyla eklendi).
// BAĞIMLILIKLAR: IUserRepository, IActivityLogger, ISecurityLogger.
// ─────────────────────────────────────────────────────────────────────────────

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
