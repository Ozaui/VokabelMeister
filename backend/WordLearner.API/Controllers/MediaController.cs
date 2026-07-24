using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.API.Controllers;

// MediatR Command+Handler DEĞİL — HealthController ile aynı gerekçe, saf bir G/Ç işlemi.
[ApiController]
[Route("api/v1/media")]
public class MediaController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IActivityLogger _activityLogger;

    public MediaController(IFileStorageService fileStorageService, IActivityLogger activityLogger)
    {
        _fileStorageService = fileStorageService;
        _activityLogger = activityLogger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    [HttpPost("images/upload")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaUploadResponse>> UploadImage(IFormFile? file, CancellationToken ct)
    {
        // file NULLABLE — nullable olmayan olsaydı [ApiController] bu alanı otomatik zorunlu
        // sayar, boş istek MVC action'a hiç girmeden ham ProblemDetails JSON'ı dönerdi.
        if (file is null || file.Length == 0)
            throw new FileRequiredException();

        await using var stream = file.OpenReadStream();
        var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.Length, ct);

        // EntityId=NULL — görsel bu aşamada henüz hiçbir WordConcept'e bağlanmadı.
        await _activityLogger.LogAsync(
            CurrentUserId,
            CurrentRole,
            "UPLOAD_MEDIA",
            entityType: "Word",
            entityId: null,
            newValue: new { Url = url, file.FileName, file.Length },
            ct: ct
        );

        return StatusCode(StatusCodes.Status201Created, new MediaUploadResponse(url));
    }
}
