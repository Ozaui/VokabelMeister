// ─────────────────────────────────────────────────────────────────────────────
// DenyQrLoginCommandHandlerTests.cs
//
// AMAÇ: DenyQrLoginCommandHandler'ın ConfirmQrLoginCommandHandler ile birebir
//       aynı ön koşulları (Scanned + sahiplik) uyguladığını, hedef durumun
//       Denied olduğunu doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.QrLogin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.QrLogin;

public class DenyQrLoginCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private DenyQrLoginCommandHandler CreateHandler() => new(_qrRepo.Object, _passwordService.Object);

    /// <summary>
    /// Deny_ScannedSessionOwnedByUser_TransitionsToDenied
    ///
    /// AMAÇ: Scanned bir oturumu, onu tarayan kullanıcı reddedince Denied'e geçtiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Deny_ScannedSessionOwnedByUser_TransitionsToDenied()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Scanned,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new DenyQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        session.Status.Should().Be(QrLoginStatus.Denied);
    }

    /// <summary>
    /// Deny_WrongUser_ThrowsQrSessionForbiddenException
    ///
    /// AMAÇ: Oturumu TARAMAMIŞ bir kullanıcı deny etmeye çalışırsa
    ///       QrSessionForbiddenException (403) fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Deny_WrongUser_ThrowsQrSessionForbiddenException()
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
        var act = () => handler.Handle(new DenyQrLoginCommand("token") { UserId = 99 }, default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionForbiddenException>();
    }

    /// <summary>
    /// Deny_NotScanned_ThrowsQrSessionGoneException
    ///
    /// AMAÇ: Henüz taranmamış bir oturum reddedilmeye çalışılırsa
    ///       QrSessionGoneException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Deny_NotScanned_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DenyQrLoginCommand("token") { UserId = 5 }, default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }
}
