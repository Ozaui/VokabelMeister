// ─────────────────────────────────────────────────────────────────────────────
// LoginWithGoogleCommand.cs
//
// AMAÇ: POST /auth/google — Google ID token'ı ile giriş yapar; hesap yoksa
//       oluşturur, e-posta eşleşen yerel hesap varsa GoogleId'yi ona bağlar
//       (account linking).
// NEDEN: Google zaten kimliği doğruladığı için 2FA OTP adımı GEREKMEZ.
// BAĞIMLILIKLAR: IUserRepository, IGoogleTokenValidator, ILoginCompletionService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

// AMAÇ: Google Sign-In SDK'sının istemcide ürettiği ID token'ı taşır.
// NEDEN ClientIp init-property: bkz. VerifyLoginOtpCommand.
public record LoginWithGoogleCommand(string IdToken) : IRequest<AuthTokenResponse>
{
    public string? ClientIp { get; init; }
}

public class LoginWithGoogleCommandHandler : IRequestHandler<LoginWithGoogleCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly ILoginCompletionService _loginCompletionService;

    public LoginWithGoogleCommandHandler(
        IUserRepository userRepository,
        IGoogleTokenValidator googleTokenValidator,
        ILoginCompletionService loginCompletionService
    )
    {
        _userRepository = userRepository;
        _googleTokenValidator = googleTokenValidator;
        _loginCompletionService = loginCompletionService;
    }

    public async Task<AuthTokenResponse> Handle(LoginWithGoogleCommand request, CancellationToken ct)
    {
        var payload =
            await _googleTokenValidator.ValidateAsync(request.IdToken, ct)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByGoogleIdAsync(payload.GoogleId, ct);
        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email, ct);
            if (user is not null)
            {
                // NEDEN: Aynı e-postayla önceden yerel/başka sağlayıcı hesabı varsa,
                //        yeni bir hesap açmak yerine GoogleId bu hesaba bağlanır —
                //        aksi hâlde kullanıcı aynı e-posta için iki ayrı hesaba sahip olurdu.
                user.GoogleId = payload.GoogleId;
            }
            else
            {
                user = new User
                {
                    Email = payload.Email,
                    FirstName = payload.FirstName ?? "Kullanıcı",
                    LastName = payload.LastName ?? string.Empty,
                    AuthProvider = "Google",
                    GoogleId = payload.GoogleId,
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
