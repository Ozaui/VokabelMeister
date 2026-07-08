// ─────────────────────────────────────────────────────────────────────────────
// GetQrLoginStatusCommandHandlerTests.cs
//
// AMAÇ: GetQrLoginStatusCommandHandler'ın Confirmed'i İLK okuduğunda token
//       üretip Consumed'e geçtiğini, Consumed sonrası tekrar okumanın 410
//       döndüğünü, Expired'ın ise 410 DEĞİL sadece durum bilgisiyle döndüğünü doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.QrLogin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.QrLogin;

public class GetQrLoginStatusCommandHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();

    private GetQrLoginStatusCommandHandler CreateHandler() =>
        new(_qrRepo.Object, _passwordService.Object, _userRepo.Object, _loginCompletionService.Object);

    /// <summary>
    /// GetStatus_ConfirmedSession_CompletesLoginAndConsumesSession
    ///
    /// AMAÇ: Confirmed bir oturum İLK okunduğunda ILoginCompletionService ile token
    ///       üretildiğini, oturumun Consumed'e geçtiğini ve yanıtta token'ların döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConfirmedSession_CompletesLoginAndConsumesSession()
    {
        // ARRANGE
        var user = new User { Id = 5, Email = "test@example.com" };
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Confirmed,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        _userRepo.Setup(r => r.GetByIdAsync(5, default)).ReturnsAsync(user);
        var authResponse = new AuthTokenResponse("access", "refresh", 900, new AuthUserDto(5, "A1"), false);
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(user, "1.2.3.4", default))
            .ReturnsAsync(authResponse);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusCommand("token") { ClientIp = "1.2.3.4" }, default);

        // ASSERT
        result.Status.Should().Be("Confirmed");
        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        session.Status.Should().Be(QrLoginStatus.Consumed);
    }

    /// <summary>
    /// GetStatus_ConsumedSession_ThrowsQrSessionGoneException
    ///
    /// AMAÇ: Token'lar bir kez döndükten sonra (Consumed) aynı oturum TEKRAR
    ///       okunmaya çalışılırsa QrSessionGoneException (410) fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConsumedSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Consumed,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
        _loginCompletionService.Verify(
            l => l.CompleteLoginAsync(It.IsAny<User>(), It.IsAny<string?>(), default),
            Times.Never
        );
    }

    /// <summary>
    /// GetStatus_PendingSession_ReturnsStatusOnlyWithoutTokens
    ///
    /// AMAÇ: Henüz Confirmed olmayan bir oturum için yalnızca Status alanının
    ///       dolduğunu, token alanlarının null döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetStatus_PendingSession_ReturnsStatusOnlyWithoutTokens()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        result.Status.Should().Be("Pending");
        result.AccessToken.Should().BeNull();
    }

    /// <summary>
    /// GetStatus_ExpiredSession_ReturnsExpiredStatusWithoutThrowing
    ///
    /// AMAÇ: ExpiresAt geçmiş bir oturum sorgulandığında 410 DEĞİL, 200 +
    ///       {status:"Expired"} döndüğünü doğrulamak — web bunu "yeni QR üret"
    ///       sinyali olarak kullanır, henüz hiçbir token sızmadığı için "gone" değildir.
    /// </summary>
    [Fact]
    public async Task GetStatus_ExpiredSession_ReturnsExpiredStatusWithoutThrowing()
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
        var result = await handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        result.Status.Should().Be("Expired");
        session.Status.Should().Be(QrLoginStatus.Expired);
    }

    /// <summary>
    /// GetStatus_TokenNotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Hash'e karşılık gelen bir oturum bulunamazsa EntityNotFoundException (404) fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetStatus_TokenNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusCommand("gecersiz-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
