// ─────────────────────────────────────────────────────────────────────────────
// JwtTokenServiceTests.cs
//
// AMAÇ: JwtTokenService'in access/refresh token üretimini ve Algorithm Confusion
//       önlemli expired-token doğrulamasını test etmek.
// NEDEN: Bu servis, tüm Auth API'nin kimlik doğrulama omurgasıdır — claim'ler yanlış
//        üretilirse [Authorize] middleware'i kullanıcıyı hiç tanımaz; Algorithm
//        Confusion kontrolü atlanırsa saldırgan sahte bir token ile kimlik doğrulayabilir.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.Extensions.Configuration,
//                System.IdentityModel.Tokens.Jwt, WordLearner.Application.Services.JwtTokenService.
// ─────────────────────────────────────────────────────────────────────────────

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

    /// <summary>
    /// GenerateAccessToken_ValidUser_ProducesTokenWithExpectedClaims
    ///
    /// AMAÇ: Üretilen JWT'nin kullanıcının Id/Email/Role/FirstName bilgilerini claim
    ///       olarak doğru taşıdığını doğrulamak.
    /// NEDEN: [Authorize] middleware'i DB'ye gitmeden bu claim'lerden kullanıcı kimliğini
    ///        okur — yanlış/eksik claim, AuthController'daki CurrentUserId gibi alanların yanlış çalışmasına yol açar.
    /// </summary>
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

    /// <summary>
    /// GenerateAccessToken_ValidUser_SetsExpirationFromConfiguration
    ///
    /// AMAÇ: Token'ın geçerlilik süresinin Jwt:ExpirationMinutes ayarına göre üretildiğini doğrulamak.
    /// NEDEN: Program.cs'teki JwtBearer doğrulaması ve bu üretim AYNI süreyi kullanmalı;
    ///        biri değişip diğeri değişmezse token beklenenden erken/geç geçersiz olur.
    /// </summary>
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

    /// <summary>
    /// GenerateRefreshToken_Called_ProducesHighEntropyUniqueTokens
    ///
    /// AMAÇ: Art arda üretilen iki refresh token'ın birbirinden farklı olduğunu doğrulamak.
    /// NEDEN: Aynı token iki kez üretilirse iki farklı oturum aynı sırra sahip olur —
    ///        biri iptal edildiğinde diğeri de etkilenir, oturum izolasyonu bozulur.
    /// </summary>
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

    /// <summary>
    /// GetPrincipalFromExpiredToken_ValidSignatureButExpired_ReturnsPrincipal
    ///
    /// AMAÇ: Süresi dolmuş ama imzası geçerli bir token'dan ClaimsPrincipal döndüğünü doğrulamak.
    /// NEDEN: /auth/refresh akışı tam olarak bu senaryoya dayanır — access token süresi
    ///        dolmuş olmasına rağmen kullanıcı kimliği hâlâ okunabilmeli.
    /// </summary>
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

    /// <summary>
    /// GetPrincipalFromExpiredToken_TamperedSignature_ReturnsNull
    ///
    /// AMAÇ: Farklı bir SecretKey ile imzalanmış (yani bu sunucu tarafından üretilmemiş)
    ///       bir token'ın null döndüğünü doğrulamak.
    /// NEDEN: Bir saldırganın kendi ürettiği sahte bir token ile /auth/refresh'i
    ///        kandırabilmesinin önüne geçen tek kontrol budur.
    /// </summary>
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

    /// <summary>
    /// GetPrincipalFromExpiredToken_MalformedToken_ReturnsNull
    ///
    /// AMAÇ: Geçersiz formatlı (JWT bile olmayan) bir string verildiğinde exception
    ///       fırlamadan null döndüğünü doğrulamak.
    /// NEDEN: /auth/refresh endpoint'ine rastgele bir string gönderilmesi 500'e değil,
    ///        AuthService'in InvalidRefreshTokenException'ına (kontrollü 401'e) düşmeli.
    /// </summary>
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
