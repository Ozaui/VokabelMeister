using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Features.Auth;

public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword, string? DeviceInfo, string? IpAddress, string? Language) : IRequest<Unit>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOtpService _otpService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly ISecurityLogger _securityLogger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOtpService otpService,
        IPasswordService passwordService,
        IEmailService emailService,
        ISecurityLogger securityLogger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _otpService = otpService;
        _passwordService = passwordService;
        _emailService = emailService;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.PasswordReset);
        if (result == OtpVerificationResult.Expired)
        {
            await _securityLogger.LogAsync(LogEventType.OtpFailed, userId: user.Id, email: request.Email,
                ipAddress: request.IpAddress, userAgent: request.DeviceInfo, detail: "OTP_EXPIRED",
                cancellationToken: cancellationToken);
            throw new OtpExpiredException();
        }
        if (result == OtpVerificationResult.InvalidCode)
        {
            await _securityLogger.LogAsync(LogEventType.OtpFailed, userId: user.Id, email: request.Email,
                ipAddress: request.IpAddress, userAgent: request.DeviceInfo, detail: "INVALID_OTP",
                cancellationToken: cancellationToken);
            throw new InvalidOtpException();
        }

        user.PasswordHash = _passwordService.Hash(request.NewPassword);

        // SECURITY.md §7: şifre sıfırlanınca tüm cihazlardan çıkış — çalınmış bir oturum varsa da kapanır.
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _securityLogger.LogAsync(LogEventType.PasswordReset, userId: user.Id, email: user.Email,
            ipAddress: request.IpAddress, userAgent: request.DeviceInfo, detail: "PASSWORD_RESET",
            cancellationToken: cancellationToken);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);
        return Unit.Value;
    }
}
