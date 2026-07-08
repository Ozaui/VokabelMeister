// ─────────────────────────────────────────────────────────────────────────────
// QrLoginController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §3.1'deki 4 QR ile giriş endpoint'ini
//       MediatR üzerinden ilgili Command'a bağlayan ince controller katmanı.
// NEDEN: AuthController'dan AYRI bir controller — QR akışı Admin panelde yok
//        (yalnızca Web/Mobil), rotası da `/auth/qr/*` altında toplu (CLAUDE.md §5
//        — Command+Handler aynı dosyada, controller ince katman kuralı).
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.QrLogin;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/auth/qr")]
public class QrLoginController : ControllerBase
{
    private readonly IMediator _mediator;

    public QrLoginController(IMediator mediator) => _mediator = mediator;

    // AMAÇ: İsteği atan tarafın IP adresi (bkz. AuthController.ClientIp).
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut kullanıcının Id'sini okur
    //       (bkz. AuthController.CurrentUserId) — scan/confirm/deny kendi kimliğini
    //       token'dan alır, body'den değil.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: Web'in göstereceği yeni bir QR oturumu başlatır.
    [HttpPost("generate")]
    [EnableRateLimiting("qrGenerate")]
    public async Task<ActionResult<QrGenerateResponse>> Generate(CancellationToken ct)
    {
        var command = new GenerateQrLoginCommand
        {
            ClientIp = ClientIp,
            DeviceInfo = Request.Headers.UserAgent.ToString(),
        };
        return Ok(await _mediator.Send(command, ct));
    }

    // AMAÇ: Web'in ~2sn'de bir sorguladığı polling endpoint'i — Confirmed'de tek
    //       seferlik token döner, sonra oturum Consumed'e geçer.
    [HttpGet("{token}/status")]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<QrStatusResponse>> GetStatus(string token, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetQrLoginStatusCommand(token) { ClientIp = ClientIp }, ct));

    // AMAÇ: Mobil, zaten giriş yapmış olduğu JWT'siyle QR'ı taradığında çağırır.
    [HttpPost("{token}/scan")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<ActionResult<QrScanResponse>> Scan(string token, CancellationToken ct) =>
        Ok(await _mediator.Send(new ScanQrLoginCommand(token) { UserId = CurrentUserId }, ct));

    // AMAÇ: Mobil kullanıcı, gördüğü cihaz/pairingCode'u doğrulayıp girişi onaylar.
    [HttpPost("{token}/confirm")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Confirm(string token, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmQrLoginCommand(token) { UserId = CurrentUserId }, ct);
        return NoContent();
    }

    // AMAÇ: Mobil kullanıcı girişi reddeder (cihaz/kod eşleşmiyor veya isteği tanımıyor).
    [HttpPost("{token}/deny")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    public async Task<IActionResult> Deny(string token, CancellationToken ct)
    {
        await _mediator.Send(new DenyQrLoginCommand(token) { UserId = CurrentUserId }, ct);
        return NoContent();
    }
}
