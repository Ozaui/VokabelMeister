// ─────────────────────────────────────────────────────────────────────────────
// SmtpSettingsController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §11'deki `/admin/smtp-settings` uç noktalarını
//       MediatR üzerinden ilgili Command/Query'e bağlayan ince controller katmanı.
// NEDEN AdminController'A EKLENMEDİ, AYRI bir controller: WordsController/
//       CategoriesController/MediaController ile AYNI desen — Admin-only bir
//       domain kaynağı, kendi controller'ında yaşar; AdminController yalnızca
//       "Kullanıcı Yönetimi/İstatistik/Toplu Import/Log Görüntüleme" dört
//       dilimini taşır (A-07), SMTP kendi domain'i (System) olduğu için ayrı.
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

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

[ApiController]
[Route("api/v1/admin/smtp-settings")]
[Authorize(Roles = "Admin")]
public class SmtpSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SmtpSettingsController(IMediator mediator) => _mediator = mediator;

    // AMAÇ: AdminController ile AYNI desen — IActivityLogger/ISecurityLogger'a "kim yaptı" bilgisi.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    // AMAÇ: Test e-postasının gönderileceği adres — isteği yapan admin'in KENDİ e-postası.
    private string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email)!;

    // AMAÇ: MessageResponse.Message'ın hangi dilde çözüleceği (AuthController ile aynı desen).
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);

    // AMAÇ: Kayıtlı SMTP ayarlarını döner — şifre alanı "***" ile maskelenir.
    [HttpGet]
    [ProducesResponseType(typeof(SmtpSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SmtpSettingsDto>> GetSmtpSettings(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetSmtpSettingsQuery(), ct));

    // AMAÇ: SMTP ayarlarını kaydeder (ilk kayıtta oluşturur, sonrasında günceller).
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

    // AMAÇ: Kayıtlı SMTP ayarlarıyla gerçekten bağlanıp isteği yapan admin'in kendi
    //       e-posta adresine bir test e-postası gönderir.
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
