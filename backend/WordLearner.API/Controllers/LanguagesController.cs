using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Features.Words;

namespace WordLearner.API.Controllers;

// WordsController'dan AYRI — Language kendi entity'si (BaseEntity'siz, statik seed), CategoriesController
// gibi tek endpoint'lik küçük bir domain controller'ı için ayrı dosya açma emsali (SmtpSettingsController).
[ApiController]
[Route("api/v1/languages")]
public class LanguagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LanguagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LanguageDto>>> GetLanguages(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetLanguagesQuery(), ct));
}
