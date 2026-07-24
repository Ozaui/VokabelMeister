using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Features.Categories;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories(
        [FromQuery] string? level,
        [FromQuery] bool includeWordCount = false,
        CancellationToken ct = default
    )
    {
        var query = new GetCategoriesQuery(level, includeWordCount);
        return Ok(await _mediator.Send(query, ct));
    }

    [HttpGet("{id:int}/words")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<WordConceptListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<WordConceptListItemDto>>> GetCategoryWords(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default
    )
    {
        var query = new GetCategoryWordsQuery(id, page, pageSize);
        return Ok(await _mediator.Send(query, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryCommand command, CancellationToken ct)
    {
        var response = await _mediator.Send(
            command with
            {
                UserId = CurrentUserId,
                ActorRole = CurrentRole,
            },
            ct
        );
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id,
        UpdateCategoryCommand command,
        CancellationToken ct
    ) =>
        Ok(
            await _mediator.Send(
                command with
                {
                    Id = id,
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand(id) { UserId = CurrentUserId, ActorRole = CurrentRole }, ct);
        return NoContent();
    }
}
