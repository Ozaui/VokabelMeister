using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class RefreshCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    public RefreshCommandHandlerTests()
    {
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(t => $"hash-of-{t}");
    }

    private RefreshCommandHandler CreateHandler() => new(
        _userRepository.Object, _refreshTokenRepository.Object, _tokenService.Object, _passwordService.Object);

    [Fact]
    public async Task Handle_TokenNotFound_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("ham-token", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_TokenAlreadyUsed_RevokesFamilyAndThrowsInvalidRefreshTokenException()
    {
        // ARRANGE — Token Family Pattern: replay algılanınca TÜM family iptal edilir
        var token = new RefreshToken { TokenFamily = "family-1", IsUsed = true };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("ham-token", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenRepository.Verify(r => r.RevokeFamilyAsync("family-1", It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(true, false)]  // revoked
    [InlineData(false, true)]  // expired
    public async Task Handle_RevokedOrExpiredToken_ThrowsInvalidRefreshTokenException(bool revoked, bool expired)
    {
        // ARRANGE
        var token = new RefreshToken
        {
            IsUsed = false,
            RevokedAt = revoked ? DateTime.UtcNow : null,
            ExpiresAt = expired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(1)
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("ham-token", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Theory]
    [InlineData(false, false, false)] // inactive
    [InlineData(true, true, false)]   // deleted
    [InlineData(true, false, true)]   // anonymized
    public async Task Handle_UserNotUsable_ThrowsInvalidRefreshTokenException(bool isActive, bool isDeleted, bool isAnonymized)
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1, IsUsed = false, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var user = new User { Id = 1, IsActive = isActive, IsDeleted = isDeleted, IsAnonymized = isAnonymized };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("ham-token", null, null), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_ValidToken_RotatesTokenAndKeepsFamily()
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1, TokenFamily = "family-1", IsUsed = false, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash-of-ham-token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
        var user = new User { Id = 1, IsActive = true };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("yeni-access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns(new RefreshTokenResult("yeni-refresh", DateTime.UtcNow.AddDays(7)));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new RefreshCommand("ham-token", "Chrome", "1.2.3.4"), default);

        // ASSERT
        token.IsUsed.Should().BeTrue();
        result.AccessToken.Should().Be("yeni-access");
        result.RefreshToken.Should().Be("yeni-refresh");
        _refreshTokenRepository.Verify(r => r.AddAsync(It.Is<RefreshToken>(t =>
            t.UserId == 1 && t.TokenFamily == "family-1" && t.DeviceInfo == "Chrome" && t.IpAddress == "1.2.3.4"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
