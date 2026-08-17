using FluentAssertions;
using Moq;
using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Logging;

namespace Zausel.Tests.Services;

public class ActivityLoggerTests
{
    private readonly Mock<IActivityLogRepository> _activityLogRepository = new();

    private ActivityLogger CreateLogger() => new(_activityLogRepository.Object);

    [Fact]
    public async Task LogAsync_NoOldOrNewValue_SavesLogWithNullDiffColumns()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(userId: 5, actorRole: "User", action: "LOGIN");

        // ASSERT
        _activityLogRepository.Verify(r => r.AddAsync(It.Is<ActivityLog>(l =>
            l.UserId == 5 &&
            l.ActorRole == "User" &&
            l.Action == "LOGIN" &&
            l.OldValue == null &&
            l.NewValue == null),
            It.IsAny<CancellationToken>()), Times.Once);
        _activityLogRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogAsync_OldAndNewValueProvided_SerializesBothAsJson()
    {
        // ARRANGE — çağıran taraf hassas alanları (şifre/hash) zaten dışarıda bırakmış bir DTO geçirir
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(
            userId: 5,
            actorRole: "Admin",
            action: "UPDATE_WORD",
            entityType: "Word",
            entityId: 42,
            oldValue: new { Text = "eski" },
            newValue: new { Text = "yeni" });

        // ASSERT
        _activityLogRepository.Verify(r => r.AddAsync(It.Is<ActivityLog>(l =>
            l.EntityType == "Word" &&
            l.EntityId == 42 &&
            l.OldValue == "{\"Text\":\"eski\"}" &&
            l.NewValue == "{\"Text\":\"yeni\"}"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
