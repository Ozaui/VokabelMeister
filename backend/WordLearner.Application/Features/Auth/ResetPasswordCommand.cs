// ─────────────────────────────────────────────────────────────────────────────
// ResetPasswordCommand.cs
//
// AMAÇ: POST /auth/reset-password — OTP + yeni şifre ile şifreyi değiştirir,
//       tüm cihazlardan çıkış yapar.
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService,
//                IOtpService, IEmailService.
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

// AMAÇ: Adım 2 — OTP + yeni şifre. Başarılıysa tüm cihazlardan çıkış yapılır.
// NEDEN Language/ClientIp init-property: bkz. LoginCommand — ClientIp A-04'te
//       OtpFailed SecurityLog kaydı için eklendi.
public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword)
    : IRequest<MessageResponse>
{
    public string? Language { get; init; }
    public string? ClientIp { get; init; }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ISecurityLogger _securityLogger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        IEmailService emailService,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
        _securityLogger = securityLogger;
    }

    public async Task<MessageResponse> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        try
        {
            _otpService.Validate(user, request.OtpCode, OtpPurpose.PasswordReset);
        }
        catch (InvalidOtpException)
        {
            await _securityLogger.LogAsync(
                LogEventType.OtpFailed,
                user?.Id,
                request.Email,
                request.ClientIp,
                detail: "PasswordReset",
                ct: ct
            );
            throw;
        }

        user!.PasswordHash = _passwordService.Hash(request.NewPassword);
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, user.Id, ct);

        // NEDEN: Şifre değiştiğinde tüm cihazlardan çıkış yapılır (REFERENCE/SECURITY.md §7).
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, ct);

        // NEDEN: LogEventType.PasswordReset bir BAŞARI olayıdır (OtpFailed'in aksine) —
        //        "şifre sıfırlama akışı tamamlandı" audit izi, ör. hesabın ele geçirilip
        //        geçirilmediğini araştıran bir admin için.
        await _securityLogger.LogAsync(
            LogEventType.PasswordReset,
            user.Id,
            ipAddress: request.ClientIp,
            ct: ct
        );

        return new MessageResponse("PASSWORD_UPDATED", SuccessMessages.Resolve("PASSWORD_UPDATED", request.Language));
    }
}
