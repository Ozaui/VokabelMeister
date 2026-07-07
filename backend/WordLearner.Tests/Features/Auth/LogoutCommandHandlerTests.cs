// ─────────────────────────────────────────────────────────────────────────────
// LogoutCommandHandlerTests.cs
//
// AMAÇ: LogoutCommandHandler'ın sahiplik kontrolünü (yalnızca kendi token'ını
//       iptal edebilme) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private LogoutCommandHandler CreateHandler() => new(_refreshTokenRepo.Object, _passwordService.Object);

    /// <summary>
    /// LogoutAsync_OwnToken_RevokesToken
    ///
    /// AMAÇ: Kullanıcının kendi refresh token'ını iptal edebildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_OwnToken_RevokesToken()
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1 };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LogoutCommand("token") { UserId = 1 }, default);

        // ASSERT
        token.RevokedAt.Should().NotBeNull();
    }

    /// <summary>
    /// LogoutAsync_TokenBelongsToDifferentUser_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Başka bir kullanıcıya ait refresh token'ı iptal etmeye çalışıldığında
    ///       reddedildiğini doğrulamak.
    /// NEDEN: Sahiplik kontrolü olmazsa bir kullanıcı başka birinin oturumunu kapatabilirdi.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_TokenBelongsToDifferentUser_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var baskasininTokeni = new RefreshToken { UserId = 2 };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(baskasininTokeni);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LogoutCommand("baskasinin-tokeni") { UserId = 1 }, default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }
}
