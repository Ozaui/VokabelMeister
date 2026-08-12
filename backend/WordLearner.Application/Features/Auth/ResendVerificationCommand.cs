using MediatR;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record ResendVerificationCommand(string Email, string? Language) : IRequest<Unit>;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(IUserRepository userRepository, IOtpService otpService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        // ForgotPassword ile aynı enumeration-önleme deseni: kayıtlı olmayan/zaten doğrulanmış
        // e-postada da 200 döner, kod ÜRETİLMEZ/gönderilmez — istemci fark edemez.
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || user.IsEmailVerified)
            return Unit.Value;

        var otpCode = _otpService.Generate(user, OtpPurpose.EmailVerification);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, otpCode, request.Language, cancellationToken);
        return Unit.Value;
    }
}
