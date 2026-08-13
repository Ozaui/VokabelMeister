using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.Application.Common;
using WordLearner.Application.DTOs;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.QrLogin;

namespace WordLearner.API.Controllers;

[Route("auth/qr")]
public class QrLoginController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public QrLoginController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    [AllowAnonymous]
    [EnableRateLimiting("qrGenerate")]
    public async Task<ActionResult<QrLoginGenerateResponse>> Generate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerateQrLoginCommand(ClientIpAddress, DeviceInfo), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{token}/status")]
    [AllowAnonymous]
    [EnableRateLimiting("qrStatus")]
    public async Task<ActionResult<QrLoginStatusResponse>> GetStatus(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQrLoginStatusQuery(token, AcceptLanguage), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{token}/scan")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<QrLoginScanResponse>> Scan(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ScanQrLoginCommand(token, CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{token}/confirm")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<MessageResponse>> Confirm(string token, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmQrLoginCommand(token, CurrentUserId), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("QR_LOGIN_CONFIRMED", AcceptLanguage)));
    }

    [HttpPost("{token}/deny")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<MessageResponse>> Deny(string token, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DenyQrLoginCommand(token, CurrentUserId), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("QR_LOGIN_DENIED", AcceptLanguage)));
    }
}
