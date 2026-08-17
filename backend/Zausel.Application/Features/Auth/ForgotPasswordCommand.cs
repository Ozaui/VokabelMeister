using MediatR;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Application.Features.Auth;

public record ForgotPasswordCommand(string Email, string? Language) : IRequest<Unit>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUserRepository userRepository, IOtpService otpService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // SECURITY.md §7: kullanıcı yoksa bile 200 — e-posta enumerasyonu önlenir. AuthProvider/
        // PasswordHash'e BAKILMAZ, sosyal hesap da OTP'yle şifre belirleyebilir (kasıtlı, SECURITY.md §1.2).
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Unit.Value;

        var otpCode = _otpService.Generate(user, OtpPurpose.PasswordReset);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetAsync(user.Email, user.FirstName, otpCode, request.Language, cancellationToken);
        return Unit.Value;
    }
}
