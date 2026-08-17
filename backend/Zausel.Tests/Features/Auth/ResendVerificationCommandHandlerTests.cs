using Moq;
using FluentAssertions;
using Zausel.Application.Features.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Tests.Features.Auth;

public class ResendVerificationCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ResendVerificationCommandHandler CreateHandler() =>
        new(_userRepository.Object, _otpService.Object, _emailService.Object);

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnitWithoutSendingEmail()
    {
        // ARRANGE — enumeration önleme: kayıtsız e-postada da sessizce 200
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResendVerificationCommand("yok@test.de", "tr"), default);

        // ASSERT
        _emailService.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyVerified_ReturnsUnitWithoutSendingEmail()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", IsEmailVerified = true };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResendVerificationCommand("ada@test.de", "tr"), default);

        // ASSERT
        _emailService.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnverifiedUser_GeneratesOtpAndSendsEmail()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", FirstName = "Ada", IsEmailVerified = false };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate(user, OtpPurpose.EmailVerification)).Returns("654321");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ResendVerificationCommand("ada@test.de", "de"), default);

        // ASSERT
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationAsync("ada@test.de", "Ada", "654321", "de", It.IsAny<CancellationToken>()), Times.Once);
    }
}
