using Moq;
using FluentAssertions;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;

namespace Zausel.Tests.Features.Auth;

public class LoginWithAppleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IAppleTokenValidator> _appleTokenValidator = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private LoginWithAppleCommandHandler CreateHandler() => new(
        _userRepository.Object, _refreshTokenRepository.Object, _appleTokenValidator.Object,
        _loginCompletionService.Object, _emailService.Object);

    private static LoginCompletionResult CreateCompletion(bool recovered = false) =>
        new("access-token", "refresh-token", new RefreshToken { UserId = 1 }, recovered);

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _appleTokenValidator.Setup(v => v.ValidateAsync("kotu-token", It.IsAny<CancellationToken>())).ReturnsAsync((AppleTokenPayload?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithAppleCommand("kotu-token", null, null, null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }

    [Fact]
    public async Task Handle_NoEmailInToken_CreatesUserWithPrivateRelayPlaceholderEmail()
    {
        // ARRANGE — token'da e-posta yoksa (nadiren, tekrar giriş) sub tabanlı yer tutucu üretilir
        var payload = new AppleTokenPayload("apple-sub-1", Email: null);
        _appleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        _userRepository.Setup(r => r.GetByAppleIdAsync("apple-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _loginCompletionService.Setup(l => l.Complete(It.IsAny<User>(), null, null)).Returns(CreateCompletion());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithAppleCommand("iyi-token", "Ada", "Lovelace", null, null, "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "apple-sub-1@privaterelay.appleid.com" && u.AppleId == "apple-sub-1" && u.FirstName == "Ada"),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingEmailDifferentProvider_LinksAppleIdToExistingAccount()
    {
        // ARRANGE
        var payload = new AppleTokenPayload("apple-sub-1", "ada@test.de");
        _appleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        _userRepository.Setup(r => r.GetByAppleIdAsync("apple-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var existingUser = new User { Id = 5, Email = "ada@test.de", AuthProvider = "Local", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);
        _loginCompletionService.Setup(l => l.Complete(existingUser, null, null)).Returns(CreateCompletion());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithAppleCommand("iyi-token", null, null, null, null, "tr"), default);

        // ASSERT
        existingUser.AppleId.Should().Be("apple-sub-1");
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveAccount_ThrowsAccountInactiveException()
    {
        // ARRANGE
        var payload = new AppleTokenPayload("apple-sub-1", "ada@test.de");
        _appleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        var user = new User { Id = 1, Email = "ada@test.de", AppleId = "apple-sub-1", IsActive = false };
        _userRepository.Setup(r => r.GetByAppleIdAsync("apple-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithAppleCommand("iyi-token", null, null, null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountInactiveException>();
    }

    [Fact]
    public async Task Handle_AccountWasRecovered_SendsAccountRecoveredNotification()
    {
        // ARRANGE
        var payload = new AppleTokenPayload("apple-sub-1", "ada@test.de");
        _appleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        var user = new User { Id = 1, Email = "ada@test.de", FirstName = "Ada", AppleId = "apple-sub-1", IsActive = true };
        _userRepository.Setup(r => r.GetByAppleIdAsync("apple-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _loginCompletionService.Setup(l => l.Complete(user, null, null)).Returns(CreateCompletion(recovered: true));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithAppleCommand("iyi-token", null, null, null, null, "de"), default);

        // ASSERT
        _emailService.Verify(e => e.SendAccountRecoveredNotificationAsync("ada@test.de", "Ada", "de", It.IsAny<CancellationToken>()), Times.Once);
    }
}
