using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.DTOs;
using WordLearner.Infrastructure.Data;

namespace WordLearner.API.Controllers;

// MediatR Command+Handler DEĞİL — saf altyapı kontrolü (YAGNI), DbContext doğrudan enjekte edilir.
[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    private readonly WordLearnerDbContext _db;

    public HealthController(WordLearnerDbContext db) => _db = db;

    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken ct)
    {
        var databaseConnected = await _db.Database.CanConnectAsync(ct);
        var response = new HealthResponse(
            databaseConnected ? "Healthy" : "Unhealthy",
            databaseConnected,
            DateTime.UtcNow
        );

        return databaseConnected ? Ok(response) : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
