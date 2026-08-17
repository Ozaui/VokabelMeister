using Moq;
using Zausel.Application.Features.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;

namespace Zausel.Tests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_userRepository.Object, _otpService.Object, _emailService.Object);

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnitWithoutSendingEmail()
    {
        // ARRANGE — SECURITY.md §7: e-posta enumerasyonu önlenir, kayıtsız e-postada da 200
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ForgotPasswordCommand("yok@test.de", "tr"), default);

        // ASSERT
        _emailService.Verify(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingUser_GeneratesOtpAndSendsPasswordResetEmail()
    {
        // ARRANGE — sosyal-yalnızca hesap da (PasswordHash null) OTP ile şifre belirleyebilir
        var user = new User { Email = "ada@test.de", FirstName = "Ada", PasswordHash = null };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate(user, OtpPurpose.PasswordReset)).Returns("999888");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new ForgotPasswordCommand("ada@test.de", "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendPasswordResetAsync("ada@test.de", "Ada", "999888", "tr", It.IsAny<CancellationToken>()), Times.Once);
    }
}
