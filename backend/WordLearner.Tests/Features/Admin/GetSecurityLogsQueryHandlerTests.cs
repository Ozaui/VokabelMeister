// ─────────────────────────────────────────────────────────────────────────────
// GetSecurityLogsQueryHandlerTests.cs
//
// AMAÇ: GetSecurityLogsQueryHandler'ın EventType'ı string'e çevirdiğini VE
//       Detail'i istenen dile göre ÇÖZDÜĞÜNÜ (bilinen kod → çeviri, bilinmeyen
//       kod → aynen, null → null) doğrulamak — CLAUDE.md "İkinci istisna".
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Admin;

public class GetSecurityLogsQueryHandlerTests
{
    private readonly Mock<ISecurityLogRepository> _securityLogRepo = new();

    private GetSecurityLogsQueryHandler CreateHandler() => new(_securityLogRepo.Object);

    /// <summary>
    /// Handle_KnownDetailCode_ResolvesToRequestedLanguage
    /// </summary>
    [Fact]
    public async Task Handle_KnownDetailCode_ResolvesToRequestedLanguage()
    {
        // ARRANGE
        var log = new SecurityLog
        {
            Id = 1,
            EventType = LogEventType.AdminAction,
            Detail = "USER_ROLE_CHANGED",
        };
        _securityLogRepo
            .Setup(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<SecurityLog>(new List<SecurityLog> { log }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetSecurityLogsQuery(null, null, null, null) { Language = "de" }, default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.EventType == "AdminAction" && i.Detail == "Benutzerrolle geändert");
    }

    /// <summary>
    /// Handle_UnknownDetailCode_PassesThroughUnchanged
    ///
    /// AMAÇ: RateLimitHit'in Detail'i (bir istek yolu, ör. "/api/v1/auth/login") sözlükte
    ///       YOK — LocalizedMessageResolver bunu AYNEN döner, hata FIRLATMAZ.
    /// </summary>
    [Fact]
    public async Task Handle_UnknownDetailCode_PassesThroughUnchanged()
    {
        // ARRANGE
        var log = new SecurityLog
        {
            Id = 2,
            EventType = LogEventType.RateLimitHit,
            Detail = "/api/v1/auth/login",
        };
        _securityLogRepo
            .Setup(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<SecurityLog>(new List<SecurityLog> { log }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetSecurityLogsQuery(null, null, null, null) { Language = "tr" }, default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.Detail == "/api/v1/auth/login");
    }

    /// <summary>
    /// Handle_NullDetail_StaysNull
    /// </summary>
    [Fact]
    public async Task Handle_NullDetail_StaysNull()
    {
        // ARRANGE
        var log = new SecurityLog { Id = 3, EventType = LogEventType.QrLoginConfirmed, Detail = null };
        _securityLogRepo
            .Setup(r => r.GetPagedAsync(null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<SecurityLog>(new List<SecurityLog> { log }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetSecurityLogsQuery(null, null, null, null), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.Detail == null);
    }
}
