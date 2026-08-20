using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Features.Categories;

namespace Zausel.API.Controllers;

[Route("categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories(
        [FromQuery] string? level, [FromQuery] bool includeWordCount, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(level, includeWordCount), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCategoryCommand(
                request.ParentCategoryId, request.DisplayOrder, request.Icon, request.Color, request.MinLevel, request.MaxLevel,
                request.Translations, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCategoryCommand(
                id, request.ParentCategoryId, request.DisplayOrder, request.Icon, request.Color, request.MinLevel, request.MaxLevel,
                request.Translations, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id, CurrentUserId, CurrentUserRole), cancellationToken);
        return NoContent();
    }
}
