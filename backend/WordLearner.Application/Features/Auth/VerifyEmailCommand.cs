using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record VerifyEmailCommand(string Email, string OtpCode) : IRequest<Unit>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public VerifyEmailCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // E-posta bulunamazsa da AYNI hata: hangi e-postanın kayıtlı olduğunu sızdırmamak için.
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.EmailVerification);
        if (result == OtpVerificationResult.Expired)
            throw new OtpExpiredException();
        if (result == OtpVerificationResult.InvalidCode)
            throw new InvalidOtpException();

        user.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
