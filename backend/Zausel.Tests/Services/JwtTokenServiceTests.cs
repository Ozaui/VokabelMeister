using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zausel.Application.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Services;

public class JwtTokenServiceTests
{
    private const string SecretKey = "test-icin-en-az-32-karakter-uzunlugunda-bir-anahtar";

    private static JwtTokenService CreateService() =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = SecretKey,
            ["Jwt:Issuer"] = "ZauselApp",
            ["Jwt:Audience"] = "ZauselApp",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        }).Build());

    private static User CreateUser() => new()
    {
        Id = 42,
        Email = "test@example.com",
        FirstName = "Ada",
        Role = "User"
    };

    [Fact]
    public void GenerateAccessToken_ValidUser_ContainsExpectedClaims()
    {
        // ARRANGE
        var service = CreateService();
        var user = CreateUser();

        // ACT
        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // ASSERT — dört claim de (Id/Email/Role/firstName) token'a doğru yazılmış olmalı
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == "Ada");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsTokenWithFutureExpiry()
    {
        // ARRANGE
        var service = CreateService();

        // ACT
        var result = service.GenerateRefreshToken();

        // ASSERT — 7 gün sonrası (appsettings'teki RefreshTokenExpirationDays), makul bir tolerans içinde
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ValidToken_ReturnsPrincipal()
    {
        // ARRANGE
        var service = CreateService();
        var token = service.GenerateAccessToken(CreateUser());

        // ACT
        var principal = service.GetPrincipalFromExpiredToken(token);

        // ASSERT
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("42");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_TokenSignedWithDifferentAlgorithm_ReturnsNull()
    {
        // ARRANGE — AYNI anahtarla ama HmacSha384 ile imzalanmış bir token (algorithm confusion senaryosu)
        var service = CreateService();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var maliciousToken = new JwtSecurityToken(
            issuer: "ZauselApp", audience: "ZauselApp",
            claims: [new Claim(ClaimTypes.NameIdentifier, "42")],
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha384));
        var tokenString = new JwtSecurityTokenHandler().WriteToken(maliciousToken);

        // ACT
        var principal = service.GetPrincipalFromExpiredToken(tokenString);

        // ASSERT — imza AYNI anahtarla geçerli olsa bile, algoritma HmacSha256 DEĞİLSE reddedilmeli
        principal.Should().BeNull();
    }
}
