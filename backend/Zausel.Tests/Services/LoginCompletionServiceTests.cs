using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Services;

public class LoginCompletionServiceTests
{
    private static LoginCompletionService CreateService()
    {
        var tokenService = new JwtTokenService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-icin-en-az-32-karakter-uzunlugunda-bir-anahtar",
            ["Jwt:Issuer"] = "ZauselApp",
            ["Jwt:Audience"] = "ZauselApp",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        }).Build());
        return new LoginCompletionService(tokenService, new PasswordService());
    }

    private static User CreateUser() => new()
    {
        Id = 7,
        Email = "test@example.com",
        FirstName = "Ada",
        Role = "User"
    };

    [Fact]
    public void Complete_AnonymizedUser_ThrowsAccountAnonymizedException()
    {
        // ARRANGE — SECURITY.md §1: IsAnonymized → 403 (kalıcı silindi)
        var service = CreateService();
        var user = CreateUser();
        user.IsAnonymized = true;

        // ACT
        var act = () => service.Complete(user, deviceInfo: null, ipAddress: null);

        // ASSERT
        act.Should().Throw<AccountAnonymizedException>();
    }

    [Fact]
    public void Complete_ActiveUser_ReturnsTokensAndUpdatesLoginMetadata()
    {
        // ARRANGE
        var service = CreateService();
        var user = CreateUser();

        // ACT
        var result = service.Complete(user, deviceInfo: "Chrome/Mac", ipAddress: "1.2.3.4");

        // ASSERT
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenValue.Should().NotBeNullOrWhiteSpace();
        result.AccountWasRecovered.Should().BeFalse();
        user.LastLoginIP.Should().Be("1.2.3.4");
        user.LoginCount.Should().Be(1);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_SoftDeletedUser_RecoversAccountAndReturnsAccountWasRecoveredTrue()
    {
        // ARRANGE — grace period içinde (IsAnonymized=false) silinmiş hesap otomatik kurtarılır
        var service = CreateService();
        var user = CreateUser();
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow.AddDays(-2);
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(28);
        user.IsActive = false;

        // ACT
        var result = service.Complete(user, deviceInfo: null, ipAddress: null);

        // ASSERT
        result.AccountWasRecovered.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.DeletedAt.Should().BeNull();
        user.ScheduledDeletionAt.Should().BeNull();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Complete_ValidUser_ReturnsRefreshTokenEntityWithHashedTokenNotRawValue()
    {
        // ARRANGE — ham refresh token asla RefreshTokens tablosuna yazılmaz, yalnızca SHA-256 hash'i
        var service = CreateService();
        var user = CreateUser();

        // ACT
        var result = service.Complete(user, deviceInfo: "iOS App", ipAddress: "5.6.7.8");

        // ASSERT
        result.RefreshTokenEntity.UserId.Should().Be(user.Id);
        result.RefreshTokenEntity.TokenHash.Should().NotBe(result.RefreshTokenValue);
        result.RefreshTokenEntity.TokenFamily.Should().NotBeNullOrWhiteSpace();
        result.RefreshTokenEntity.DeviceInfo.Should().Be("iOS App");
        result.RefreshTokenEntity.IpAddress.Should().Be("5.6.7.8");
        result.RefreshTokenEntity.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }
}
