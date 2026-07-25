using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Application.Features.Auth;

// Language/ClientIp Accept-Language header'ından ve bağlantıdan gelir, gövdede yer almaz
// (Controller `with` ile set eder).
public record LoginCommand(string Email, string Password) : IRequest<MessageResponse>
{
    public string? Language { get; init; }
    public string? ClientIp { get; init; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, MessageResponse>
{
    // Kullanıcı bulunamadığında/şifresi olmadığında (sosyal hesap) bile sabit süreli bir BCrypt
    // karşılaştırması yapılabilsin diye önceden hesaplanmış, hiçbir gerçek şifreyle eşleşmeyecek
    // hash (timing attack önlemi). ConfirmAccountDeletionCommandHandler'da bilinçli bir kopyası daha var.
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

        await _emailService.SendLoginOtpAsync(user.Email, otpCode, request.Language, ct);
        return new MessageResponse("OTP_SENT", SuccessMessages.Resolve("OTP_SENT", request.Language));
    }
}
