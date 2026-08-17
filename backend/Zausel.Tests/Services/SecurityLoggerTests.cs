using Moq;
using Zausel.Application.Interfaces.Repositories.Logging;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Logging;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Tests.Services;

public class SecurityLoggerTests
{
    private readonly Mock<ISecurityLogRepository> _securityLogRepository = new();

    private SecurityLogger CreateLogger() => new(_securityLogRepository.Object, new PasswordService());

    [Fact]
    public async Task LogAsync_EmailProvided_StoresHashNotRawEmail()
    {
        // ARRANGE
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(LogEventType.LoginFailed, email: "user@example.com", ipAddress: "1.2.3.4");

        // ASSERT — ham e-posta hiçbir zaman EmailHash'e yazılmaz, SHA-256 → Base64 44 karakter
        _securityLogRepository.Verify(r => r.AddAsync(It.Is<SecurityLog>(l =>
            l.EventType == LogEventType.LoginFailed &&
            l.EmailHash != "user@example.com" &&
            l.EmailHash != null && l.EmailHash.Length == 44 &&
            l.IpAddress == "1.2.3.4"),
            It.IsAny<CancellationToken>()), Times.Once);
        _securityLogRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogAsync_NoEmailProvided_LeavesEmailHashNull()
    {
        // ARRANGE — ör. TokenReplay gibi e-postanın hiç bilinmediği bir olay
        var logger = CreateLogger();

        // ACT
        await logger.LogAsync(LogEventType.TokenReplay, userId: 7);

        // ASSERT
        _securityLogRepository.Verify(r => r.AddAsync(It.Is<SecurityLog>(l =>
            l.UserId == 7 &&
            l.EmailHash == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
