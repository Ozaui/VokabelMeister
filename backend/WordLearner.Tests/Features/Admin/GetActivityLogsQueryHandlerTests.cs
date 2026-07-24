// ─────────────────────────────────────────────────────────────────────────────
// GetActivityLogsQueryHandlerTests.cs
//
// AMAÇ: GetActivityLogsQueryHandler'ın filtreleri repository'ye AYNEN ilettiğini
//       VE Action/OldValue/NewValue'yu HİÇ ÇEVİRMEDEN (ham) DTO'ya taşıdığını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Tests.Features.Admin;

public class GetActivityLogsQueryHandlerTests
{
    private readonly Mock<IActivityLogRepository> _activityLogRepo = new();

    private GetActivityLogsQueryHandler CreateHandler() => new(_activityLogRepo.Object);

    /// <summary>
    /// Handle_ForwardsFiltersAndMapsRawActionAndValues
    /// </summary>
    [Fact]
    public async Task Handle_ForwardsFiltersAndMapsRawActionAndValues()
    {
        // ARRANGE
        var log = new ActivityLog
        {
            Id = 1,
            UserId = 5,
            ActorRole = "Admin",
            Action = "CREATE_WORD",
            EntityType = "WordConcept",
            EntityId = 42,
            NewValue = "{\"PartOfSpeech\":\"Noun\"}",
        };
        _activityLogRepo
            .Setup(r => r.GetPagedAsync(5, "CREATE_WORD", "WordConcept", null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ActivityLog>(new List<ActivityLog> { log }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetActivityLogsQuery(5, "CREATE_WORD", "WordConcept", null, null), default);

        // ASSERT — Action ÇEVRİLMEDEN, NewValue HAM JSON olarak geldi.
        result.Items.Should().ContainSingle(i => i.Action == "CREATE_WORD" && i.NewValue == "{\"PartOfSpeech\":\"Noun\"}");
    }
}
