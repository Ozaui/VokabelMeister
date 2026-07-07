// ─────────────────────────────────────────────────────────────────────────────
// OtpServiceTests.cs
//
// AMAÇ: OtpService'in OTP üretimi/doğrulanması/temizlenmesi mantığını doğrulamak
//       (amaç/hash/süre kontrolleri dahil).
// NEDEN: Bu mantık eskiden AuthServiceTests içinde her akış (VerifyEmail/VerifyLoginOtp/
//        ResetPassword/ConfirmAccountDeletion) için ayrı ayrı tekrar test ediliyordu;
//        MediatR CQRS'e geçişte paylaşılan bir servise çıkarıldığı için (bkz.
//        Application/Services/OtpService.cs) artık TEK bir yerde, kapsamlı test edilir.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

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

    /// <summary>
    /// Generate_AlwaysReturnsSixDigitCodeAndItsHash
    ///
    /// AMAÇ: Generate'in her zaman 6 haneli bir kod ve bu kodun (IPasswordService.HashToken
    ///       ile üretilen) hash'ini döndürdüğünü doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Validate_MatchingHashPurposeAndUnexpired_DoesNotThrow
    ///
    /// AMAÇ: Doğru hash + doğru amaç + süresi dolmamış bir OTP için Validate'in
    ///       herhangi bir istisna fırlatmadığını doğrulamak (mutlu yol).
    /// </summary>
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

    /// <summary>
    /// Validate_NullUser_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Kullanıcı bulunamadığında (null) da InvalidOtpException fırlatıldığını
    ///       doğrulamak — "kullanıcı yok" ile "OTP yanlış" arasında ayrım yapılmaz
    ///       (bilgi sızıntısı önlemi, ResendVerification/ForgotPassword ile aynı desen).
    /// </summary>
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

    /// <summary>
    /// Validate_WrongHash_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Girilen kodun hash'i beklenenle eşleşmediğinde InvalidOtpException
    ///       fırlatıldığını doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Validate_WrongPurpose_ThrowsInvalidOtpException
    ///
    /// AMAÇ: OTP'nin ait olduğu amaç (ör. LoginOtp) beklenen amaçtan (ör. PasswordReset)
    ///       farklıysa InvalidOtpException fırlatıldığını doğrulamak — bir akış için
    ///       üretilen OTP başka bir akışta kullanılamaz.
    /// </summary>
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

    /// <summary>
    /// Validate_ExpiredOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Süresi dolmuş bir OTP için InvalidOtpException fırlatıldığını doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Clear_PendingOtp_NullsAllPendingOtpFields
    ///
    /// AMAÇ: Clear çağrıldığında bekleyen OTP'nin hash/süre/amaç alanlarının hepsinin
    ///       null'landığını doğrulamak.
    /// </summary>
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
