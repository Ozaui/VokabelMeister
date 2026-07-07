// ─────────────────────────────────────────────────────────────────────────────
// ResetPasswordCommand.cs
//
// AMAÇ: POST /auth/reset-password — OTP + yeni şifre ile şifreyi değiştirir,
//       tüm cihazlardan çıkış yapar.
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService,
//                IOtpService, IEmailService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Adım 2 — OTP + yeni şifre. Başarılıysa tüm cihazlardan çıkış yapılır.
public record ResetPasswordCommand(string Email, string OtpCode, string NewPassword)
    : IRequest<MessageResponse>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, MessageResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        IEmailService emailService
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<MessageResponse> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        _otpService.Validate(user, request.OtpCode, OtpPurpose.PasswordReset);

        user!.PasswordHash = _passwordService.Hash(request.NewPassword);
        _otpService.Clear(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        // NEDEN: Şifre değiştiğinde tüm cihazlardan çıkış yapılır (REFERENCE/SECURITY.md §7).
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, ct);

        return new MessageResponse("Şifreniz güncellendi.");
    }
}
