using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.Features.Progress;

namespace Zausel.API.Controllers;

[Route("progress")]
[Authorize]
public class ProgressController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProgressController(IMediator mediator) => _mediator = mediator;

    [HttpGet("summary")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<ProgressSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgressSummaryQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("words")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<ProgressWordResponse>>> GetWords([FromQuery] string band, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgressWordsQuery(CurrentUserId, band), cancellationToken);
        return Ok(result);
    }

    [HttpGet("suspended")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<SuspendedWordResponse>>> GetSuspended(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSuspendedWordsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }
}
