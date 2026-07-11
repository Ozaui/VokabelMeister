// ─────────────────────────────────────────────────────────────────────────────
// LoginWithGoogleCommandHandlerTests.cs
//
// AMAÇ: LoginWithGoogleCommandHandler'ın hesap bulma/oluşturma/bağlama (account
//       linking) mantığını doğrulamak. Giriş tamamlama (ILoginCompletionService)
//       mock'lanır — bkz. LoginCompletionServiceTests.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class LoginWithGoogleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IGoogleTokenValidator> _googleValidator = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();

    private LoginWithGoogleCommandHandler CreateHandler() =>
        new(_userRepo.Object, _googleValidator.Object, _loginCompletionService.Object);

    private void SetupLoginCompletion() =>
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync(new AuthTokenResponse("access-token", "refresh-token", 900, new AuthUserDto(1, "A1"), false));

    /// <summary>
    /// LoginWithGoogle_ExistingGoogleUser_ReturnsTokensWithoutCreatingNewAccount
    ///
    /// AMAÇ: GoogleId ile eşleşen bir kullanıcı bulunduğunda yeni hesap açılmadan
    ///       doğrudan token döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogle_ExistingGoogleUser_ReturnsTokensWithoutCreatingNewAccount()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-123", user.Email, "Test", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default)).ReturnsAsync(user);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new LoginWithGoogleCommand("id-token"), default);

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), null, default), Times.Never);
    }

    /// <summary>
    /// LoginWithGoogle_NoExistingAccount_CreatesNewUser
    ///
    /// AMAÇ: Ne GoogleId ne de e-posta ile eşleşen bir kullanıcı bulunmadığında yeni
    ///       hesap oluşturulduğunu doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogle_NoExistingAccount_CreatesNewUser()
    {
        // ARRANGE
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-999", "yeni@example.com", "Yeni", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-999", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("yeni@example.com", default)).ReturnsAsync((User?)null);
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithGoogleCommand("id-token"), default);

        // ASSERT
        _userRepo.Verify(
            r => r.AddAsync(It.Is<User>(u => u.GoogleId == "google-999" && u.AuthProvider == "Google"), null, default),
            Times.Once
        );
    }

    /// <summary>
    /// LoginWithGoogle_EmailMatchesExistingLocalAccount_LinksGoogleIdToAccount
    ///
    /// AMAÇ: Google'dan gelen e-posta, mevcut bir yerel (Local) hesapla eşleştiğinde
    ///       yeni hesap açmak yerine GoogleId'nin o hesaba bağlandığını (account linking)
    ///       doğrulamak.
    /// NEDEN: Aksi hâlde aynı kişi aynı e-posta için iki ayrı hesaba sahip olur —
    ///        "user register olurken hangi hesaba giriyor" tasarım kararının kalbi.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogle_EmailMatchesExistingLocalAccount_LinksGoogleIdToAccount()
    {
        // ARRANGE
        var mevcutYerelHesap = new User { Id = 1, Email = "ortak@example.com", IsActive = true };
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-777", "ortak@example.com", "Ortak", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-777", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("ortak@example.com", default)).ReturnsAsync(mevcutYerelHesap);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithGoogleCommand("id-token"), default);

        // ASSERT — yeni kayıt AÇILMAMALI, mevcut hesaba GoogleId bağlanmalı
        mevcutYerelHesap.GoogleId.Should().Be("google-777");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), null, default), Times.Never);
    }

    /// <summary>
    /// LoginWithGoogle_InvalidToken_ThrowsInvalidSocialTokenException
    ///
    /// AMAÇ: Google token doğrulaması null döndüğünde InvalidSocialTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogle_InvalidToken_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _googleValidator.Setup(g => g.ValidateAsync("gecersiz-token", default)).ReturnsAsync((GoogleTokenPayload?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithGoogleCommand("gecersiz-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }

    /// <summary>
    /// LoginWithGoogle_AccountNotActive_ThrowsAccountNotActiveException
    ///
    /// AMAÇ: Google ile eşleşen hesap dondurulmuşsa (IsActive=false) login'in reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogle_AccountNotActive_ThrowsAccountNotActiveException()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = false };
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-123", user.Email, null, null));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default)).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithGoogleCommand("id-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountNotActiveException>();
    }
}
