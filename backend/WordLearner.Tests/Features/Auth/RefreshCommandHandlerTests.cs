// ─────────────────────────────────────────────────────────────────────────────
// RefreshCommandHandlerTests.cs
//
// AMAÇ: RefreshCommandHandler'ın Token Family Pattern'ini (rotation, replay
//       tespiti, family iptali) doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions, AutoMapper (AuthProfile).
// ─────────────────────────────────────────────────────────────────────────────

using AutoMapper;
using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class RefreshCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();

    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<AuthProfile>()).CreateMapper();

    private RefreshCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _tokenService.Object,
            _loginCompletionService.Object,
            CreateMapper()
        );

    private void SetupTokenService()
    {
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));
        _loginCompletionService.Setup(l => l.ExpiresInSeconds()).Returns(900);
    }

    /// <summary>
    /// Refresh_ValidToken_RotatesTokenAndReturnsNewPair
    ///
    /// AMAÇ: Geçerli bir refresh token ile yeni bir access+refresh token çifti
    ///       üretildiğini ve eski token'ın kullanıldı (IsUsed=true) olarak işaretlendiğini
    ///       doğrulamak.
    /// </summary>
    [Fact]
    public async Task Refresh_ValidToken_RotatesTokenAndReturnsNewPair()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        var mevcutToken = new RefreshToken
        {
            UserId = user.Id,
            TokenFamily = "family-1",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
        };
        _passwordService.Setup(p => p.HashToken("eski-refresh-token")).Returns("eski-hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("eski-hash", default)).ReturnsAsync(mevcutToken);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("refresh-token")).Returns("yeni-hash");
        SetupTokenService();
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new RefreshCommand("eski-refresh-token") { ClientIp = "1.2.3.4" },
            default
        );

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        mevcutToken.IsUsed.Should().BeTrue();
        _refreshTokenRepo.Verify(
            r => r.AddAsync(It.Is<RefreshToken>(t => t.TokenFamily == "family-1"), user.Id, default),
            Times.Once
        );
    }

    /// <summary>
    /// Refresh_TokenNotFound_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: DB'de bulunamayan bir refresh token için InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Refresh_TokenNotFound_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync((RefreshToken?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("gecersiz-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    /// <summary>
    /// Refresh_TokenExpired_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Süresi dolmuş bir refresh token için InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Refresh_TokenExpired_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var suresiGecmisToken = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(-1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(suresiGecmisToken);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("suresi-gecmis-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    /// <summary>
    /// Refresh_TokenAlreadyUsed_RevokesEntireFamilyAndThrows
    ///
    /// AMAÇ: Zaten kullanılmış (IsUsed=true) bir refresh token TEKRAR kullanıldığında
    ///       (replay saldırısı) aynı TokenFamily'deki TÜM token'ların iptal edildiğini
    ///       ve InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// NEDEN: Token Family Pattern'in en kritik davranışı — bir token çalınıp kullanılmışsa
    ///        gerçek kullanıcı bir sonraki refresh'te bunu (replay) tetikler ve tüm
    ///        family (dolayısıyla saldırganın elindeki token da) iptal edilir.
    /// </summary>
    [Fact]
    public async Task Refresh_TokenAlreadyUsed_RevokesEntireFamilyAndThrows()
    {
        // ARRANGE
        var kullanilmisToken = new RefreshToken
        {
            TokenFamily = "family-replay",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsUsed = true,
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(kullanilmisToken);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("kullanilmis-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenRepo.Verify(r => r.RevokeFamilyAsync("family-replay", default), Times.Once);
    }

    /// <summary>
    /// Refresh_UserAnonymized_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Token geçerli olsa bile ait olduğu kullanıcı anonimleştirilmişse
    ///       (IsAnonymized=true) refresh'in reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Refresh_UserAnonymized_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true, IsAnonymized = true };
        var token = new RefreshToken { UserId = user.Id, TokenFamily = "family-1", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(token);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RefreshCommand("token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }
}
