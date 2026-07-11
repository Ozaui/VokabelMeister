// ─────────────────────────────────────────────────────────────────────────────
// ResendVerificationCommand.cs
//
// AMAÇ: POST /auth/resend-verification — e-posta doğrulama kodunu tekrar gönderir.
// NEDEN: Kullanıcı bulunamasa da aynı yanıt döner — e-posta numaralandırma
//        saldırısını önlemek için (ForgotPasswordCommand ile aynı desen).
// BAĞIMLILIKLAR: IUserRepository, IOtpService, IEmailService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Doğrulama kodunun süresi dolduysa/gelmediyse yenisini ister.
// NEDEN Language init-property: bkz. LoginCommand.
public record ResendVerificationCommand(string Email) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
}

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> Handle(ResendVerificationCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is not null && !user.IsEmailVerified)
        {
            var (otpCode, otpHash) = _otpService.Generate();
            user.PendingOtpCodeHash = otpHash;
            user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(IOtpService.OtpExpiryMinutes);
            user.PendingOtpCodePurpose = OtpPurpose.EmailVerification;
            await _userRepository.UpdateAsync(user, ct: ct);
            await _emailService.SendEmailVerificationOtpAsync(user.Email, otpCode, ct);
        }

        return new MessageResponse(
            "VERIFICATION_CODE_SENT",
            SuccessMessages.Resolve("VERIFICATION_CODE_SENT", request.Language)
        );
    }
}
