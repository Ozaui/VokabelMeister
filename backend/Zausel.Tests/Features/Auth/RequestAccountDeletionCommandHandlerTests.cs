using Moq;
using FluentAssertions;
using Zausel.Application.Features.Auth;
using Zausel.Application.Interfaces.Repositories.Auth;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Auth;
using Zausel.Domain.Enums.Auth;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Auth;

public class RequestAccountDeletionCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RequestAccountDeletionCommandHandler CreateHandler() =>
        new(_userRepository.Object, _otpService.Object, _emailService.Object);

    [Fact]
    public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new RequestAccountDeletionCommand(99, "tr"), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidUser_GeneratesOtpAndSendsDeletionConfirmationEmail()
    {
        // ARRANGE
        var user = new User { Id = 1, Email = "ada@test.de", FirstName = "Ada" };
        _userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Generate(user, OtpPurpose.AccountDeletion)).Returns("777666");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new RequestAccountDeletionCommand(1, "tr"), default);

        // ASSERT
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailService.Verify(e => e.SendAccountDeletionConfirmationAsync("ada@test.de", "Ada", "777666", "tr", It.IsAny<CancellationToken>()), Times.Once);
    }
}
