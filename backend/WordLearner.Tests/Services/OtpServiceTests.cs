using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Services;

public class OtpServiceTests
{
    private readonly Mock<IPasswordService> _passwordService = new();

    private OtpService CreateService() => new(_passwordService.Object);

    private static User CreateUserWithPendingOtp(
        string otpHash = "otp-hash",
        OtpPurpose purpose = OtpPurpose.EmailVerification,
        DateTime? expiresAt = null
    ) =>
        new()
        {
            Email = "test@example.com",
            PendingOtpCodeHash = otpHash,
            PendingOtpCodeExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(5),
            PendingOtpCodePurpose = purpose,
        };

    [Fact]
    public void Generate_AlwaysReturnsSixDigitCodeAndItsHash()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns<string>(code => $"hash-of-{code}");
        var service = CreateService();

        // ACT
        var (code, hash) = service.Generate();

        // ASSERT
        code.Should().MatchRegex(@"^\d{6}$");
        hash.Should().Be($"hash-of-{code}");
    }

    [Fact]
    public void Validate_MatchingHashPurposeAndUnexpired_DoesNotThrow()
    {
        // ARRANGE
        var user = CreateUserWithPendingOtp();
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        var service = CreateService();

        // ACT
        var act = () => service.Validate(user, "123456", OtpPurpose.EmailVerification);

        // ASSERT
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NullUser_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var service = CreateService();

        // ACT
        var act = () => service.Validate(null, "123456", OtpPurpose.EmailVerification);

        // ASSERT
        act.Should().Throw<InvalidOtpException>();
    }

    [Fact]
    public void Validate_WrongHash_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateUserWithPendingOtp(otpHash: "dogru-hash");
        _passwordService.Setup(p => p.HashToken("999999")).Returns("yanlis-hash");
        var service = CreateService();

        // ACT
        var act = () => service.Validate(user, "999999", OtpPurpose.EmailVerification);

        // ASSERT
        act.Should().Throw<InvalidOtpException>();
    }

    [Fact]
    public void Validate_WrongPurpose_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateUserWithPendingOtp(purpose: OtpPurpose.LoginOtp);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        var service = CreateService();

        // ACT
        var act = () => service.Validate(user, "123456", OtpPurpose.PasswordReset);

        // ASSERT
        act.Should().Throw<InvalidOtpException>();
    }

    [Fact]
    public void Validate_ExpiredOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateUserWithPendingOtp(expiresAt: DateTime.UtcNow.AddMinutes(-1));
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        var service = CreateService();

        // ACT
        var act = () => service.Validate(user, "123456", OtpPurpose.EmailVerification);

        // ASSERT
        act.Should().Throw<InvalidOtpException>();
    }

    [Fact]
    public void Clear_PendingOtp_NullsAllPendingOtpFields()
    {
        // ARRANGE
        var user = CreateUserWithPendingOtp();
        var service = CreateService();

        // ACT
        service.Clear(user);

        // ASSERT
        user.PendingOtpCodeHash.Should().BeNull();
        user.PendingOtpCodeExpiresAt.Should().BeNull();
        user.PendingOtpCodePurpose.Should().BeNull();
    }
}
