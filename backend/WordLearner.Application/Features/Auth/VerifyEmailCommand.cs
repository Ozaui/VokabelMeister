// ─────────────────────────────────────────────────────────────────────────────
// VerifyEmailCommand.cs
//
// AMAÇ: POST /auth/verify-email — kayıt sonrası e-postaya gelen OTP kodunu
//       doğrular, hesabı aktive eder.
// BAĞIMLILIKLAR: IUserRepository, IOtpService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Kayıt sonrası e-postaya gelen 6 haneli kodu doğrular.
// NEDEN Language init-property: bkz. LoginCommand.
public record VerifyEmailCommand(string Email, string OtpCode) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public VerifyEmailCommandHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<MessageResponse> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        _otpService.Validate(user, request.OtpCode, OtpPurpose.EmailVerification);

        user!.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        return new MessageResponse("EMAIL_VERIFIED", SuccessMessages.Resolve("EMAIL_VERIFIED", request.Language));
    }
}
