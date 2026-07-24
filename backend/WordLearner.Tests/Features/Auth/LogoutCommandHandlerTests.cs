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

    [Fact]
    public async Task Logout_OwnToken_RevokesToken()
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

    [Fact]
    public async Task Logout_TokenBelongsToDifferentUser_ThrowsInvalidRefreshTokenException()
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
