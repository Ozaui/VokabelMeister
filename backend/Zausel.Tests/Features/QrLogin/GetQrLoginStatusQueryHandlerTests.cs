using Moq;
using FluentAssertions;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.QrLogin;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Tests.Features.QrLogin;

public class GetQrLoginStatusQueryHandlerTests
{
    private readonly Mock<IQrLoginSessionRepository> _qrLoginSessionRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();
    private readonly Mock<IEmailService> _emailService = new();

    public GetQrLoginStatusQueryHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private GetQrLoginStatusQueryHandler CreateHandler() => new(
        _qrLoginSessionRepository.Object, _userRepository.Object, _refreshTokenRepository.Object,
        _passwordService.Object, _loginCompletionService.Object, _emailService.Object);

    private static LoginCompletionResult CreateCompletion(bool recovered = false) =>
        new("access-token", "refresh-token", new RefreshToken { UserId = 1 }, recovered);

    [Fact]
    public async Task Handle_SessionNotFound_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusQuery("kotu-token", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_ConsumedSession_ThrowsQrSessionGoneException()
    {
        // ARRANGE — Consumed = token zaten teslim edildi, TEK SEFERLİK kuralı (SECURITY.md §1.3 ADIM 4)
        var session = new QrLoginSession { Status = QrLoginStatus.Consumed };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusQuery("token", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_PendingSessionPastExpiry_MarksExpiredAndReturnsExpiredStatus()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Pending, ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusQuery("token", "tr"), default);

        // ASSERT — A-17 ExpiredTokenCleanupJob'un filtresi için DB'ye gerçek bir durum olarak yazılır
        session.Status.Should().Be(QrLoginStatus.Expired);
        result.Status.Should().Be("Expired");
        _qrLoginSessionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ScannedSessionNotExpired_ReturnsScannedStatusWithoutTokens()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Scanned, ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusQuery("token", "tr"), default);

        // ASSERT — polling akışı, Confirmed dışında yalnızca status doludur
        result.Status.Should().Be("Scanned");
        result.AccessToken.Should().BeNull();
        _loginCompletionService.Verify(l => l.Complete(It.IsAny<User>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConfirmedSessionWithMissingUser_ThrowsQrSessionGoneException()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Confirmed, ExpiresAt = DateTime.UtcNow.AddMinutes(1), UserId = 1 };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusQuery("token", "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<QrSessionGoneException>();
    }

    [Fact]
    public async Task Handle_ConfirmedSession_CompletesLoginAndMarksConsumed()
    {
        // ARRANGE
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Confirmed,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
            UserId = 1,
            RequesterDeviceInfo = "Chrome/Mac",
            RequesterIp = "1.2.3.4"
        };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var user = new User { Id = 1, CurrentLevel = "A1", ThemePreference = "System", LanguagePreference = "tr" };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var completion = CreateCompletion();
        _loginCompletionService.Setup(l => l.Complete(user, "Chrome/Mac", "1.2.3.4")).Returns(completion);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusQuery("token", "tr"), default);

        // ASSERT — yanıttaki status TÜKETİLMEDEN ÖNCEKİ (Confirmed), session'a yazılan ise Consumed
        result.Status.Should().Be("Confirmed");
        result.AccessToken.Should().Be("access-token");
        result.User!.Id.Should().Be(1);
        session.Status.Should().Be(QrLoginStatus.Consumed);
        _refreshTokenRepository.Verify(r => r.AddAsync(completion.RefreshTokenEntity, It.IsAny<CancellationToken>()), Times.Once);
        _qrLoginSessionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConfirmedSessionAccountWasRecovered_SendsAccountRecoveredNotification()
    {
        // ARRANGE
        var session = new QrLoginSession { Status = QrLoginStatus.Confirmed, ExpiresAt = DateTime.UtcNow.AddMinutes(1), UserId = 1 };
        _qrLoginSessionRepository.Setup(r => r.GetByTokenHashAsync("hash-of-token", It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var user = new User { Id = 1, Email = "ada@test.de", FirstName = "Ada" };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _loginCompletionService.Setup(l => l.Complete(user, null, null)).Returns(CreateCompletion(recovered: true));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetQrLoginStatusQuery("token", "de"), default);

        // ASSERT
        _emailService.Verify(e => e.SendAccountRecoveredNotificationAsync("ada@test.de", "Ada", "de", It.IsAny<CancellationToken>()), Times.Once);
    }
}
