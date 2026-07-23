// ─────────────────────────────────────────────────────────────────────────────
// WordsController.cs
//
// AMAÇ: REFERENCE/API_ENDPOINTS.md §5'teki sistem kelimesi (Words) endpoint'lerini
//       MediatR üzerinden ilgili Command/Query'e bağlayan ince controller katmanı.
// NEDEN: Liste/detay okuma herkese açık ([Authorize] — giriş yapmış herkes),
//        CRUD yalnızca Admin ([Authorize(Roles="Admin")]) — CLAUDE.md "Roller ve
//        sahiplik": sistem içeriği CRUD Admin, okuma [Authorize]. Bu, projedeki
//        İLK `[Authorize(Roles="Admin")]` kullanımıdır (Auth/QrLogin'de emsal yoktu).
// BAĞIMLILIKLAR: IMediator, ValidationFilter (global, Program.cs'de kayıtlı).
// ─────────────────────────────────────────────────────────────────────────────

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

    // AMAÇ: JWT'deki NameIdentifier claim'inden mevcut kullanıcının Id'sini okur —
    //       IActivityLogger'a "kim yaptı" bilgisini taşımak için (AuthController deseni).
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // AMAÇ: JWT'deki Role claim'i — ActivityLog.ActorRole'e yazılır (kaydın yazıldığı
    //       andaki rolü dondurur, kullanıcının rolü sonradan değişse bile).
    private string? CurrentRole => User.FindFirstValue(ClaimTypes.Role);

    // AMAÇ: Filtre+sayfalı kelime kavramı listesi.
    // NEDEN categoryId parametresi (A-06 eklemesi): API_ENDPOINTS.md §5'in "level,
    //        categoryId, partOfSpeech, search, page, pageSize" filtre listesinde
    //        A-05 döneminden beri VARDI ama Category tabloları o zaman yoktu.
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

    // AMAÇ: Bir kelime kavramının tüm dilleriyle (WordDetail+örnekler dahil) tam detayı.
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(WordConceptDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WordConceptDetailDto>> GetWordById(int id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetWordByIdQuery(id), ct));

    // AMAÇ: Yeni bir WordConcept + 1 veya 2 dilde translations[] oluşturur.
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

    // AMAÇ: Mevcut çevirileri günceller veya kavrama eksik olan dili ekler.
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

    // AMAÇ: WordConcept'i + tüm dillerini soft-delete eder.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWord(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWordCommand(id) { UserId = CurrentUserId, ActorRole = CurrentRole }, ct);
        return NoContent();
    }

    // AMAÇ: `languageId`'de eşleşmemiş (tek dilli) kavramların filtre+sayfalı
    //       listesi + karşı dilin havuzunda önerilen eşleşme adayı.
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

    // AMAÇ: `otherConceptId`'nin tek Word'ünü `primaryId`'ye taşıyarak iki
    //       eşleşmemiş kavramı tek (2 dilli) kavrama birleştirir.
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
