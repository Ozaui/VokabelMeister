using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RegisterCommandHandler CreateHandler() =>
        new(_userRepository.Object, _passwordService.Object, _otpService.Object, _emailService.Object);

    [Fact]
    public async Task Handle_EmailAlreadyRegistered_ThrowsEmailAlreadyRegisteredException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "ada@test.de" });
        var handler = CreateHandler();
        var command = new RegisterCommand("ada@test.de", "Sifre123!", "Ada", "Lovelace", "tr");

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EmailAlreadyRegisteredException>();
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesUserAndSendsVerificationEmail()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.Hash("Sifre123!")).Returns("hashed-sifre");
        _otpService.Setup(o => o.Generate(It.IsAny<User>(), OtpPurpose.EmailVerification)).Returns("123456");
        var handler = CreateHandler();
        var command = new RegisterCommand("ada@test.de", "Sifre123!", "Ada", "Lovelace", "tr");

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT — DB'ye eklenen User doğru alanlarla kuruldu, OTP e-postayla gönderildi
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "ada@test.de" && u.PasswordHash == "hashed-sifre" && u.FirstName == "Ada" && u.LastName == "Lovelace"),
            It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationAsync("ada@test.de", "Ada", "123456", "tr", It.IsAny<CancellationToken>()), Times.Once);
        result.Email.Should().Be("ada@test.de");
        result.FirstName.Should().Be("Ada");
    }
}
