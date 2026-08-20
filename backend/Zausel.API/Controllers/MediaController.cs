using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Media;
using Zausel.Application.Features.Media;

namespace Zausel.API.Controllers;

[Route("media")]
public class MediaController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator) => _mediator = mediator;

    // file: IFormFile? (nullable) — [ApiController]'ın ASP.NET'in kendi ham 400 gövdesini otomatik
    // üretmesini engeller, eksik dosya bizim standart ApiErrorResponse şeklimize (FileRequiredException) düşer.
    [HttpPost("images/upload")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("general")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<MediaUploadResponse>> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            throw new FileRequiredException();

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadImageCommand(stream, file.FileName, file.Length, CurrentUserId, CurrentUserRole),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
