using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Localization;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Auth;

public class RequestAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RequestAccountDeletionCommandHandler CreateHandler() =>
        new(_userRepo.Object, _otpService.Object, _emailService.Object);

    [Fact]
    public async Task RequestAccountDeletion_ExistingUser_SendsDeletionOtp()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new RequestAccountDeletionCommand(user.Id), default);

        // ASSERT
        _emailService.Verify(e => e.SendAccountDeletionOtpAsync(user.Email, "123456", null, default), Times.Once);
    }

    [Fact]
    public async Task RequestAccountDeletion_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RequestAccountDeletionCommand(999), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task RequestAccountDeletion_GermanLanguage_ReturnsGermanMessage()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "test@example.com" };
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(
            new RequestAccountDeletionCommand(user.Id) { Language = "de" },
            default
        );

        // ASSERT
        sonuc.Code.Should().Be("ACCOUNT_DELETION_OTP_SENT");
        sonuc.Message.Should().Be(SuccessMessages.Resolve("ACCOUNT_DELETION_OTP_SENT", "de"));
    }
}
