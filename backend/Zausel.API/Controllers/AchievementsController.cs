using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs.Achievements;
using Zausel.Application.Features.Achievements;

namespace Zausel.API.Controllers;

[Route("achievements")]
[Authorize]
public class AchievementsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AchievementsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<AchievementResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAchievementsQuery(CurrentUserId, AcceptLanguage), cancellationToken);
        return Ok(result);
    }
}
