using Moq;
using FluentAssertions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    public LogoutCommandHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private LogoutCommandHandler CreateHandler() => new(_refreshTokenRepository.Object, _passwordService.Object);

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsUnitIdempotently()
    {
        // ARRANGE — logout idempotent olmalı, bulunamayan token'da da başarı döner
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LogoutCommand(1, "ham-token"), default);

        // ASSERT
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TokenBelongsToOtherUser_ReturnsUnitWithoutRevoking()
    {
        // ARRANGE — başkasının token'ını manipüle etmeye çalışıldığı ayrı bir hatayla belli edilmez
        var token = new RefreshToken { UserId = 99, RevokedAt = null };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LogoutCommand(1, "ham-token"), default);

        // ASSERT
        token.RevokedAt.Should().BeNull();
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRevoked_ReturnsUnitIdempotently()
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1, RevokedAt = DateTime.UtcNow.AddMinutes(-5) };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LogoutCommand(1, "ham-token"), default);

        // ASSERT
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesTokenAndSaves()
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1, RevokedAt = null };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LogoutCommand(1, "ham-token"), default);

        // ASSERT
        token.RevokedAt.Should().NotBeNull();
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
