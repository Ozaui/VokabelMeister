using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword, string? Language) : IRequest<Unit>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOtpService _otpService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOtpService otpService,
        IPasswordService passwordService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _otpService = otpService;
        _passwordService = passwordService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.PasswordReset);
        if (result == OtpVerificationResult.Expired)
            throw new OtpExpiredException();
        if (result == OtpVerificationResult.InvalidCode)
            throw new InvalidOtpException();

        user.PasswordHash = _passwordService.Hash(request.NewPassword);

        // SECURITY.md §7: şifre sıfırlanınca tüm cihazlardan çıkış — çalınmış bir oturum varsa da kapanır.
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);
        return Unit.Value;
    }
}
