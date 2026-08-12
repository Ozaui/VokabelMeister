using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

// FirstName/LastName opsiyonel: Apple bu bilgiyi yalnızca kullanıcının O CİHAZDA İLK kez izin
// verdiği anda gönderir, istemci onu yakalayıp bu isteğe eklemek zorunda — sonraki girişlerde boş gelir.
public record LoginWithAppleCommand(string IdentityToken, string? FirstName, string? LastName, string? DeviceInfo, string? IpAddress, string? Language) : IRequest<LoginResponse>;

public class LoginWithAppleCommandHandler : IRequestHandler<LoginWithAppleCommand, LoginResponse>
{
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly IEmailService _emailService;

    public LoginWithAppleCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAppleTokenValidator appleTokenValidator,
        ILoginCompletionService loginCompletionService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _appleTokenValidator = appleTokenValidator;
        _loginCompletionService = loginCompletionService;
        _emailService = emailService;
    }

    public async Task<LoginResponse> Handle(LoginWithAppleCommand request, CancellationToken cancellationToken)
    {
        var payload = await _appleTokenValidator.ValidateAsync(request.IdentityToken, cancellationToken)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByAppleIdAsync(payload.Subject, cancellationToken);
        if (user is null)
        {
            user = payload.Email is not null ? await _userRepository.GetByEmailAsync(payload.Email, cancellationToken) : null;
            if (user is null)
            {
                user = new User
                {
                    // Apple relay e-postası da olsa Email NOT NULL alan — token'da e-posta yoksa
                    // (nadiren, tekrar giriş) sub tabanlı bir yer tutucu üretilir.
                    Email = payload.Email ?? $"{payload.Subject}@privaterelay.appleid.com",
                    AppleId = payload.Subject,
                    AuthProvider = "Apple",
                    FirstName = request.FirstName ?? "Apple",
                    LastName = request.LastName ?? "User",
                    IsEmailVerified = true,
                    EmailVerifiedAt = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user, cancellationToken);
            }
            else
            {
                user.AppleId = payload.Subject;
            }
        }

        if (!user.IsActive)
            throw new AccountInactiveException();

        var completion = _loginCompletionService.Complete(user, request.DeviceInfo, request.IpAddress);

        await _refreshTokenRepository.AddAsync(completion.RefreshTokenEntity, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        if (completion.AccountWasRecovered)
            await _emailService.SendAccountRecoveredNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);

        var userDto = new AuthUserDto(user.Id, user.CurrentLevel, user.ThemePreference, user.LanguagePreference);
        return new LoginResponse(completion.AccessToken, completion.RefreshTokenValue, AccessTokenExpiresInSeconds, userDto, completion.AccountWasRecovered);
    }
}
