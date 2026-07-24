using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RegisterCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _emailService.Object,
            AuthTestMapper.Create()
        );

    [Fact]
    public async Task Register_NewEmail_CreatesUserAndSendsVerificationOtp()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("new@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _passwordService.Setup(p => p.Hash("Deneme123!@#")).Returns("hashed-password");
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        var handler = CreateHandler();
        var command = new RegisterCommand("new@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var sonuc = await handler.Handle(command, default);

        // ASSERT
        sonuc.Email.Should().Be("new@example.com");
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "hashed-password"), null, default), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync("new@example.com", "123456", default), Times.Once);
    }

    [Fact]
    public async Task Register_NewEmail_ReturnsDefaultSystemThemePreference()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("tema@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _passwordService.Setup(p => p.Hash("Deneme123!@#")).Returns("hashed-password");
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        var handler = CreateHandler();
        var command = new RegisterCommand("tema@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var sonuc = await handler.Handle(command, default);

        // ASSERT
        sonuc.ThemePreference.Should().Be("System");
    }

    [Fact]
    public async Task Register_EmailAlreadyRegistered_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo
            .Setup(r => r.GetByEmailAsync("var@example.com", default))
            .ReturnsAsync(new User { Id = 1, Email = "var@example.com", IsActive = true });
        var handler = CreateHandler();
        var command = new RegisterCommand("var@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task Register_EmailPreviouslyAnonymized_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("eski@example.com", default)).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.HashToken("eski@example.com")).Returns("email-hash");
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync("email-hash", default)).ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new RegisterCommand("eski@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }
}
