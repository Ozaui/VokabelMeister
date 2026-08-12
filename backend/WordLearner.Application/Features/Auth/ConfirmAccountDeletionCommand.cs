using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Exceptions;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record ConfirmAccountDeletionCommand(int UserId, string OtpCode) : IRequest<Unit>;

public class ConfirmAccountDeletionCommandHandler : IRequestHandler<ConfirmAccountDeletionCommand, Unit>
{
    private const int GraceDays = 30;

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOtpService _otpService;

    public ConfirmAccountDeletionCommandHandler(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _otpService = otpService;
    }

    public async Task<Unit> Handle(ConfirmAccountDeletionCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"User not found: Id={request.UserId}");

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.AccountDeletion);
        if (result == OtpVerificationResult.Expired)
            throw new OtpExpiredException();
        if (result == OtpVerificationResult.InvalidCode)
            throw new InvalidOtpException();

        // Kalıcı anonimleştirme DEĞİL — 30 gün grace period başlar (SECURITY.md §9).
        // AccountCleanupBackgroundService (A-20) süre dolunca PII'yi temizler.
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(GraceDays);
        user.IsActive = false;

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
