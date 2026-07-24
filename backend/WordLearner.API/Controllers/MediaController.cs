// ─────────────────────────────────────────────────────────────────────────────
// MediaController.cs
//
// AMAÇ: POST /media/images/upload — admin panelin bir kelime kartına ekleyeceği
//       görseli sunucuya yükleyip herkese açık bir URL döndüren uç nokta
//       (REFERENCE/ENV.md §7).
// NEDEN MediatR Command+Handler DEĞİL: HealthController ile AYNI gerekçe — bu
//       bir CQRS dikey dilimi değil, tek bir servis çağrısına (IFileStorageService)
//       sarılı saf bir G/Ç işlemi; iş kuralı/domain mantığı taşımıyor (YAGNI).
// BAĞIMLILIKLAR: IFileStorageService, IActivityLogger.
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.API.Controllers;

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

    // AMAÇ: WordsController/AdminController ile AYNI desen — IActivityLogger'a "kim yaptı" bilgisi.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    // AMAÇ: Bir görseli yükler, diske yazar ve herkese açık URL'ini döner.
    // NEDEN Admin: Görsel yalnızca admin panelin kelime formu üzerinden yüklenir
    //       (B-03) — CLAUDE.md "sistem içeriği CRUD Admin" kuralıyla aynı sınıf.
    [HttpPost("images/upload")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaUploadResponse>> UploadImage(IFormFile? file, CancellationToken ct)
    {
        // NEDEN file NULLABLE (kod denetiminde bulundu): `IFormFile file` (nullable OLMAYAN)
        //       yazılsaydı, `[ApiController]` + nullable-reference-types kombinasyonu bu alanı
        //       OTOMATİK zorunlu sayar — istek `file` alanı hiç GÖNDERMEDEN atılırsa, MVC
        //       action'a hiç GİRMEDEN kendi ham ProblemDetails JSON'ını döner (projenin
        //       ApiErrorResponse şekli DEĞİL, ErrorMessages'tan da GEÇMEZ). `IFormFile?` bu
        //       otomatik davranışı KAPATIR, kontrol elle (aşağıda) yapılır.
        if (file is null || file.Length == 0)
            throw new FileRequiredException();

        await using var stream = file.OpenReadStream();
        var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.Length, ct);

        // NEDEN EntityType=Word/EntityId=NULL: Görsel bu aşamada henüz hiçbir
        //       WordConcept'e bağlanmadı — admin panel formu yükleme bittikten
        //       SONRA dönen URL'i POST/PUT /words'ün imageUrl alanına koyar (A-05).
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
