using FluentAssertions;
using Moq;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Logging;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Services;

public class SecurityLoggerTests
{
    private readonly Mock<ISecurityLogRepository> _repository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private SecurityLogger CreateLogger() => new(_repository.Object, _passwordService.Object);

    [Fact]
    public async Task LogAsync_EmailGiven_HashesEmailBeforeStoring()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken("test@example.com")).Returns("hashed-email");
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(LogEventType.LoginFailed, email: "test@example.com", ct: default);

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(
                It.Is<SecurityLog>(s =>
                    s.EmailHash == "hashed-email" && s.EventType == LogEventType.LoginFailed
                ),
                default
            ),
            Times.Once
        );
        _passwordService.Verify(p => p.HashToken("test@example.com"), Times.Once);
    }

    [Fact]
    public async Task LogAsync_NoEmailGiven_LeavesEmailHashNull()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(LogEventType.TokenReplay, userId: 7, ct: default);

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(It.Is<SecurityLog>(s => s.EmailHash == null && s.UserId == 7), default),
            Times.Once
        );
        _passwordService.Verify(p => p.HashToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LogAsync_AllFieldsGiven_PassesThemAllToSecurityLog()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(
            LogEventType.RateLimitHit,
            ipAddress: "1.2.3.4",
            userAgent: "TestAgent/1.0",
            detail: "/api/v1/auth/login",
            ct: default
        );

        // ASSERT
        _repository.Verify(
            r => r.AddAsync(
                It.Is<SecurityLog>(s =>
                    s.IpAddress == "1.2.3.4"
                    && s.UserAgent == "TestAgent/1.0"
                    && s.Detail == "/api/v1/auth/login"
                ),
                default
            ),
            Times.Once
        );
    }
}
