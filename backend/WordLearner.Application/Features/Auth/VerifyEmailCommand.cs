// ─────────────────────────────────────────────────────────────────────────────
// VerifyEmailCommand.cs
//
// AMAÇ: POST /auth/verify-email — kayıt sonrası e-postaya gelen OTP kodunu
//       doğrular, hesabı aktive eder.
// BAĞIMLILIKLAR: IUserRepository, IOtpService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Kayıt sonrası e-postaya gelen 6 haneli kodu doğrular.
// NEDEN Language/ClientIp init-property: bkz. LoginCommand — ClientIp A-04'te
//       OtpFailed SecurityLog kaydı için eklendi.
public record VerifyEmailCommand(string Email, string OtpCode) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
    public string? ClientIp { get; init; }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ISecurityLogger _securityLogger;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _securityLogger = securityLogger;
    }

    public async Task<MessageResponse> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        try
        {
            _otpService.Validate(user, request.OtpCode, OtpPurpose.EmailVerification);
        }
        catch (InvalidOtpException)
        {
            await _securityLogger.LogAsync(
                LogEventType.OtpFailed,
                user?.Id,
                request.Email,
                request.ClientIp,
                detail: "EmailVerification",
                ct: ct
            );
            throw;
        }

        user!.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, user.Id, ct);

        return new MessageResponse("EMAIL_VERIFIED", SuccessMessages.Resolve("EMAIL_VERIFIED", request.Language));
    }
}
