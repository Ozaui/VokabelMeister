// ─────────────────────────────────────────────────────────────────────────────
// AuthController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §3'teki 13 Auth endpoint'ini MediatR üzerinden
//       ilgili Command'a bağlayan ince controller katmanı.
// NEDEN: Controller — CODING_STANDARDS.md §5: "ince katman: JWT'den userId al,
//        servisi çağır, DTO döndür. İş mantığı yok." İş mantığı artık
//        Application/Features/Auth/ altındaki Command Handler'larda. Doğrulama
//        (FluentValidation) ValidationFilter tarafından action çalışmadan önce
//        otomatik yapılır (Command tipleri Request DTO'larının yerini aldığı için
//        ValidationFilter'ın tip-agnostik mekanizması değişmeden çalışır).
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.API.Common;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    // AMAÇ: İsteği atan cihazın IP adresi — RefreshToken.IpAddress ve User.LastLoginIP'ye yazılır.
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    // AMAÇ: İsteğin Accept-Language header'ından çıkarılan dil kodu — MessageResponse
    //       üreten Command'lara `with` ile geçirilir (bkz. A-03.2, SECURITY.md §1.4).
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut kullanıcının Id'sini okur.
    // NEDEN: [Authorize] öznitelikli endpoint'ler (logout, delete-account/*) kendi
    //        kullanıcı kimliğini body'den değil token'dan alır — başkasının hesabı
    //        üzerinde işlem yapılamaz.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: Yeni kullanıcı kaydı oluşturur, e-posta doğrulama OTP'si gönderir.
    [HttpPost("register")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterCommand command,
        CancellationToken ct
    )
    {
        var response = await _mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    // AMAÇ: Kayıt sonrası e-postaya gelen OTP kodunu doğrular.
    [HttpPost("verify-email")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> VerifyEmail(
        VerifyEmailCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    // AMAÇ: E-posta doğrulama kodunu tekrar gönderir.
    [HttpPost("resend-verification")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResendVerification(
        ResendVerificationCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    // AMAÇ: Login adım 1 — şifreyi doğrular, başarılıysa OTP gönderir (token DÖNMEZ).
    [HttpPost("login")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> Login(
        LoginCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    // AMAÇ: Login adım 2 — OTP'yi doğrular, başarılıysa access+refresh token üretir.
    [HttpPost("login/verify-otp")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> VerifyLoginOtp(
        VerifyLoginOtpCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { ClientIp = ClientIp }, ct));

    // AMAÇ: Google ID token'ı ile giriş yapar/kayıt olur (2FA gerekmez).
    [HttpPost("google")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithGoogle(
        LoginWithGoogleCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { ClientIp = ClientIp }, ct));

    // AMAÇ: Apple identity token'ı ile giriş yapar/kayıt olur.
    [HttpPost("apple")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithApple(
        LoginWithAppleCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { ClientIp = ClientIp }, ct));

    // AMAÇ: Refresh token'ı doğrular, rotate eder, yeni access+refresh token çifti üretir.
    [HttpPost("refresh")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        RefreshCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { ClientIp = ClientIp }, ct));

    // AMAÇ: Verilen refresh token'ı kalıcı olarak iptal eder (yalnızca sahibi).
    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { UserId = CurrentUserId }, ct);
        return NoContent();
    }

    // AMAÇ: Şifre sıfırlama OTP'si gönderir (kullanıcı yoksa bile aynı yanıt döner).
    [HttpPost("forgot-password")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(
        ForgotPasswordCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    // AMAÇ: OTP + yeni şifre ile şifreyi değiştirir, tüm cihazlardan çıkış yapar.
    [HttpPost("reset-password")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResetPassword(
        ResetPasswordCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    // AMAÇ: Hesap silme OTP'si gönderir (15dk geçerli).
    [HttpPost("delete-account/request")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<MessageResponse>> RequestAccountDeletion(CancellationToken ct) =>
        Ok(
            await _mediator.Send(
                new RequestAccountDeletionCommand(CurrentUserId) { Language = Language },
                ct
            )
        );

    // AMAÇ: OTP + şifre ile hesap silmeyi onaylar; soft delete + 30 gün grace zamanlar.
    [HttpPost("delete-account/confirm")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<MessageResponse>> ConfirmAccountDeletion(
        ConfirmAccountDeletionCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { UserId = CurrentUserId, Language = Language }, ct));
}
