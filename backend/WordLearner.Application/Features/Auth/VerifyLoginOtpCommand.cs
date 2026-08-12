using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.Auth;

public record VerifyLoginOtpCommand(string Email, string OtpCode, string? DeviceInfo, string? IpAddress, string? Language) : IRequest<LoginResponse>;

public class VerifyLoginOtpCommandHandler : IRequestHandler<VerifyLoginOtpCommand, LoginResponse>
{
    // JwtTokenService.GenerateAccessToken 15dk'yı sabit üretiyor — yanıt bunu yansıtır.
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOtpService _otpService;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly IEmailService _emailService;

    public VerifyLoginOtpCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOtpService otpService,
        ILoginCompletionService loginCompletionService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _otpService = otpService;
        _loginCompletionService = loginCompletionService;
        _emailService = emailService;
    }

    public async Task<LoginResponse> Handle(VerifyLoginOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidOtpException();

        if (!user.IsActive)
            throw new AccountInactiveException();

        var result = _otpService.Verify(user, request.OtpCode, OtpPurpose.LoginOtp);
        if (result == OtpVerificationResult.Expired)
            throw new OtpExpiredException();
        if (result == OtpVerificationResult.InvalidCode)
            throw new InvalidOtpException();

        var completion = _loginCompletionService.Complete(user, request.DeviceInfo, request.IpAddress);

        await _refreshTokenRepository.AddAsync(completion.RefreshTokenEntity, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        if (completion.AccountWasRecovered)
            await _emailService.SendAccountRecoveredNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);

        var userDto = new AuthUserDto(user.Id, user.CurrentLevel, user.ThemePreference, user.LanguagePreference);
        return new LoginResponse(completion.AccessToken, completion.RefreshTokenValue, AccessTokenExpiresInSeconds, userDto, completion.AccountWasRecovered);
    }
}
