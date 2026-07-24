using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.QrLogin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.QrLogin;

public class ScanQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private ScanQrLoginCommandHandler CreateHandler() => new(_qrRepo.Object, _passwordService.Object);

    [Fact]
    public async Task Scan_PendingSession_TransitionsToScannedAndReturnsRequesterInfo()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
            RequesterIp = "9.9.9.9",
            RequesterDeviceInfo = "Chrome/Windows",
            PairingCode = "4821",
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new ScanQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        session.Status.Should().Be(QrLoginStatus.Scanned);
        session.UserId.Should().Be(5);
        session.ScannedAt.Should().NotBeNull();
        result.RequesterIp.Should().Be("9.9.9.9");
        result.RequesterDeviceInfo.Should().Be("Chrome/Windows");
        result.PairingCode.Should().Be("4821");
        // NEDEN: audit alanı (UpdatedByUserId) oturumu tarayan kullanıcının Id'siyle
        //        doldurulmalı — bkz. IRepository.UpdateAsync NEDEN notu.
        _qrRepo.Verify(r => r.UpdateAsync(session, 5, default), Times.Once);
    }

    [Fact]
    public async Task Scan_TokenNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken("gecersiz-token")).Returns("opaque-sha256-abc123");
        _qrRepo
            .Setup(r => r.GetByTokenHashAsync("opaque-sha256-abc123", default))
            .ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("gecersiz-token"), default);

        // ASSERT
        var sonuc = await act.Should().ThrowAsync<EntityNotFoundException>();
        // NEDEN: exception mesajı ham token'ı DEĞİL, hash'ini taşımalı — ham QR
        //        token'ı bir secret'tir, log'a/exception mesajına sızmamalı.
        sonuc.Which.Message.Should().NotContain("gecersiz-token");
        sonuc.Which.Message.Should().Contain("opaque-sha256-abc123");
    }

    [Fact]
    public async Task Scan_ExpiredSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
        session.Status.Should().Be(QrLoginStatus.Expired);
        _qrRepo.Verify(r => r.UpdateAsync(session, null, default), Times.Once);
    }

    [Fact]
    public async Task Scan_AlreadyScannedSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new ScanQrLoginCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }
}
