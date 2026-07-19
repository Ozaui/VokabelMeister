// ─────────────────────────────────────────────────────────────────────────────
// VerifyLoginOtpCommand.cs
//
// AMAÇ: POST /auth/login/verify-otp — Login adım 2: OTP'yi doğrular, başarılıysa
//       access+refresh token üretir.
// BAĞIMLILIKLAR: IUserRepository, IOtpService, ILoginCompletionService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Login adım 2 (ve QR/sosyal giriş sonrası paylaşılan OTP şekli) — e-postaya
//       gelen 6 haneli kodu doğrular, başarılıysa token üretilir.
// NEDEN ClientIp init-property: İstek gövdesinde gelmez — controller,
//       HttpContext'ten okuduğu değeri model binding'den SONRA `with` ile ekler.
public record VerifyLoginOtpCommand(string Email, string OtpCode) : IRequest<AuthTokenResponse>
{
    public string? ClientIp { get; init; }
}

public class VerifyLoginOtpCommandHandler : IRequestHandler<VerifyLoginOtpCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly ISecurityLogger _securityLogger;

    public VerifyLoginOtpCommandHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        ILoginCompletionService loginCompletionService,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _loginCompletionService = loginCompletionService;
        _securityLogger = securityLogger;
    }

    public async Task<AuthTokenResponse> Handle(VerifyLoginOtpCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        try
        {
            _otpService.Validate(user, request.OtpCode, OtpPurpose.LoginOtp);
        }
        catch (InvalidOtpException)
        {
            await _securityLogger.LogAsync(
                LogEventType.OtpFailed,
                user?.Id,
                request.Email,
                request.ClientIp,
                detail: "LoginOtp",
                ct: ct
            );
            throw;
        }

        return await _loginCompletionService.CompleteLoginAsync(user!, request.ClientIp, ct);
    }
}
