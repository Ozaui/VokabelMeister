// ─────────────────────────────────────────────────────────────────────────────
// ActivityLoggerTests.cs
//
// AMAÇ: ActivityLogger'ın ActivityLog kaydını doğru alanlarla kurup
//       IActivityLogRepository.AddAsync'e geçirdiğini (özellikle OldValue/NewValue
//       JSON serileştirmesini) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Tests.Services;

public class ActivityLoggerTests
{
    private readonly Mock<IActivityLogRepository> _repository = new();

    private ActivityLogger CreateLogger() => new(_repository.Object);

    /// <summary>
    /// LogAsync_MinimalFields_AddsActivityLogWithoutOldOrNewValue
    ///
    /// AMAÇ: yalnızca zorunlu alanlarla (userId/actorRole/action) çağrıldığında
    ///       OldValue/NewValue'nun null kaldığını doğrulamak (ör. CREATE eylemi
    ///       için yalnızca NewValue verilir, ikisi de opsiyoneldir).
    /// </summary>
    [Fact]
    public async Task LogAsync_MinimalFields_AddsActivityLogWithoutOldOrNewValue()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(7, "Admin", "DELETE_USER_CARD", "UserCard", 42, ct: default);

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(
                It.Is<ActivityLog>(a =>
                    a.UserId == 7
                    && a.ActorRole == "Admin"
                    && a.Action == "DELETE_USER_CARD"
                    && a.EntityType == "UserCard"
                    && a.EntityId == 42
                    && a.OldValue == null
                    && a.NewValue == null
                ),
                default
            ),
            Times.Once
        );
    }

    /// <summary>
    /// LogAsync_OldAndNewValueGiven_SerializesBothAsJson
    ///
    /// AMAÇ: oldValue/newValue nesneleri verildiğinde JSON'a serileştirilip
    ///       OldValue/NewValue kolonlarına yazıldığını doğrulamak (ör. UPDATE eylemi
    ///       için audit diff).
    /// </summary>
    [Fact]
    public async Task LogAsync_OldAndNewValueGiven_SerializesBothAsJson()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(
            7,
            "Admin",
            "UPDATE_WORD",
            "WordConcept",
            42,
            oldValue: new { Level = "A1" },
            newValue: new { Level = "A2" },
            ct: default
        );

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(
                It.Is<ActivityLog>(a =>
                    a.OldValue == """{"Level":"A1"}"""
                    && a.NewValue == """{"Level":"A2"}"""
                ),
                default
            ),
            Times.Once
        );
    }

    /// <summary>
    /// LogAsync_IpAndUserAgentGiven_PassesThemToActivityLog
    ///
    /// AMAÇ: ipAddress/userAgent verildiğinde ActivityLog'un ilgili alanlarına
    ///       aynen yazıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LogAsync_IpAndUserAgentGiven_PassesThemToActivityLog()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(
            7,
            "User",
            "CREATE_WORD",
            ipAddress: "1.2.3.4",
            userAgent: "TestAgent/1.0",
            ct: default
        );

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(
                It.Is<ActivityLog>(a => a.IpAddress == "1.2.3.4" && a.UserAgent == "TestAgent/1.0"),
                default
            ),
            Times.Once
        );
    }
}
