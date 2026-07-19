// ─────────────────────────────────────────────────────────────────────────────
// SecurityLoggerTests.cs
//
// AMAÇ: SecurityLogger'ın SecurityLog kaydını doğru alanlarla kurup
//       ISecurityLogRepository.AddAsync'e geçirdiğini — özellikle e-postanın
//       ham hâlde DEĞİL, IPasswordService.HashToken ile hash'lenerek yazıldığını
//       (PII kuralı, SECURITY.md §6) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

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

    /// <summary>
    /// LogAsync_EmailGiven_HashesEmailBeforeStoring
    ///
    /// AMAÇ: email verildiğinde ham e-postanın DEĞİL, IPasswordService.HashToken'ın
    ///       ürettiği hash'in EmailHash'e yazıldığını doğrulamak — PII kuralı gereği.
    /// </summary>
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

    /// <summary>
    /// LogAsync_NoEmailGiven_LeavesEmailHashNull
    ///
    /// AMAÇ: email verilmediğinde (ör. TokenReplay — yalnızca userId biliniyor)
    ///       EmailHash'in null kaldığını ve HashToken'ın hiç ÇAĞRILMADIĞINI doğrulamak.
    /// </summary>
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

    /// <summary>
    /// LogAsync_AllFieldsGiven_PassesThemAllToSecurityLog
    ///
    /// AMAÇ: ipAddress/userAgent/detail verildiğinde SecurityLog'un ilgili alanlarına
    ///       aynen yazıldığını doğrulamak.
    /// </summary>
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
