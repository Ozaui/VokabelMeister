using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class LoginWithGoogleCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IGoogleTokenValidator> _googleTokenValidator = new();
    private readonly Mock<ILoginCompletionService> _loginCompletionService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private LoginWithGoogleCommandHandler CreateHandler() => new(
        _userRepository.Object, _refreshTokenRepository.Object, _googleTokenValidator.Object,
        _loginCompletionService.Object, _emailService.Object);

    private static LoginCompletionResult CreateCompletion(bool recovered = false) =>
        new("access-token", "refresh-token", new RefreshToken { UserId = 1 }, recovered);

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _googleTokenValidator.Setup(v => v.ValidateAsync("kotu-token", It.IsAny<CancellationToken>())).ReturnsAsync((GoogleTokenPayload?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithGoogleCommand("kotu-token", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }

    [Fact]
    public async Task Handle_ExistingGoogleUser_ReturnsLoginResponseWithoutCreatingUser()
    {
        // ARRANGE
        var payload = new GoogleTokenPayload("google-sub-1", "ada@test.de", "Ada", "Lovelace");
        _googleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        var user = new User { Id = 1, Email = "ada@test.de", GoogleId = "google-sub-1", IsActive = true };
        _userRepository.Setup(r => r.GetByGoogleIdAsync("google-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var completion = CreateCompletion();
        _loginCompletionService.Setup(l => l.Complete(user, null, null)).Returns(completion);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new LoginWithGoogleCommand("iyi-token", null, null, "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        result.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Handle_ExistingEmailDifferentProvider_LinksGoogleIdToExistingAccount()
    {
        // ARRANGE — daha önce Local kayıt açılmış aynı e-posta, Google hesabı BİRLEŞTİRİLİR
        var payload = new GoogleTokenPayload("google-sub-1", "ada@test.de", "Ada", "Lovelace");
        _googleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        _userRepository.Setup(r => r.GetByGoogleIdAsync("google-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var existingLocalUser = new User { Id = 5, Email = "ada@test.de", AuthProvider = "Local", IsActive = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(existingLocalUser);
        _loginCompletionService.Setup(l => l.Complete(existingLocalUser, null, null)).Returns(CreateCompletion());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithGoogleCommand("iyi-token", null, null, "tr"), default);

        // ASSERT
        existingLocalUser.GoogleId.Should().Be("google-sub-1");
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoExistingUser_CreatesNewGoogleUser()
    {
        // ARRANGE
        var payload = new GoogleTokenPayload("google-sub-1", "yeni@test.de", "Yeni", "Kullanici");
        _googleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        _userRepository.Setup(r => r.GetByGoogleIdAsync("google-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _userRepository.Setup(r => r.GetByEmailAsync("yeni@test.de", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _loginCompletionService.Setup(l => l.Complete(It.IsAny<User>(), null, null)).Returns(CreateCompletion());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new LoginWithGoogleCommand("iyi-token", null, null, "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "yeni@test.de" && u.GoogleId == "google-sub-1" && u.AuthProvider == "Google" && u.IsEmailVerified),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveAccount_ThrowsAccountInactiveException()
    {
        // ARRANGE
        var payload = new GoogleTokenPayload("google-sub-1", "ada@test.de", "Ada", "Lovelace");
        _googleTokenValidator.Setup(v => v.ValidateAsync("iyi-token", It.IsAny<CancellationToken>())).ReturnsAsync(payload);
        var user = new User { Id = 1, Email = "ada@test.de", GoogleId = "google-sub-1", IsActive = false };
        _userRepository.Setup(r => r.GetByGoogleIdAsync("google-sub-1", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new LoginWithGoogleCommand("iyi-token", null, null, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<AccountInactiveException>();
    }
}
