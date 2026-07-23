// ─────────────────────────────────────────────────────────────────────────────
// CategoriesController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §6'daki kategori endpoint'lerini MediatR
//       üzerinden ilgili Command/Query'e bağlayan ince controller katmanı.
// NEDEN: WordsController (A-05) ile BİREBİR aynı yetki deseni — liste/detay okuma
//        [Authorize] (giriş yapmış herkes), CRUD yalnızca Admin
//        ([Authorize(Roles="Admin")]) — CLAUDE.md "Roller ve sahiplik": sistem
//        içeriği CRUD Admin, okuma [Authorize].
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut kullanıcının Id'sini okur —
    //       IActivityLogger'a "kim yaptı" bilgisini taşımak için (WordsController deseni).
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: JWT'deki Role claim'i — ActivityLog.ActorRole'e yazılır.
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    // AMAÇ: Hiyerarşik (ağaç) kategori listesi.
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

    // AMAÇ: Bir kategorinin kelimelerinin sayfalı listesi.
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

    // AMAÇ: Yeni bir Category + 1+ dilde translations[] oluşturur.
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

    // AMAÇ: Kategori alanlarını ve çevirilerini günceller.
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

    // AMAÇ: Kategoriyi soft-delete eder (alt kategori/aktif kelime varsa 409).
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
