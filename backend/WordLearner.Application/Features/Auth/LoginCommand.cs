using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record LoginCommand(string Email, string Password, string? Language) : IRequest<Unit>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Unit>
{
    // Kullanıcı bulunamadığında da bcrypt karşılaştırması ÇALIŞTIRILIR (sabit süre) — aksi halde
    // "kayıtlı e-posta" ile "kayıtsız e-posta" yanıt süresinden ayırt edilebilir (SECURITY.md §1).
    private const string DummyHash = "$2a$12$C6UzMDM.H6dfI/f/IKcEeO4vTvHKp2FTa9DKAdKrdBvfsjnPZDe1i";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IOtpService otpService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // PasswordHash null → sosyal-yalnızca hesap, şifre ile giriş edilemez (aynı hata, sızıntı yok).
        var hashToVerify = user?.PasswordHash ?? DummyHash;
        var passwordMatches = _passwordService.Verify(request.Password, hashToVerify);

        if (user is null || user.PasswordHash is null || !passwordMatches)
            throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new AccountInactiveException();

        var otpCode = _otpService.Generate(user, OtpPurpose.LoginOtp);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _emailService.SendLoginOtpAsync(user.Email, user.FirstName, otpCode, request.Language, cancellationToken);
        return Unit.Value;
    }
}
