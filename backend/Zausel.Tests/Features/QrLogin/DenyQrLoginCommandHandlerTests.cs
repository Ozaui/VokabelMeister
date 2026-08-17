using Moq;
using FluentAssertions;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.QrLogin;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Enums.Logging;

namespace Zausel.Tests.Features.QrLogin;

public class DenyQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrLoginSessionRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    public DenyQrLoginCommandHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private DenyQrLoginCommandHandler CreateHandler() =>
        new(_qrLoginSessionRepository.Object, _passwordService.Object, _securityLogger.Object);

    [Fact]
    public async Task Handle_SessionNotFound_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DenyQrLoginCommand("kotu-token", 1, null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_DifferentUserThanScanner_ThrowsQrSessionForbiddenException()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Scanned, ExpiresAt = DateTime.UtcNow.AddMinutes(1), UserId = 7 };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DenyQrLoginCommand("token", 99, null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionForbiddenException>();
    }

    [Fact]
    public async Task Handle_ScannerDenies_MarksSessionDeniedAndLogsQrLoginDenied()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Scanned, ExpiresAt = DateTime.UtcNow.AddMinutes(1), UserId = 7 };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new DenyQrLoginCommand("token", 7, "TestAgent/1.0", "9.9.9.9"), default);

        // ASSERT
        session.Status.Should().Be(QrLoginStatus.Denied);
        _qrLoginSessionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // SecurityLog: A-04'te eklenen QrLoginDenied olayı
        _securityLogger.Verify(s => s.LogAsync(LogEventType.QrLoginDenied, 7, null, "9.9.9.9", "TestAgent/1.0", "QR_LOGIN_DENIED", It.IsAny<CancellationToken>()), Times.Once);
    }
}
