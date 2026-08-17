using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Features.Words;

namespace Zausel.API.Controllers;

[Route("words")]
public class WordsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public WordsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<PagedResult<WordResponse>>> GetWords(
        [FromQuery] string? level, [FromQuery] string? partOfSpeech, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetWordsQuery(level, partOfSpeech, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<WordResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWordByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<WordResponse>> Create(
        WordCreateRequest request, [FromQuery] bool force, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateWordCommand(request.PartOfSpeech, request.DifficultyLevel, request.ImageUrl, request.Translations, force, CurrentUserId),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<WordResponse>> Update(
        int id, WordUpdateRequest request, [FromQuery] bool force, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateWordCommand(id, request.PartOfSpeech, request.DifficultyLevel, request.ImageUrl, request.Translations, force, CurrentUserId),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteWordCommand(id, CurrentUserId), cancellationToken);
        return NoContent();
    }
}
