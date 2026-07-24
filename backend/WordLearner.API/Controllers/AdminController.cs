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

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Language => RequestLanguageResolver.Resolve(HttpContext);

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetUsersQuery(search, role, page, pageSize), ct));

    [HttpGet("users/{id:int}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailDto>> GetUserById(int id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetUserByIdQuery(id), ct));

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

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AdminStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminStatisticsDto>> GetStatistics(
        [FromQuery] int daysForGraph = 30,
        CancellationToken ct = default
    ) => Ok(await _mediator.Send(new GetAdminStatisticsQuery(daysForGraph), ct));

    [HttpPost("words/import")]
    [ProducesResponseType(typeof(BulkImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BulkImportResultDto>> BulkImportWords(BulkImportWordsCommand command, CancellationToken ct) =>
        Ok(await _mediator.Send(command with { UserId = CurrentUserId, ActorRole = CurrentRole }, ct));

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
