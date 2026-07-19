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
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

// NEDEN UserId/Language/ClientIp init-property: bkz. LogoutCommand — JWT'den ve
//       Accept-Language header'ından gelir, gövdede yer almaz. ClientIp A-04'te
//       OtpFailed/AccountDeletion SecurityLog kayıtları için eklendi.
public record ConfirmAccountDeletionCommand(string OtpCode, string Password)
    : IRequest<MessageResponse>
{
    public int UserId { get; init; }
    public string? Language { get; init; }
    public string? ClientIp { get; init; }
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
    private readonly ISecurityLogger _securityLogger;

    public ConfirmAccountDeletionCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _securityLogger = securityLogger;
    }

    public async Task<MessageResponse> Handle(
        ConfirmAccountDeletionCommand request,
        CancellationToken ct
    )
    {
        var user =
            await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.UserId);

        try
        {
            _otpService.Validate(user, request.OtpCode, OtpPurpose.AccountDeletion);
        }
        catch (InvalidOtpException)
        {
            await _securityLogger.LogAsync(
                LogEventType.OtpFailed,
                user.Id,
                ipAddress: request.ClientIp,
                detail: "AccountDeletion",
                ct: ct
            );
            throw;
        }

        if (
            !_passwordService.Verify(
                request.Password,
                user.PasswordHash ?? FakePasswordHashForTiming
            )
        )
        {
            // NEDEN Detail bir KOD: bkz. RefreshCommand.cs'teki aynı NEDEN notu — admin panel
            //       kendi dil tercihine göre görüntüler (A-07'de tr/de sözlükten çözülür),
            //       burada yalnızca sabit bir Code saklanır.
            await _securityLogger.LogAsync(
                LogEventType.LoginFailed,
                user.Id,
                ipAddress: request.ClientIp,
                detail: "ACCOUNT_DELETION_PASSWORD_MISMATCH",
                ct: ct
            );
            throw new InvalidCredentialsException();
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(AccountDeletionGraceDays);
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, user.Id, ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);

        await _securityLogger.LogAsync(
            LogEventType.AccountDeletion,
            user.Id,
            ipAddress: request.ClientIp,
            ct: ct
        );

        return new MessageResponse(
            "ACCOUNT_DELETION_CONFIRMED",
            SuccessMessages.Resolve("ACCOUNT_DELETION_CONFIRMED", request.Language)
        );
    }
}
