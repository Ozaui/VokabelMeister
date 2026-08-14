using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordLearner.Application.Common;
using WordLearner.Application.DTOs;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;

namespace WordLearner.API.Controllers;

[Route("auth")]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName, AcceptLanguage), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VerifyEmailCommand(request.Email, request.OtpCode, DeviceInfo, ClientIpAddress), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("EMAIL_VERIFIED", AcceptLanguage)));
    }

    [HttpPost("resend-verification")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResendVerification(ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResendVerificationCommand(request.Email, AcceptLanguage), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("VERIFICATION_RESENT", AcceptLanguage)));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<MessageResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LoginCommand(request.Email, request.Password, DeviceInfo, ClientIpAddress, AcceptLanguage), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("LOGIN_OTP_SENT", AcceptLanguage)));
    }

    [HttpPost("login/verify-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<LoginResponse>> VerifyLoginOtp(VerifyLoginOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new VerifyLoginOtpCommand(request.Email, request.OtpCode, DeviceInfo, ClientIpAddress, AcceptLanguage), cancellationToken);
        return Ok(result);
    }

    [HttpPost("google")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<LoginResponse>> LoginWithGoogle(LoginWithGoogleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginWithGoogleCommand(request.IdToken, DeviceInfo, ClientIpAddress, AcceptLanguage), cancellationToken);
        return Ok(result);
    }

    [HttpPost("apple")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<LoginResponse>> LoginWithApple(LoginWithAppleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginWithAppleCommand(request.IdentityToken, request.FirstName, request.LastName, DeviceInfo, ClientIpAddress, AcceptLanguage), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshCommand(request.RefreshToken, DeviceInfo, ClientIpAddress), cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<MessageResponse>> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutCommand(CurrentUserId, request.RefreshToken), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("LOGGED_OUT", AcceptLanguage)));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email, AcceptLanguage), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("PASSWORD_RESET_OTP_SENT", AcceptLanguage)));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("anonymous")]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ResetPasswordCommand(request.Email, request.OtpCode, request.NewPassword, DeviceInfo, ClientIpAddress, AcceptLanguage), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("PASSWORD_RESET", AcceptLanguage)));
    }

    [HttpPost("delete-account/request")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<MessageResponse>> RequestAccountDeletion(CancellationToken cancellationToken)
    {
        await _mediator.Send(new RequestAccountDeletionCommand(CurrentUserId, AcceptLanguage), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("ACCOUNT_DELETION_OTP_SENT", AcceptLanguage)));
    }

    [HttpPost("delete-account/confirm")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<MessageResponse>> ConfirmAccountDeletion(ConfirmAccountDeletionRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmAccountDeletionCommand(CurrentUserId, request.OtpCode, DeviceInfo, ClientIpAddress), cancellationToken);
        return Ok(new MessageResponse(SuccessMessages.Resolve("ACCOUNT_DELETED", AcceptLanguage)));
    }
}
