using Moq;
using FluentAssertions;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Auth;
using Zausel.Application.Features.QrLogin;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Tests.Features.QrLogin;

public class ScanQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrLoginSessionRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    public ScanQrLoginCommandHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private ScanQrLoginCommandHandler CreateHandler() =>
        new(_qrLoginSessionRepository.Object, _passwordService.Object);

    [Fact]
    public async Task Handle_SessionNotFound_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("kotu-token", 1), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_ExpiredSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("token", 1), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_NotPendingStatus_ThrowsQrSessionGoneException()
    {
        // ARRANGE — zaten taranmış bir session tekrar taranmaya çalışılıyor
        var session = new QrLoginSession { Status = QrLoginStatus.Scanned, ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("token", 1), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_PendingSession_MarksScannedAndReturnsRequesterInfo()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
            RequesterDeviceInfo = "Chrome/Mac",
            RequesterIp = "1.2.3.4",
            PairingCode = "4242"
        };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new ScanQrLoginCommand("token", 7), default);

        // ASSERT
        session.Status.Should().Be(QrLoginStatus.Scanned);
        session.UserId.Should().Be(7);
        session.ScannedAt.Should().NotBeNull();
        result.Should().Be(new QrLoginScanResponse("Chrome/Mac", "1.2.3.4", "4242"));
        _qrLoginSessionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
