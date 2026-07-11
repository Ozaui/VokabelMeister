// ─────────────────────────────────────────────────────────────────────────────
// LoginWithAppleCommandHandlerTests.cs
//
// AMAÇ: LoginWithAppleCommandHandler'ın hesap bulma/oluşturma/AppleId eşleştirme
//       mantığını (özellikle e-postasız sonraki girişleri) doğrulamak. Giriş
//       tamamlama (ILoginCompletionService) mock'lanır — bkz. LoginCompletionServiceTests.
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

public class LoginWithAppleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IAppleTokenValidator> _appleValidator = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();

    private LoginWithAppleCommandHandler CreateHandler() =>
        new(_userRepo.Object, _appleValidator.Object, _loginCompletionService.Object);

    private void SetupLoginCompletion() =>
        _loginCompletionService
            .Setup(l => l.CompleteLoginAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync(new AuthTokenResponse("access-token", "refresh-token", 900, new AuthUserDto(1, "A1"), false));

    /// <summary>
    /// LoginWithApple_ExistingAppleUser_ReturnsTokens
    ///
    /// AMAÇ: AppleId ile eşleşen mevcut bir kullanıcı için token döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithApple_ExistingAppleUser_ReturnsTokens()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-123", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-123", default)).ReturnsAsync(user);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new LoginWithAppleCommand("identity-token"), default);

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
    }

    /// <summary>
    /// LoginWithApple_FirstAuthorizationWithEmail_CreatesNewAccount
    ///
    /// AMAÇ: İlk yetkilendirmede (e-posta dolu) ne AppleId ne de e-posta eşleşmesi
    ///       olmadığında yeni hesap açıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithApple_FirstAuthorizationWithEmail_CreatesNewAccount()
    {
        // ARRANGE
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-999", "yeni@example.com"));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-999", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("yeni@example.com", default)).ReturnsAsync((User?)null);
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithAppleCommand("identity-token"), default);

        // ASSERT
        _userRepo.Verify(
            r => r.AddAsync(It.Is<User>(u => u.AppleId == "apple-999" && u.AuthProvider == "Apple"), null, default),
            Times.Once
        );
    }

    /// <summary>
    /// LoginWithApple_SubsequentLoginWithoutEmail_MatchesByAppleIdOnly
    ///
    /// AMAÇ: Apple'ın yalnızca İLK yetkilendirmede e-posta verdiği, sonraki girişlerde
    ///       payload.Email'in null geldiği senaryoda AppleId ile hâlâ doğru kullanıcının
    ///       bulunduğunu ve mevcut e-postanın ÜZERİNE YAZILMADIĞINI doğrulamak.
    /// NEDEN: AppleTokenValidator.cs'in NEDEN açıklamasındaki kritik kısıt — email
    ///        yoksa DB'deki mevcut e-posta korunmalı.
    /// </summary>
    [Fact]
    public async Task LoginWithApple_SubsequentLoginWithoutEmail_MatchesByAppleIdOnly()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "korunan@example.com", IsActive = true };
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-123", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-123", default)).ReturnsAsync(user);
        SetupLoginCompletion();
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithAppleCommand("identity-token"), default);

        // ASSERT — e-posta hiç değişmemeli, GetByEmailAsync hiç çağrılmamalı (email null olduğu için)
        user.Email.Should().Be("korunan@example.com");
        _userRepo.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), default), Times.Never);
    }

    /// <summary>
    /// LoginWithApple_NoEmailAndNoExistingAccount_ThrowsInvalidSocialTokenException
    ///
    /// AMAÇ: Ne AppleId ile eşleşme ne de e-posta (savunmacı/teorik olarak beklenmeyen
    ///       bir durum) varken yeni hesap açılamayacağını, InvalidSocialTokenException
    ///       fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithApple_NoEmailAndNoExistingAccount_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-000", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-000", default)).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithAppleCommand("identity-token"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }
}
