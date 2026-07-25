using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

// ClientIp web'in bu isteği attığı IP'dir (telefonun IP'si değil) — token üretiminde User.LastLoginIP'ye yazılır.
public record GetQrLoginStatusCommand(string QrToken) : IRequest<QrStatusResponse>
{
    public string? ClientIp { get; init; }

    // Yalnızca grace period'daki bir hesap kurtarıldığında gidecek bilgilendirme e-postasının dili için.
    public string? Language { get; init; }
}

public class GetQrLoginStatusCommandHandler : IRequestHandler<GetQrLoginStatusCommand, QrStatusResponse>
{
    private readonly IQrLoginSessionRepository _qrLoginSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;
    private readonly ILoginCompletionService _loginCompletionService;

    public GetQrLoginStatusCommandHandler(
        IQrLoginSessionRepository qrLoginSessionRepository,
        IPasswordService passwordService,
        IUserRepository userRepository,
        ILoginCompletionService loginCompletionService
    )
    {
        _qrLoginSessionRepository = qrLoginSessionRepository;
        _passwordService = passwordService;
        _userRepository = userRepository;
        _loginCompletionService = loginCompletionService;
    }

    public async Task<QrStatusResponse> Handle(GetQrLoginStatusCommand request, CancellationToken ct)
    {
        var tokenHash = _passwordService.HashToken(request.QrToken);
        var session =
            await _qrLoginSessionRepository.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new EntityNotFoundException(typeof(QrLoginSession), tokenHash);

        if (session.IsExpired(DateTime.UtcNow))
        {
            // 410 DEĞİL, 200 + {status:"Expired"} — web bunu "yeni QR üret" sinyali olarak
            // kullanır, henüz hiçbir token üretilmediği için "gone" anlamına gelmez.
            await _qrLoginSessionRepository.UpdateAsync(session, ct: ct);
            return new QrStatusResponse(session.Status.ToString(), null, null, null, null);
        }

        if (session.Status == QrLoginStatus.Consumed)
            throw new QrSessionGoneException();

        if (session.Status != QrLoginStatus.Confirmed)
            return new QrStatusResponse(session.Status.ToString(), null, null, null, null);

        // GetByIdIncludingDeletedAsync (soft-delete filtresi yok sayılır) — CompleteLoginAsync'in
        // grace-period kurtarma mantığı diğer giriş yollarıyla (LoginCommand vb.) tutarlı çalışsın diye.
        var user =
            await _userRepository.GetByIdIncludingDeletedAsync(session.UserId!.Value, ct)
            ?? throw new EntityNotFoundException(typeof(User), session.UserId.Value);

        if (!user.IsActive)
            throw new AccountNotActiveException();

        var authResponse = await _loginCompletionService.CompleteLoginAsync(user, request.ClientIp, request.Language, ct);

        session.Status = QrLoginStatus.Consumed;
        await _qrLoginSessionRepository.UpdateAsync(session, user.Id, ct);

        return new QrStatusResponse(
            QrLoginStatus.Confirmed.ToString(),
            authResponse.AccessToken,
            authResponse.RefreshToken,
            authResponse.ExpiresIn,
            authResponse.User
        );
    }
}
