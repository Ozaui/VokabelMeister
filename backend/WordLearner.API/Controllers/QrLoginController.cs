using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.API.Common;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.QrLogin;

namespace WordLearner.API.Controllers;

// AuthController'dan AYRI — QR akışı Admin panelde yok (yalnızca Web/Mobil).
[ApiController]
[Route("api/v1/auth/qr")]
public class QrLoginController : ControllerBase
{
    private readonly IMediator _mediator;

    public QrLoginController(IMediator mediator) => _mediator = mediator;

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("generate")]
    [EnableRateLimiting("qrGenerate")]
    [ProducesResponseType(typeof(QrGenerateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QrGenerateResponse>> Generate(CancellationToken ct)
    {
        var command = new GenerateQrLoginCommand
        {
            ClientIp = ClientIp,
            DeviceInfo = Request.Headers.UserAgent.ToString(),
        };
        return Ok(await _mediator.Send(command, ct));
    }

    // "qrStatus" — paylaşımlı "anonymous" limitini kullansaydı bu polling sıklığı (~30 istek/dk)
    // tüm anonim trafiği kilitlerdi (bkz. Program.cs "qrStatus" policy).
    [HttpGet("{token}/status")]
    [EnableRateLimiting("qrStatus")]
    [ProducesResponseType(typeof(QrStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status410Gone)]
    public async Task<ActionResult<QrStatusResponse>> GetStatus(string token, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetQrLoginStatusCommand(token) { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("{token}/scan")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(typeof(QrScanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status410Gone)]
    public async Task<ActionResult<QrScanResponse>> Scan(string token, CancellationToken ct) =>
        Ok(await _mediator.Send(new ScanQrLoginCommand(token) { UserId = CurrentUserId }, ct));

    [HttpPost("{token}/confirm")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status410Gone)]
    public async Task<IActionResult> Confirm(string token, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmQrLoginCommand(token) { UserId = CurrentUserId }, ct);
        return NoContent();
    }

    [HttpPost("{token}/deny")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status410Gone)]
    public async Task<IActionResult> Deny(string token, CancellationToken ct)
    {
        await _mediator.Send(new DenyQrLoginCommand(token) { UserId = CurrentUserId }, ct);
        return NoContent();
    }
}
