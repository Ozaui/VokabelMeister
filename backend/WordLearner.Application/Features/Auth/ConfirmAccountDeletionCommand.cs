// ─────────────────────────────────────────────────────────────────────────────
// ConfirmAccountDeletionCommand.cs
//
// AMAÇ: POST /auth/delete-account/confirm — OTP + şifre ile hesap silmeyi onaylar;
//       soft delete yapar, 30 gün sonra kalıcı anonimleştirme için zamanlar
//       (AccountCleanupBackgroundService, A-10).
// NEDEN: Geri alınamaz bir işlem olduğu için OTP'ye ek olarak şifre de istenir (çift onay).
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService, IOtpService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// NEDEN UserId init-property: bkz. LogoutCommand — JWT'den gelir, gövdede yer almaz.
public record ConfirmAccountDeletionCommand(string OtpCode, string Password) : IRequest<MessageResponse>
{
    public int UserId { get; init; }
}

public class ConfirmAccountDeletionCommandHandler
    : IRequestHandler<ConfirmAccountDeletionCommand, MessageResponse>
{
    // NEDEN 30 gün: REFERENCE/SECURITY.md §9 — hesap silme grace period.
    private const int AccountDeletionGraceDays = 30;

    // NEDEN static + doğrudan BCrypt.Net: bkz. LoginCommandHandler'daki aynı alanın notu —
    //       bilinçli, küçük bir tekrar (2 bağımsız kullanım).
    private static readonly string FakePasswordHashForTiming = BCrypt.Net.BCrypt.HashPassword(
        Guid.NewGuid().ToString(),
        workFactor: 12
    );

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;

    public ConfirmAccountDeletionCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        IOtpService otpService
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _otpService = otpService;
    }

    public async Task<MessageResponse> Handle(ConfirmAccountDeletionCommand request, CancellationToken ct)
    {
        var user =
            await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.UserId);

        _otpService.Validate(user, request.OtpCode, OtpPurpose.AccountDeletion);

        if (!_passwordService.Verify(request.Password, user.PasswordHash ?? FakePasswordHashForTiming))
            throw new InvalidCredentialsException();

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(AccountDeletionGraceDays);
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);

        return new MessageResponse("Hesabınız silindi. 30 gün içinde tekrar giriş yaparak geri alabilirsiniz.");
    }
}
