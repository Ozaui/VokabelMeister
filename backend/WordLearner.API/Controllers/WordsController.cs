using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Features.Words;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/words")]
public class WordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WordsController(IMediator mediator) => _mediator = mediator;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<WordConceptListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<WordConceptListItemDto>>> GetWords(
        [FromQuery] string? level,
        [FromQuery] string? partOfSpeech,
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var query = new GetWordsQuery(level, partOfSpeech, search, page, pageSize, categoryId);
        return Ok(await _mediator.Send(query, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(WordConceptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WordConceptDetailDto>> GetWordById(int id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetWordByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WordConceptDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WordConceptDetailDto>> CreateWord(
        [FromQuery] bool force,
        CreateWordCommand command,
        CancellationToken ct
    )
    {
        var response = await _mediator.Send(
            command with
            {
                Force = force,
                UserId = CurrentUserId,
                ActorRole = CurrentRole,
            },
            ct
        );
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WordConceptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WordConceptDetailDto>> UpdateWord(
        int id,
        [FromQuery] bool force,
        UpdateWordCommand command,
        CancellationToken ct
    ) =>
        Ok(
            await _mediator.Send(
                command with
                {
                    Id = id,
                    Force = force,
                    UserId = CurrentUserId,
                    ActorRole = CurrentRole,
                },
                ct
            )
        );

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWord(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWordCommand(id) { UserId = CurrentUserId, ActorRole = CurrentRole }, ct);
        return NoContent();
    }

    [HttpGet("unmatched")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<UnmatchedWordConceptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UnmatchedWordConceptDto>>> GetUnmatchedWords(
        [FromQuery] int languageId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var query = new GetUnmatchedWordConceptsQuery(languageId, search, page, pageSize);
        return Ok(await _mediator.Send(query, ct));
    }

    [HttpPost("pair")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WordConceptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WordConceptDetailDto>> PairWordConcepts(
        PairWordConceptsCommand command,
        CancellationToken ct
    ) => Ok(await _mediator.Send(command with { UserId = CurrentUserId, ActorRole = CurrentRole }, ct));
}
