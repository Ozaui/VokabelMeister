using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Application.Features.UserCategories;

namespace Zausel.API.Controllers;

[Route("user-categories")]
[Authorize]
public class UserCategoriesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UserCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<List<UserCategoryResponse>>> GetUserCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserCategoriesQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<UserCategoryResponse>> Create(UserCategoryCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateUserCategoryCommand(request.Name, request.Description, request.Color, request.Icon, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<UserCategoryResponse>> Update(int id, UserCategoryUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateUserCategoryCommand(id, request.Name, request.Description, request.Color, request.Icon, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("general")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCategoryCommand(id, CurrentUserId, CurrentUserRole), cancellationToken);
        return NoContent();
    }
}
