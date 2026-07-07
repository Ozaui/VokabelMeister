// ─────────────────────────────────────────────────────────────────────────────
// LoginWithAppleCommand.cs
//
// AMAÇ: POST /auth/apple — Apple identity token'ı ile giriş yapar; hesap yoksa
//       oluşturur, e-posta eşleşen yerel hesap varsa AppleId'yi ona bağlar.
// NEDEN: Apple e-postayı yalnızca İLK yetkilendirmede verir — sonraki girişlerde
//        payload.Email null olabilir; bu durumda yalnızca AppleId ile aranır,
//        DB'deki mevcut e-posta asla üzerine yazılmaz.
// BAĞIMLILIKLAR: IUserRepository, IAppleTokenValidator, ILoginCompletionService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Apple Sign-In'in istemcide ürettiği identity token'ı taşır (yalnızca iOS).
// NEDEN ClientIp init-property: bkz. VerifyLoginOtpCommand.
public record LoginWithAppleCommand(string IdentityToken) : IRequest<AuthTokenResponse>
{
    public string? ClientIp { get; init; }
}

public class LoginWithAppleCommandHandler : IRequestHandler<LoginWithAppleCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly ILoginCompletionService _loginCompletionService;

    public LoginWithAppleCommandHandler(
        IUserRepository userRepository,
        IAppleTokenValidator appleTokenValidator,
        ILoginCompletionService loginCompletionService
    )
    {
        _userRepository = userRepository;
        _appleTokenValidator = appleTokenValidator;
        _loginCompletionService = loginCompletionService;
    }

    public async Task<AuthTokenResponse> Handle(LoginWithAppleCommand request, CancellationToken ct)
    {
        var payload =
            await _appleTokenValidator.ValidateAsync(request.IdentityToken, ct)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByAppleIdAsync(payload.AppleId, ct);
        if (user is null)
        {
            if (payload.Email is not null)
                user = await _userRepository.GetByEmailAsync(payload.Email, ct);

            if (user is not null)
            {
                user.AppleId = payload.AppleId;
            }
            else
            {
                // NEDEN: İlk yetkilendirmede email gelmemesi teorik olarak beklenmez;
                //        savunmacı olarak ele alınır — email yoksa yeni kayıt açılamaz.
                if (payload.Email is null)
                    throw new InvalidSocialTokenException();

                user = new User
                {
                    Email = payload.Email,
                    FirstName = "Kullanıcı",
                    LastName = string.Empty,
                    AuthProvider = "Apple",
                    AppleId = payload.AppleId,
                    IsEmailVerified = true,
                };
                await _userRepository.AddAsync(user, ct: ct);
            }
        }

        if (!user.IsActive)
            throw new AccountNotActiveException();

        return await _loginCompletionService.CompleteLoginAsync(user, request.ClientIp, ct);
    }
}
