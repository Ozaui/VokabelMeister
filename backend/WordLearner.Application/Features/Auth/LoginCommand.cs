// ─────────────────────────────────────────────────────────────────────────────
// LoginCommand.cs
//
// AMAÇ: POST /auth/login — Login adım 1: şifreyi doğrular, başarılıysa OTP
//       gönderir (token DÖNMEZ).
// NEDEN: REFERENCE/SECURITY.md §1 — 2 adımlı OTP girişinin ilk adımı. Timing
//        attack önlemi: kullanıcı yoksa veya şifresi yoksa (sosyal hesap) bile
//        SABİT SÜRELİ bir BCrypt karşılaştırması yapılır, bu yüzden hangi
//        durumun gerçekleştiği fark etmeksizin aynı sürede aynı hata döner.
// BAĞIMLILIKLAR: IUserRepository, IPasswordService, IOtpService, IEmailService.
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

// NEDEN Language/ClientIp init-property: bkz. LogoutCommand'daki UserId — Accept-Language
//       header'ından ve bağlantıdan gelir, gövdede yer almaz (Controller `with` ile set eder).
//       ClientIp A-04'te eklendi — LoginFailed SecurityLog kaydı için gerekli.
public record LoginCommand(string Email, string Password) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
    public string? ClientIp { get; init; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, MessageResponse>
{
    // AMAÇ: Kullanıcı bulunamadığında/şifresi olmadığında (sosyal hesap) bile SABİT
    //       SÜRELİ bir BCrypt karşılaştırması yapılabilmesi için önceden hesaplanmış,
    //       geçerli formatlı ama hiçbir gerçek şifreyle eşleşmeyecek bir hash.
    // NEDEN static + doğrudan BCrypt.Net: Bu alan tip yüklenirken (instance yokken) bir
    //        kez hesaplanır — IPasswordService instance metodu bu bağlamda kullanılamaz;
    //        PasswordService.Hash'in yaptığı işlemin aynısı (workFactor:12) burada tekrarlanır.
    //        NOT: ConfirmAccountDeletionCommandHandler'da da bağımsız bir kopyası var —
    //        bilinçli, küçük bir tekrar (2 kullanım, tek satır); paylaşılan bir servise
    //        çıkarmak aşırı soyutlama olurdu.
    private static readonly string FakePasswordHashForTiming = BCrypt.Net.BCrypt.HashPassword(
        Guid.NewGuid().ToString(),
        workFactor: 12
    );

    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly ISecurityLogger _securityLogger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IOtpService otpService,
        IEmailService emailService,
        ISecurityLogger securityLogger
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
        _securityLogger = securityLogger;
    }

    public async Task<MessageResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        var hashToVerify = user?.PasswordHash ?? FakePasswordHashForTiming;
        var passwordValid = _passwordService.Verify(request.Password, hashToVerify);

        if (user is null || user.PasswordHash is null || !passwordValid)
        {
            await _securityLogger.LogAsync(
                LogEventType.LoginFailed,
                user?.Id,
                request.Email,
                request.ClientIp,
                ct: ct
            );
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
            throw new AccountNotActiveException();

        var (otpCode, otpHash) = _otpService.Generate();
        user.PendingOtpCodeHash = otpHash;
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(IOtpService.OtpExpiryMinutes);
        user.PendingOtpCodePurpose = OtpPurpose.LoginOtp;
        await _userRepository.UpdateAsync(user, user.Id, ct);

        await _emailService.SendLoginOtpAsync(user.Email, otpCode, ct);
        return new MessageResponse("OTP_SENT", SuccessMessages.Resolve("OTP_SENT", request.Language));
    }
}
