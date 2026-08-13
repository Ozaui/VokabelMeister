using Moq;
using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories.Auth;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Features.Auth;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IOtpService> _otpService = new();

    private VerifyEmailCommandHandler CreateHandler() => new(_userRepository.Object, _otpService.Object);

    [Fact]
    public async Task Handle_EmailNotFound_ThrowsInvalidOtpException()
    {
        // ARRANGE — hangi e-postanın kayıtlı olduğunu sızdırmamak için VerifyEmail ile aynı hata
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyEmailCommand("yok@test.de", "123456"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_OtpExpired_ThrowsOtpExpiredException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.EmailVerification)).Returns(OtpVerificationResult.Expired);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyEmailCommand("ada@test.de", "123456"), default);

        // ASSERT
        await act.Should().ThrowAsync<OtpExpiredException>();
    }

    [Fact]
    public async Task Handle_InvalidCode_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de" };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "000000", OtpPurpose.EmailVerification)).Returns(OtpVerificationResult.InvalidCode);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new VerifyEmailCommand("ada@test.de", "000000"), default);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    [Fact]
    public async Task Handle_ValidCode_MarksEmailVerifiedAndSaves()
    {
        // ARRANGE
        var user = new User { Email = "ada@test.de", IsEmailVerified = false };
        _userRepository.Setup(r => r.GetByEmailAsync("ada@test.de", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _otpService.Setup(o => o.Verify(user, "123456", OtpPurpose.EmailVerification)).Returns(OtpVerificationResult.Success);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new VerifyEmailCommand("ada@test.de", "123456"), default);

        // ASSERT
        user.IsEmailVerified.Should().BeTrue();
        user.EmailVerifiedAt.Should().NotBeNull();
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
