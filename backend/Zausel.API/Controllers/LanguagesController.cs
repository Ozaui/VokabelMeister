using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs.Languages;
using Zausel.Application.Features.Languages;

namespace Zausel.API.Controllers;

[Route("languages")]
public class LanguagesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public LanguagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<LanguageResponse>>> GetLanguages(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLanguagesQuery(), cancellationToken);
        return Ok(result);
    }
}
