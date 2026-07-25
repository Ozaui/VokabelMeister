using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Auth;

public record LoginWithGoogleCommand(string IdToken) : IRequest<AuthTokenResponse>
{
    public string? ClientIp { get; init; }

    // Yalnızca grace period'daki bir hesap kurtarıldığında gidecek bilgilendirme e-postasının dili için.
    public string? Language { get; init; }
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
                // Aynı e-postayla önceden yerel hesap varsa yeni hesap açmak yerine GoogleId ona bağlanır.
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

        return await _loginCompletionService.CompleteLoginAsync(user, request.ClientIp, request.Language, ct);
    }
}
