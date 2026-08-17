using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Application.Features.QrLogin;

public record GetQrLoginStatusQuery(string QrToken, string? Language) : IRequest<QrLoginStatusResponse>;

public class GetQrLoginStatusQueryHandler : IRequestHandler<GetQrLoginStatusQuery, QrLoginStatusResponse>
{
    // JwtTokenService.GenerateAccessToken 15dk'yı sabit üretiyor — VerifyLoginOtpCommandHandler ile aynı.
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILoginCompletionService _loginCompletionService;
    private readonly IEmailService _emailService;

    public GetQrLoginStatusQueryHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ILoginCompletionService loginCompletionService,
        IEmailService emailService)
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _loginCompletionService = loginCompletionService;
        _emailService = emailService;
    }

    public async Task<QrLoginStatusResponse> Handle(GetQrLoginStatusQuery request, CancellationToken cancellationToken)
    {
        var session = await _qrLoginSessionRepository.GetByTokenHashAsync(_passwordService.HashToken(request.QrToken), cancellationToken)
            ?? throw new QrSessionGoneException();

        // Consumed = token zaten teslim edildi, TEK SEFERLİK kuralı (SECURITY.md §1.3 ADIM 4).
        if (session.Status == QrLoginStatus.Consumed)
            throw new QrSessionGoneException();

        if (session.Status is QrLoginStatus.Pending or QrLoginStatus.Scanned && session.ExpiresAt < DateTime.UtcNow)
            session.Status = QrLoginStatus.Expired;

        if (session.Status != QrLoginStatus.Confirmed)
        {
            await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken); // Expired mutasyonu varsa persist edilir
            return new QrLoginStatusResponse(session.Status.ToString());
        }

        var user = await _userRepository.GetByIdAsync(session.UserId!.Value, cancellationToken)
            ?? throw new QrSessionGoneException();

        var completion = _loginCompletionService.Complete(user, session.RequesterDeviceInfo, session.RequesterIp);
        await _refreshTokenRepository.AddAsync(completion.RefreshTokenEntity, cancellationToken);

        var statusForResponse = session.Status.ToString();
        session.Status = QrLoginStatus.Consumed;
        // Session/User/yeni RefreshToken AYNI scoped DbContext'i paylaşıyor — tek SaveChanges üçünü de yazar.
        await _qrLoginSessionRepository.SaveChangesAsync(cancellationToken);

        if (completion.AccountWasRecovered)
            await _emailService.SendAccountRecoveredNotificationAsync(user.Email, user.FirstName, request.Language, cancellationToken);

        var userDto = new AuthUserDto(user.Id, user.CurrentLevel, user.ThemePreference, user.LanguagePreference);
        return new QrLoginStatusResponse(statusForResponse, completion.AccessToken, completion.RefreshTokenValue, AccessTokenExpiresInSeconds, userDto, completion.AccountWasRecovered);
    }
}
