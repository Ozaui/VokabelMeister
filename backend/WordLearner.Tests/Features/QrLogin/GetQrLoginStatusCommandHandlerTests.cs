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

// NEDEN bu dosyaya ek testler (2026-07-11, denetim sonrası): GetQrLoginStatusCommandHandler
// artık GetByIdIncludingDeletedAsync kullanıyor (soft-delete filtresi YOK SAYILARAK) ve
// CompleteLoginAsync'ten önce IsActive kontrolü ekliyor — normal login akışlarıyla tutarlılık
// için (bkz. IUserRepository.GetByIdIncludingDeletedAsync NEDEN notu).

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
        _userRepo.Setup(r => r.GetByIdIncludingDeletedAsync(5, default)).ReturnsAsync(user);
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
        // NEDEN: audit alanı (UpdatedByUserId) token'ı alan kullanıcının Id'siyle doldurulmalı.
        _qrRepo.Verify(r => r.UpdateAsync(session, 5, default), Times.Once);
    }

    /// <summary>
    /// GetStatus_ConfirmedSession_SoftDeletedUserWithinGracePeriod_StillCompletesLogin
    ///
    /// AMAÇ: Kullanıcı QR akışı sırasında soft-delete'li (grace period içinde) olsa bile
    ///       GetByIdIncludingDeletedAsync ile bulunup CompleteLoginAsync'e ulaştığını
    ///       doğrulamak — normal login (GetByEmailAsync, IgnoreQueryFilters) ile aynı
    ///       davranış. Öncesinde GetByIdAsync (filtreli) kullanıldığı için bu senaryoda
    ///       anlamsız bir 404 dönüyordu; bu regresyon testidir.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConfirmedSession_SoftDeletedUserWithinGracePeriod_StillCompletesLogin()
    {
        // ARRANGE
        var user = new User
        {
            Id = 5,
            Email = "test@example.com",
            IsActive = true,
            IsDeleted = true,
            IsAnonymized = false,
        };
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Confirmed,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        _userRepo.Setup(r => r.GetByIdIncludingDeletedAsync(5, default)).ReturnsAsync(user);
        var authResponse = new AuthTokenResponse("access", "refresh", 900, new AuthUserDto(5, "A1"), true);
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(user, null, default))
            .ReturnsAsync(authResponse);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        result.AccessToken.Should().Be("access");
        session.Status.Should().Be(QrLoginStatus.Consumed);
    }

    /// <summary>
    /// GetStatus_ConfirmedSession_InactiveUser_ThrowsAccountNotActiveExceptionWithoutCompletingLogin
    ///
    /// AMAÇ: Kullanıcı dondurulmuşsa (IsActive=false) CompleteLoginAsync'e hiç
    ///       ulaşılmadan AccountNotActiveException fırlatıldığını ve oturumun
    ///       Consumed'e geçmediğini doğrulamak — LoginCommand/LoginWithGoogle/Apple
    ///       ile aynı kontrol.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConfirmedSession_InactiveUser_ThrowsAccountNotActiveExceptionWithoutCompletingLogin()
    {
        // ARRANGE
        var user = new User { Id = 5, Email = "test@example.com", IsActive = false };
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Confirmed,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        _userRepo.Setup(r => r.GetByIdIncludingDeletedAsync(5, default)).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountNotActiveException>();
        session.Status.Should().Be(QrLoginStatus.Confirmed);
        _loginCompletionService.Verify(
            l => l.CompleteLoginAsync(It.IsAny<User>(), It.IsAny<string?>(), default),
            Times.Never
        );
    }

    /// <summary>
    /// GetStatus_ConfirmedSession_AnonymizedUser_PropagatesExceptionWithoutConsumingSession
    ///
    /// AMAÇ: CompleteLoginAsync (kullanıcı anonimleştirilmişse) AccountAnonymizedException
    ///       fırlatırsa, bu exception'ın yutulmadan yukarı taşındığını VE oturumun
    ///       Consumed'e geçirilmediğini (token hiç üretilmediği için) doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetStatus_ConfirmedSession_AnonymizedUser_PropagatesExceptionWithoutConsumingSession()
    {
        // ARRANGE
        var user = new User { Id = 5, Email = "test@example.com", IsActive = true, IsAnonymized = true };
        var session = new QrLoginSession
        {
            Status = QrLoginStatus.Confirmed,
            UserId = 5,
            ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        };
        _passwordService.Setup(p => p.HashToken("token")).Returns("hash");
        _qrRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(session);
        _userRepo.Setup(r => r.GetByIdIncludingDeletedAsync(5, default)).ReturnsAsync(user);
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(user, null, default))
            .ThrowsAsync(new AccountAnonymizedException());
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountAnonymizedException>();
        session.Status.Should().Be(QrLoginStatus.Confirmed);
        _qrRepo.Verify(r => r.UpdateAsync(session, It.IsAny<int?>(), default), Times.Never);
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
        _passwordService.Setup(p => p.HashToken("gecersiz-token")).Returns("opaque-sha256-abc123");
        _qrRepo
            .Setup(r => r.GetByTokenHashAsync("opaque-sha256-abc123", default))
            .ReturnsAsync((QrLoginSession?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetQrLoginStatusCommand("gecersiz-token"), default);

        // ASSERT
        var sonuc = await act.Should().ThrowAsync<EntityNotFoundException>();
        // NEDEN: exception mesajı ham token'ı DEĞİL, hash'ini taşımalı — bkz. ScanQrLoginCommandHandlerTests.
        sonuc.Which.Message.Should().NotContain("gecersiz-token");
        sonuc.Which.Message.Should().Contain("opaque-sha256-abc123");
    }
}
