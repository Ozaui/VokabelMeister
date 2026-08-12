using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

public record LoginWithGoogleCommand(string IdToken, string? DeviceInfo, string? IpAddress, string? Language) : IRequest<LoginResponse>;

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, LoginResponse>
{
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly IEmailService _emailService;

    public LoginWithGoogleCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IGoogleTokenValidator googleTokenValidator,
        ILoginCompletionService loginCompletionService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _googleTokenValidator = googleTokenValidator;
        _loginCompletionService = loginCompletionService;
        _emailService = emailService;
    }

    public async Task<LoginResponse> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        var payload = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByGoogleIdAsync(payload.Subject, cancellationToken);
        if (user is null)
        {
            // Aynı e-postayla daha önce Local/Apple kayıt açılmışsa hesabı BİRLEŞTİR — iki ayrı
            // hesap yerine kullanıcı artık ikinci bir yöntemle de aynı hesaba girebilir.
            user = await _userRepository.GetByEmailAsync(payload.Email, cancellationToken);
            if (user is null)
            {
                user = new User
                {
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    AuthProvider = "Google",
                    FirstName = payload.FirstName ?? "Google",
                    LastName = payload.LastName ?? "User",
                    IsEmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                user.GoogleId = payload.Subject;
            }
        }

        if (!user.IsActive)
            throw new AccountInactiveException();

        var completion = _loginCompletionService.Complete(user, request.DeviceInfo, request.IpAddress);

        await _refreshTokenRepository.AddAsync(completion.RefreshTokenEntity, cancellationToken);
        // İki repo AYNI DbContext'i paylaşıyor (scoped) — tek SaveChanges hem User hem RefreshToken'ı yazar.
        await _userRepository.SaveChangesAsync(cancellationToken);

        if (completion.AccountWasRecovered)
            await _emailService.SendAccountRecoveredNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);

        var userDto = new AuthUserDto(user.Id, user.CurrentLevel, user.ThemePreference, user.LanguagePreference);
        return new LoginResponse(completion.AccessToken, completion.RefreshTokenValue, AccessTokenExpiresInSeconds, userDto, completion.AccountWasRecovered);
    }
}
