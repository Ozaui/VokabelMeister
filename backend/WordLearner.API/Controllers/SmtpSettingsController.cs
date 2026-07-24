using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.API.Common;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.DTOs.Smtp;
using WordLearner.Application.Features.Smtp;

namespace WordLearner.API.Controllers;

// AdminController'a EKLENMEDİ — WordsController/CategoriesController/MediaController ile aynı
// desen, SMTP kendi domain'i (System) olduğu için ayrı controller.
[ApiController]
[Route("api/v1/admin/smtp-settings")]
[Authorize(Roles = "Admin")]
public class SmtpSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SmtpSettingsController(IMediator mediator) => _mediator = mediator;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email)!;
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);

    [HttpGet]
    [ProducesResponseType(typeof(SmtpSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SmtpSettingsDto>> GetSmtpSettings(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetSmtpSettingsQuery(), ct));

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSmtpSettings(UpdateSmtpSettingsCommand command, CancellationToken ct)
    {
        await _mediator.Send(
            command with
            {
                UserId = CurrentUserId,
                ActorRole = CurrentRole,
                IpAddress = ClientIp,
            },
            ct
        );
        return NoContent();
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<MessageResponse>> TestSmtpSettings(CancellationToken ct) =>
        Ok(
            await _mediator.Send(
                new TestSmtpSettingsCommand { ToEmail = CurrentUserEmail, Language = Language },
                ct
            )
        );
}
