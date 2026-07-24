// ─────────────────────────────────────────────────────────────────────────────
// AdminController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §11'deki admin endpoint'lerini MediatR üzerinden
//       ilgili Command/Query'e bağlayan ince controller katmanı.
// NEDEN: Tümü [Authorize(Roles="Admin")] — WordsController/CategoriesController'ın
//        aksine bu controller'da ROL AYRIMI yok, her endpoint zaten yalnızca Admin'e
//        açık (CLAUDE.md "Roller ve sahiplik": "Hiçbir public endpoint rol
//        yükseltemez" — bu controller'ın kendisi role yükseltebilen tek yer,
//        bu yüzden en üst seviyede [Authorize(Roles="Admin")] uygulanır).
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.API.Common;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Features.Admin;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut admin'in Id'sini okur —
    //       IActivityLogger/ISecurityLogger'a "kim yaptı" bilgisini taşımak için.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: JWT'deki Role claim'i — ActivityLog.ActorRole'e yazılır.
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    // AMAÇ: SecurityLog'a yazılan IP — rol/durum değişimi gibi hassas işlemlerde izlenir.
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    // AMAÇ: İsteğin Accept-Language'ından çıkarılan dil kodu — GetSecurityLogsQuery'nin
    //       SecurityLog.Detail'i çözerken kullandığı dil (AuthController'daki AYNI desen).
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);

    // AMAÇ: Arama+role filtreli sayfalı kullanıcı listesi.
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetUsersQuery(search, role, page, pageSize), ct));

    // AMAÇ: Bir kullanıcının detay+istatistik görünümü.
    [HttpGet("users/{id:int}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailDto>> GetUserById(int id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUserByIdQuery(id), ct));

    // AMAÇ: Bir kullanıcının rolünü değiştirir (User↔Admin).
    [HttpPut("users/{id:int}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleCommand command, CancellationToken ct)
    {
        await _mediator.Send(
            command with
            {
                Id = id,
                UserId = CurrentUserId,
                ActorRole = CurrentRole,
                IpAddress = ClientIp,
            },
            ct
        );
        return NoContent();
    }

    // AMAÇ: Bir hesabı dondurur/aktif eder.
    [HttpPut("users/{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusCommand command, CancellationToken ct)
    {
        await _mediator.Send(
            command with
            {
                Id = id,
                UserId = CurrentUserId,
                ActorRole = CurrentRole,
                IpAddress = ClientIp,
            },
            ct
        );
        return NoContent();
    }

    // AMAÇ: Genel istatistik — toplam/aktif/dondurulmuş kullanıcı, toplam kelime/kategori,
    //       son `daysForGraph` günün kayıt grafiği için ham sayılar.
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AdminStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatisticsDto>> GetStatistics(
        [FromQuery] int daysForGraph = 30,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetAdminStatisticsQuery(daysForGraph), ct));

    // AMAÇ: Toplu kelime import — satır bazlı best-effort, her satır bağımsız bir
    //       WordConcept açar (bkz. BulkImportWordsCommand.cs "NEDEN" notu).
    [HttpPost("words/import")]
    [ProducesResponseType(typeof(BulkImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkImportResultDto>> BulkImportWords(BulkImportWordsCommand command, CancellationToken ct) =>
        Ok(await _mediator.Send(command with { UserId = CurrentUserId, ActorRole = CurrentRole }, ct));

    // AMAÇ: Audit log görüntüleme — Action/OldValue/NewValue HAM döner (çeviri yok,
    //       bkz. GetActivityLogsQuery.cs "NEDEN Language YOK").
    [HttpGet("logs/activity")]
    [ProducesResponseType(typeof(PagedResult<ActivityLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetActivityLogs(
        [FromQuery] int? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetActivityLogsQuery(userId, action, entityType, from, to, page, pageSize), ct));

    // AMAÇ: Teknik log (Serilog) görüntüleme — İngilizce, çeviri yok.
    [HttpGet("logs/application")]
    [ProducesResponseType(typeof(PagedResult<ApplicationLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationLogDto>>> GetApplicationLogs(
        [FromQuery] string? level,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetApplicationLogsQuery(level, from, to, search, page, pageSize), ct));

    // AMAÇ: Güvenlik olayı görüntüleme — Detail, admin'in KENDİ Accept-Language'ına göre çözülür.
    [HttpGet("logs/security")]
    [ProducesResponseType(typeof(PagedResult<SecurityLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SecurityLogDto>>> GetSecurityLogs(
        [FromQuery] LogEventType? eventType,
        [FromQuery] string? ip,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetSecurityLogsQuery(eventType, ip, from, to, page, pageSize) { Language = Language }, ct));
}
