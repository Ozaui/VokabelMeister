// ─────────────────────────────────────────────────────────────────────────────
// GetQrLoginStatusCommand.cs
//
// AMAÇ: GET /auth/qr/{token}/status — web'in ~2sn'de bir sorguladığı polling
//       endpoint'i. Confirmed durumu İLK okunduğunda normal login'deki AYNI
//       ITokenService/ILoginCompletionService akışıyla token üretir, sonra
//       oturumu Consumed'e geçirir (token'lar yalnızca BİR kez döner).
// NEDEN: REFERENCE/SECURITY.md §1.3 ADIM 4 — QR girişi ayrı bir kimlik doğrulama
//        sistemi değildir, token üretimi A-03'te yazılan ortak servisten geçer.
//        Consumed sonrası tekrar okuma 410 döner (token'ların ikinci kez
//        sızdırılmasını/tekrar okunmasını önler). Expired durumu ise 410 DEĞİL,
//        200 + {status:"Expired"} döner — web bunu "yeni QR üret" sinyali olarak
//        kullanır, henüz hiçbir token üretilmediği için "gone" (kaybolan bir
//        kaynak) anlamına gelmez.
// NASIL: 1) Hash'e göre oturumu bul, yoksa 404  2) Süresi geçmişse Expired'a
//        çevir + 200 döndür  3) Consumed ise 410  4) Confirmed ise kullanıcıyı
//        (soft-delete filtresi YOK SAYILARAK — grace-period kurtarma normal
//        login akışıyla aynı şekilde çalışabilsin diye) yükle, CompleteLoginAsync
//        çağır, Consumed'e geçir, token'lı yanıt dön
//        5) Diğer durumlarda (Pending/Scanned/Denied) yalnızca Status dön.
// BAĞIMLILIKLAR: IQrLoginSessionRepository, IPasswordService, IUserRepository,
//                ILoginCompletionService.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Application.Features.QrLogin;

// NEDEN ClientIp init-property: bkz. RefreshCommand — web'in bu isteği attığı
//       IP, telefonun IP'si değil, token üretiminde User.LastLoginIP'ye yazılır.
public record GetQrLoginStatusCommand(string QrToken) : IRequest<QrStatusResponse>
{
    public string? ClientIp { get; init; }
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
            await _qrLoginSessionRepository.UpdateAsync(session, ct: ct);
            return new QrStatusResponse(session.Status.ToString(), null, null, null, null);
        }

        if (session.Status == QrLoginStatus.Consumed)
            throw new QrSessionGoneException();

        if (session.Status != QrLoginStatus.Confirmed)
            return new QrStatusResponse(session.Status.ToString(), null, null, null, null);

        // NEDEN GetByIdIncludingDeletedAsync (GetByIdAsync DEĞİL): normal login
        //       (LoginCommand/LoginWithGoogle/Apple) kullanıcıyı GetByEmailAsync ile
        //       soft-delete filtresi YOK SAYILARAK bulur, böylece CompleteLoginAsync'in
        //       grace-period kurtarma mantığı çalışabilir. GetByIdAsync (filtreli)
        //       kullanılsaydı, hesabını yeni silmiş bir kullanıcı burada anlamsız bir
        //       404 alırdı — diğer giriş yollarıyla tutarsız bir davranış.
        var user =
            await _userRepository.GetByIdIncludingDeletedAsync(session.UserId!.Value, ct)
            ?? throw new EntityNotFoundException(typeof(User), session.UserId.Value);

        // NEDEN: LoginCommand/LoginWithGoogleCommand/LoginWithAppleCommand'ın hepsi
        //        CompleteLoginAsync'e girmeden önce IsActive kontrolü yapıyor (dondurulmuş
        //        hesap giriş yapamaz) — QR akışının token üretimi de aynı ortak son
        //        adımdan geçtiği için aynı kontrole tabi olmalı.
        if (!user.IsActive)
            throw new AccountNotActiveException();

        var authResponse = await _loginCompletionService.CompleteLoginAsync(user, request.ClientIp, ct);

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
