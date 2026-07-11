// ─────────────────────────────────────────────────────────────────────────────
// ForgotPasswordCommand.cs
//
// AMAÇ: POST /auth/forgot-password — şifre sıfırlama OTP'si gönderir.
// NEDEN: Kullanıcı yoksa/anonimleştirilmişse bile AYNI yanıt döner — e-posta
//        numaralandırma saldırısını önler (REFERENCE/SECURITY.md §7).
// BAĞIMLILIKLAR: IUserRepository, IOtpService, IEmailService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Kullanıcı yoksa bile aynı yanıt döner (e-posta numaralandırma önlemi).
// NEDEN Language init-property: bkz. LoginCommand.
public record ForgotPasswordCommand(string Email) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is not null && !user.IsAnonymized)
        {
            var (otpCode, otpHash) = _otpService.Generate();
            user.PendingOtpCodeHash = otpHash;
            user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(IOtpService.OtpExpiryMinutes);
            user.PendingOtpCodePurpose = OtpPurpose.PasswordReset;
            await _userRepository.UpdateAsync(user, user.Id, ct);
            await _emailService.SendPasswordResetOtpAsync(user.Email, otpCode, ct);
        }

        return new MessageResponse(
            "PASSWORD_RESET_OTP_SENT",
            SuccessMessages.Resolve("PASSWORD_RESET_OTP_SENT", request.Language)
        );
    }
}
