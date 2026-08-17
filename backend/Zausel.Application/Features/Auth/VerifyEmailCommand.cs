using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Application.Features.Auth;

public record VerifyEmailCommand(string Email, string OtpCode, string? DeviceInfo, string? IpAddress) : IRequest<Unit>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISecurityLogger _securityLogger;

    public VerifyEmailCommandHandler(IUserRepository userRepository, IOtpService otpService, ISecurityLogger securityLogger)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _securityLogger = securityLogger;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // E-posta bulunamazsa da AYNI hata: hangi e-postanın kayıtlı olduğunu sızdırmamak için.
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.EmailVerification);
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

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
