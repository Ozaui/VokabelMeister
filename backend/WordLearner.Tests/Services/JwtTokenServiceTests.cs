using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Services;

public class JwtTokenServiceTests
{
    // AMAÇ: Testlerde gerçek appsettings.json okumadan sabit bir Jwt yapılandırması sağlar.
    // NEDEN: JwtTokenService IConfiguration["Jwt:SecretKey"] gibi indeksleyicileri kullanır;
    //        bunu Moq ile sahtelemek yerine gerçek bir in-memory IConfiguration kurmak
    //        hem daha az kırılgan hem de GetValue<T> extension metodunun gerçek davranışını test eder.
    private static IConfiguration CreateConfiguration(
        string secretKey = "test-secret-key-en-az-32-karakter-olmali!!"
    ) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = secretKey,
                    ["Jwt:Issuer"] = "WordLearnerApp",
                    ["Jwt:Audience"] = "WordLearnerApp",
                    ["Jwt:ExpirationMinutes"] = "15",
                    ["Jwt:RefreshTokenExpirationDays"] = "7",
                }
            )
            .Build();

    private static User CreateUser() =>
        new()
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test",
            Role = "User",
        };

    [Fact]
    public void GenerateAccessToken_ValidUser_ProducesTokenWithExpectedClaims()
    {
        // ARRANGE
        var servis = new JwtTokenService(CreateConfiguration());
        var kullanici = CreateUser();

        // ACT
        var token = servis.GenerateAccessToken(kullanici);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // ASSERT — her claim doğru değeri taşımalı
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "1");
        jwt.Claims.Should()
            .Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwt.Claims.Should().Contain(c => c.Type == "firstName" && c.Value == "Test");
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_SetsExpirationFromConfiguration()
    {
        // ARRANGE
        var servis = new JwtTokenService(CreateConfiguration());
        var kullanici = CreateUser();

        // ACT
        var token = servis.GenerateAccessToken(kullanici);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // ASSERT — 15 dakikalık pencereye (küçük bir tolerans ile) yakın olmalı
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_Called_ProducesHighEntropyUniqueTokens()
    {
        // ARRANGE
        var servis = new JwtTokenService(CreateConfiguration());

        // ACT
        var token1 = servis.GenerateRefreshToken();
        var token2 = servis.GenerateRefreshToken();

        // ASSERT
        token1.Token.Should().NotBe(token2.Token);
        token1.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ValidSignatureButExpired_ReturnsPrincipal()
    {
        // ARRANGE — normal üretilen bir access token (süresi henüz dolmamış ama imza geçerli)
        var servis = new JwtTokenService(CreateConfiguration());
        var token = servis.GenerateAccessToken(CreateUser());

        // ACT
        var principal = servis.GetPrincipalFromExpiredToken(token);

        // ASSERT — ValidateLifetime=false olduğu için süresi dolmamış bir token da kabul edilir,
        //          burada asıl doğrulanan imza + algoritma kontrolünün çalıştığıdır.
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("1");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_TamperedSignature_ReturnsNull()
    {
        // ARRANGE — biri farklı bir gizli anahtarla token üretir, diğeri kendi anahtarıyla doğrular
        var saldirganServisi = new JwtTokenService(
            CreateConfiguration("baska-bir-gizli-anahtar-32-karakter!!!!")
        );
        var sahteToken = saldirganServisi.GenerateAccessToken(CreateUser());
        var gercekServis = new JwtTokenService(CreateConfiguration());

        // ACT
        var principal = gercekServis.GetPrincipalFromExpiredToken(sahteToken);

        // ASSERT
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_MalformedToken_ReturnsNull()
    {
        // ARRANGE
        var servis = new JwtTokenService(CreateConfiguration());

        // ACT
        var principal = servis.GetPrincipalFromExpiredToken("bu-bir-jwt-degil");

        // ASSERT
        principal.Should().BeNull();
    }
}
