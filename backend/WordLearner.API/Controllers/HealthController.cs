// ─────────────────────────────────────────────────────────────────────────────
// HealthController.cs
//
// AMAÇ: GET /health — uygulamanın ve DB bağlantısının ayakta olup olmadığını
//       kontrol eden operasyonel uç nokta (load balancer/uptime monitoring için).
// NEDEN MediatR Command+Handler DEĞİL: Bu bir feature değil, saf altyapı kontrolü
//       (iş mantığı/kural yok, tek satırlık DB ping) — CQRS dikey dilimi burada
//       gereksiz katman eklerdi (YAGNI). Doğrudan DbContext enjekte edilir.
// BAĞIMLILIKLAR: WordLearnerDbContext.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.AspNetCore.Mvc;
using WordLearner.Application.DTOs;
using WordLearner.Infrastructure.Data;

namespace WordLearner.API.Controllers;

[ApiController]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    private readonly WordLearnerDbContext _db;

    public HealthController(WordLearnerDbContext db) => _db = db;

    // AMAÇ: Uygulamanın çalıştığını ve DB'ye bağlanabildiğini doğrular.
    // NASIL: Database.CanConnectAsync — gerçek bir sorgu çalıştırmadan bağlantı
    //        kurulabilirliğini test eder (hafif, sık çağrılabilir).
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
