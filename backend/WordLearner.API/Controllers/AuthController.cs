using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.API.Common;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("register")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        RegisterCommand command,
        CancellationToken ct
    )
    {
        var response = await _mediator.Send(command with { Language = Language }, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> VerifyEmail(
        VerifyEmailCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("resend-verification")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ResendVerification(
        ResendVerificationCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    [HttpPost("login")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MessageResponse>> Login(
        LoginCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("login/verify-otp")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponse>> VerifyLoginOtp(
        VerifyLoginOtpCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("google")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithGoogle(
        LoginWithGoogleCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("apple")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithApple(
        LoginWithAppleCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("refresh")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        RefreshCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { ClientIp = ClientIp }, ct));

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { UserId = CurrentUserId }, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(
        ForgotPasswordCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language }, ct));

    [HttpPost("reset-password")]
    [EnableRateLimiting("anonymous")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ResetPassword(
        ResetPasswordCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { Language = Language, ClientIp = ClientIp }, ct));

    [HttpPost("delete-account/request")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> RequestAccountDeletion(CancellationToken ct) =>
        Ok(
            await _mediator.Send(
                new RequestAccountDeletionCommand(CurrentUserId) { Language = Language },
                ct
            )
        );

    [HttpPost("delete-account/confirm")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> ConfirmAccountDeletion(
        ConfirmAccountDeletionCommand command,
        CancellationToken ct
    ) =>
        Ok(
            await _mediator.Send(
                command with
                {
                    UserId = CurrentUserId,
                    Language = Language,
                    ClientIp = ClientIp,
                },
                ct
            )
        );
}
