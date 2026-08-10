using FluentAssertions;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Tests.Services;

public class OtpServiceTests
{
    private readonly OtpService _service = new(new PasswordService());

    private static User CreateUser() => new() { Id = 1, Email = "test@example.com" };

    [Fact]
    public void Generate_ValidPurpose_SetsPendingFieldsAndReturnsSixDigitCode()
    {
        // ARRANGE
        var user = CreateUser();

        // ACT
        var code = _service.Generate(user, OtpPurpose.LoginOtp);

        // ASSERT — kod 6 haneli, User'ın Pending alanları set edilmiş (hash — ham kod DB'ye YAZILMAZ)
        code.Should().MatchRegex("^[0-9]{6}$");
        user.PendingOtpCodeHash.Should().NotBeNullOrEmpty().And.NotBe(code);
        user.PendingOtpCodePurpose.Should().Be(OtpPurpose.LoginOtp);
        user.PendingOtpCodeExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));
        user.PendingOtpCodeAttempts.Should().Be(0);
    }

    [Fact]
    public void Verify_CorrectCode_ReturnsSuccessAndClearsPendingFields()
    {
        // ARRANGE
        var user = CreateUser();
        var code = _service.Generate(user, OtpPurpose.LoginOtp);

        // ACT
        var result = _service.Verify(user, code, OtpPurpose.LoginOtp);

        // ASSERT — doğrulama sonrası kod tekrar kullanılamasın diye alanlar temizlenir
        result.Should().Be(OtpVerificationResult.Success);
        user.PendingOtpCodeHash.Should().BeNull();
        user.PendingOtpCodePurpose.Should().BeNull();
    }

    [Fact]
    public void Verify_WrongCode_ReturnsInvalidCodeAndIncrementsAttempts()
    {
        // ARRANGE
        var user = CreateUser();
        _service.Generate(user, OtpPurpose.LoginOtp);

        // ACT
        var result = _service.Verify(user, "000000", OtpPurpose.LoginOtp);

        // ASSERT — kod henüz temizlenmedi (1. yanlış deneme), tekrar denemeye izin var
        result.Should().Be(OtpVerificationResult.InvalidCode);
        user.PendingOtpCodeAttempts.Should().Be(1);
        user.PendingOtpCodeHash.Should().NotBeNull();
    }

    [Fact]
    public void Verify_ThirdWrongAttempt_ClearsPendingOtpSoCorrectCodeNoLongerWorks()
    {
        // ARRANGE — SECURITY.md §1: 3 yanlış → kod geçersiz
        var user = CreateUser();
        var code = _service.Generate(user, OtpPurpose.LoginOtp);
        _service.Verify(user, "000000", OtpPurpose.LoginOtp);
        _service.Verify(user, "000000", OtpPurpose.LoginOtp);

        // ACT — 3. yanlış deneme
        var thirdAttemptResult = _service.Verify(user, "000000", OtpPurpose.LoginOtp);
        // artık doğru kodu girse bile (kod DB'de temizlendiği için) başarısız olmalı
        var correctCodeAfterLockout = _service.Verify(user, code, OtpPurpose.LoginOtp);

        // ASSERT
        thirdAttemptResult.Should().Be(OtpVerificationResult.InvalidCode);
        correctCodeAfterLockout.Should().Be(OtpVerificationResult.InvalidCode);
        user.PendingOtpCodeAttempts.Should().Be(0);
        user.PendingOtpCodeHash.Should().BeNull();
    }

    [Fact]
    public void Verify_ExpiredCode_ReturnsExpiredAndClearsPendingFields()
    {
        // ARRANGE
        var user = CreateUser();
        var code = _service.Generate(user, OtpPurpose.LoginOtp);
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // ACT
        var result = _service.Verify(user, code, OtpPurpose.LoginOtp);

        // ASSERT
        result.Should().Be(OtpVerificationResult.Expired);
        user.PendingOtpCodeHash.Should().BeNull();
    }

    [Fact]
    public void Verify_MismatchedPurpose_ReturnsInvalidCode()
    {
        // ARRANGE — PasswordReset için üretilen kod, LoginOtp akışında kullanılamaz
        var user = CreateUser();
        var code = _service.Generate(user, OtpPurpose.PasswordReset);

        // ACT
        var result = _service.Verify(user, code, OtpPurpose.LoginOtp);

        // ASSERT
        result.Should().Be(OtpVerificationResult.InvalidCode);
    }

    [Fact]
    public void Verify_NoPendingCode_ReturnsInvalidCode()
    {
        // ARRANGE
        var user = CreateUser();

        // ACT
        var result = _service.Verify(user, "123456", OtpPurpose.LoginOtp);

        // ASSERT
        result.Should().Be(OtpVerificationResult.InvalidCode);
    }
}
