using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs;
using Zausel.Application.DTOs.Progress;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Features.Progress;
using Zausel.Application.Features.UserCards;

namespace Zausel.API.Controllers;

[Route("user-cards")]
[Authorize]
public class UserCardsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UserCardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<PagedResult<UserCardResponse>>> GetUserCards(
        [FromQuery] int? categoryId, [FromQuery] int? userCategoryId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetUserCardsQuery(CurrentUserId, categoryId, userCategoryId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<UserCardResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserCardByIdQuery(id, CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<UserCardResponse>> Create(
        UserCardCreateRequest request, [FromQuery] bool force, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateUserCardCommand(
                request.FrontText, request.BackText, request.Notes, request.CategoryIds, request.UserCategoryIds,
                request.Examples, force, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<UserCardResponse>> Update(
        int id, UserCardUpdateRequest request, [FromQuery] bool force, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateUserCardCommand(
                id, request.FrontText, request.BackText, request.Notes, request.IsActive, request.CategoryIds,
                request.UserCategoryIds, request.Examples, force, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("general")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCardCommand(id, CurrentUserId, CurrentUserRole), cancellationToken);
        return NoContent();
    }

    [HttpPost("learn-system-word")]
    [EnableRateLimiting("general")]
    public async Task<ActionResult<LearnSystemWordResponse>> LearnSystemWord(
        LearnSystemWordRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LearnSystemWordCommand(request.WordId, CurrentUserId, CurrentUserRole), cancellationToken);
        return result.AlreadyExists ? Ok(result) : StatusCode(StatusCodes.Status201Created, result);
    }

    // file: IFormFile? (nullable) — MediaController.UploadImage ile AYNI desen, [ApiController]'ın
    // otomatik 400'ü yerine FileRequiredException standart ApiErrorResponse şeklini üretir.
    [HttpPost("{id}/image")]
    [EnableRateLimiting("general")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<UserCardResponse>> UploadImage(int id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            throw new FileRequiredException();

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadUserCardImageCommand(id, stream, file.FileName, file.Length, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return Ok(result);
    }
}
