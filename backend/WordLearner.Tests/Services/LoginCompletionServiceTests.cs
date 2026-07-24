using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Services;

public class LoginCompletionServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IOtpService> _otpService = new();

    // AMAÇ: Testlerde gerçek appsettings.json okumadan sabit bir Jwt:ExpirationMinutes sağlar.
    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:ExpirationMinutes"] = "15" })
            .Build();

    private LoginCompletionService CreateService() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _tokenService.Object,
            _otpService.Object,
            CreateConfiguration(),
            AuthTestMapper.Create()
        );

    private void SetupTokenService()
    {
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
    }

    [Fact]
    public async Task CompleteLoginAsync_ActiveUser_ReturnsAccessAndRefreshTokens()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        SetupTokenService();
        var service = CreateService();

        // ACT
        var sonuc = await service.CompleteLoginAsync(user, "1.2.3.4");

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        sonuc.RefreshToken.Should().Be("refresh-token");
        sonuc.AccountWasRecovered.Should().BeFalse();
        user.LastLoginIP.Should().Be("1.2.3.4");
        user.LoginCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteLoginAsync_AnonymizedUser_ThrowsAccountAnonymizedException()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsAnonymized = true };
        var service = CreateService();

        // ACT
        var act = () => service.CompleteLoginAsync(user, null);

        // ASSERT
        await act.Should().ThrowAsync<AccountAnonymizedException>();
    }

    [Fact]
    public async Task CompleteLoginAsync_AccountWithinGracePeriod_RecoversAccountAndFlagsResponse()
    {
        // ARRANGE
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            IsActive = true,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-10),
            ScheduledDeletionAt = DateTime.UtcNow.AddDays(20),
        };
        SetupTokenService();
        var service = CreateService();

        // ACT
        var sonuc = await service.CompleteLoginAsync(user, null);

        // ASSERT
        sonuc.AccountWasRecovered.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.ScheduledDeletionAt.Should().BeNull();
    }

    [Fact]
    public async Task CompleteLoginAsync_AlwaysClearsPendingOtp()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        SetupTokenService();
        var service = CreateService();

        // ACT
        await service.CompleteLoginAsync(user, null);

        // ASSERT
        _otpService.Verify(o => o.Clear(user), Times.Once);
    }

    [Fact]
    public void ExpiresInSeconds_ReadsFromConfiguration()
    {
        // ARRANGE
        var service = CreateService();

        // ACT
        var sonuc = service.ExpiresInSeconds();

        // ASSERT
        sonuc.Should().Be(15 * 60);
    }
}
