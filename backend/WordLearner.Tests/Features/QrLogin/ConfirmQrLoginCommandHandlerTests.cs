using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.QrLogin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.QrLogin;

public class ConfirmQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private ConfirmQrLoginCommandHandler CreateHandler() =>
        new(_qrRepo.Object, _passwordService.Object, _securityLogger.Object);

    [Fact]
    public async Task Confirm_ScannedSessionOwnedByUser_TransitionsToConfirmed()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            UserId = 5,
            RequesterIp = "1.2.3.4",
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ConfirmQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        session.Status.Should().Be(QrLoginStatus.Confirmed);
        session.ConfirmedAt.Should().NotBeNull();
        // NEDEN: audit alanı (UpdatedByUserId) onaylayan kullanıcının Id'siyle doldurulmalı.
        _qrRepo.Verify(r => r.UpdateAsync(session, 5, default), Times.Once);
    }

    [Fact]
    public async Task Confirm_ScannedSessionOwnedByUser_LogsQrLoginConfirmedSecurityEvent()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            UserId = 5,
            RequesterIp = "1.2.3.4",
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ConfirmQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        _securityLogger.Verify(
            s => s.LogAsync(LogEventType.QrLoginConfirmed, 5, null, "1.2.3.4", null, null, default),
            Times.Once
        );
    }

    [Fact]
    public async Task Confirm_TokenNotFound_ThrowsEntityNotFoundExceptionWithoutLeakingRawToken()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken("gecersiz-token")).Returns("opaque-sha256-abc123");
        _qrRepo
            .Setup(r => r.GetByTokenHashAsync("opaque-sha256-abc123", default))
            .ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmQrLoginCommand("gecersiz-token") { UserId = 5 }, default);

        // ASSERT
        var sonuc = await act.Should().ThrowAsync<EntityNotFoundException>();
        sonuc.Which.Message.Should().NotContain("gecersiz-token");
        sonuc.Which.Message.Should().Contain("opaque-sha256-abc123");
    }

    [Fact]
    public async Task Confirm_WrongUser_ThrowsQrSessionForbiddenException()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmQrLoginCommand("token") { UserId = 99 }, default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionForbiddenException>();
        session.Status.Should().Be(QrLoginStatus.Scanned);
    }

    [Fact]
    public async Task Confirm_NotScanned_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Confirm_ExpiredSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ConfirmQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
        session.Status.Should().Be(QrLoginStatus.Expired);
    }
}
