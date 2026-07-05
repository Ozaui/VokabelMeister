// ─────────────────────────────────────────────────────────────────────────────
// AuthController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §3'teki 13 Auth endpoint'ini IAuthService'e
//       bağlayan ince controller katmanı.
// NEDEN: Controller — CODING_STANDARDS.md §5: "ince katman: JWT'den userId al,
//        servisi çağır, DTO döndür. İş mantığı yok." Doğrulama (FluentValidation)
//        ValidationFilter tarafından action çalışmadan önce otomatik yapılır.
// BAĞIMLILIKLAR: IAuthService, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // AMAÇ: İsteği atan cihazın IP adresi — RefreshToken.IpAddress ve User.LastLoginIP'ye yazılır.
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut kullanıcının Id'sini okur.
    // NEDEN: [Authorize] öznitelikli endpoint'ler (logout, delete-account/*) kendi
    //        kullanıcı kimliğini body'den değil token'dan alır — başkasının hesabı
    //        üzerinde işlem yapılamaz.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: Yeni kullanıcı kaydı oluşturur, e-posta doğrulama OTP'si gönderir.
    [HttpPost("register")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterRequest request,
        CancellationToken ct
    )
    {
        var response = await _authService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    // AMAÇ: Kayıt sonrası e-postaya gelen OTP kodunu doğrular.
    [HttpPost("verify-email")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> VerifyEmail(
        VerifyEmailRequest request,
        CancellationToken ct
    ) => Ok(await _authService.VerifyEmailAsync(request, ct));

    // AMAÇ: E-posta doğrulama kodunu tekrar gönderir.
    [HttpPost("resend-verification")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResendVerification(
        ResendVerificationRequest request,
        CancellationToken ct
    ) => Ok(await _authService.ResendVerificationAsync(request, ct));

    // AMAÇ: Login adım 1 — şifreyi doğrular, başarılıysa OTP gönderir (token DÖNMEZ).
    [HttpPost("login")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> Login(
        LoginRequest request,
        CancellationToken ct
    ) => Ok(await _authService.LoginAsync(request, ct));

    // AMAÇ: Login adım 2 — OTP'yi doğrular, başarılıysa access+refresh token üretir.
    [HttpPost("login/verify-otp")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> VerifyLoginOtp(
        VerifyOtpRequest request,
        CancellationToken ct
    ) => Ok(await _authService.VerifyLoginOtpAsync(request, ClientIp, ct));

    // AMAÇ: Google ID token'ı ile giriş yapar/kayıt olur (2FA gerekmez).
    [HttpPost("google")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithGoogle(
        GoogleLoginRequest request,
        CancellationToken ct
    ) => Ok(await _authService.LoginWithGoogleAsync(request, ClientIp, ct));

    // AMAÇ: Apple identity token'ı ile giriş yapar/kayıt olur.
    [HttpPost("apple")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithApple(
        AppleLoginRequest request,
        CancellationToken ct
    ) => Ok(await _authService.LoginWithAppleAsync(request, ClientIp, ct));

    // AMAÇ: Refresh token'ı doğrular, rotate eder, yeni access+refresh token çifti üretir.
    [HttpPost("refresh")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        RefreshRequest request,
        CancellationToken ct
    ) => Ok(await _authService.RefreshAsync(request, ClientIp, ct));

    // AMAÇ: Verilen refresh token'ı kalıcı olarak iptal eder (yalnızca sahibi).
    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken ct)
    {
        await _authService.LogoutAsync(CurrentUserId, request, ct);
        return NoContent();
    }

    // AMAÇ: Şifre sıfırlama OTP'si gönderir (kullanıcı yoksa bile aynı yanıt döner).
    [HttpPost("forgot-password")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken ct
    ) => Ok(await _authService.ForgotPasswordAsync(request, ct));

    // AMAÇ: OTP + yeni şifre ile şifreyi değiştirir, tüm cihazlardan çıkış yapar.
    [HttpPost("reset-password")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken ct
    ) => Ok(await _authService.ResetPasswordAsync(request, ct));

    // AMAÇ: Hesap silme OTP'si gönderir (15dk geçerli).
    [HttpPost("delete-account/request")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<MessageResponse>> RequestAccountDeletion(CancellationToken ct) =>
        Ok(await _authService.RequestAccountDeletionAsync(CurrentUserId, ct));

    // AMAÇ: OTP + şifre ile hesap silmeyi onaylar; soft delete + 30 gün grace zamanlar.
    [HttpPost("delete-account/confirm")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<MessageResponse>> ConfirmAccountDeletion(
        DeleteAccountConfirmRequest request,
        CancellationToken ct
    ) => Ok(await _authService.ConfirmAccountDeletionAsync(CurrentUserId, request, ct));
}
