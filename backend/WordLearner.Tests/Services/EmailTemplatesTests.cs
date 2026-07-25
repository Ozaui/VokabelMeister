using FluentAssertions;
using WordLearner.Application.Common.Localization;

namespace WordLearner.Tests.Services;

public class EmailTemplatesTests
{
    [Fact]
    public void Resolve_TurkishLanguage_ReturnsTurkishSubject()
    {
        // ACT
        var content = EmailTemplates.Resolve("EMAIL_VERIFICATION", "tr", "123456", 5);

        // ASSERT
        content.Subject.Should().Contain("Doğrulama");
    }

    [Fact]
    public void Resolve_GermanLanguage_ReturnsGermanSubject()
    {
        // ACT
        var content = EmailTemplates.Resolve("EMAIL_VERIFICATION", "de", "123456", 5);

        // ASSERT
        content.Subject.Should().Contain("Bestätigung");
    }

    [Fact]
    public void Resolve_UnsupportedLanguage_FallsBackToTurkish()
    {
        // ACT
        var content = EmailTemplates.Resolve("LOGIN_OTP", "fr", "123456", 5);

        // ASSERT
        content.Should().BeEquivalentTo(EmailTemplates.Resolve("LOGIN_OTP", "tr", "123456", 5));
    }

    [Fact]
    public void Resolve_NullLanguage_FallsBackToTurkish()
    {
        // ACT
        var content = EmailTemplates.Resolve("PASSWORD_RESET", null, "123456", 5);

        // ASSERT
        content.Should().BeEquivalentTo(EmailTemplates.Resolve("PASSWORD_RESET", "tr", "123456", 5));
    }

    [Fact]
    public void Resolve_OtpTemplate_SubstitutesCodeAndExpiryIntoBody()
    {
        // ACT
        var content = EmailTemplates.Resolve("ACCOUNT_DELETION", "tr", "987654", 15);

        // ASSERT — yer tutucu kalırsa kullanıcı gövdede ham "{0}" görürdü.
        content.HtmlBody.Should().Contain("987654").And.Contain("15 dakika");
        content.HtmlBody.Should().NotContain("{0}").And.NotContain("{1}");
    }

    [Fact]
    public void Resolve_InformationalTemplate_ReturnsBodyWithoutPlaceholders()
    {
        // ACT
        var content = EmailTemplates.Resolve("PASSWORD_CHANGED", "de");

        // ASSERT
        content.HtmlBody.Should().NotContain("{0}");
        content.HtmlBody.Should().Contain("Passwort");
    }

    [Fact]
    public void Resolve_UnknownCode_ThrowsArgumentException()
    {
        // ACT — bilinmeyen kod, ErrorMessages'ın aksine sessizce koda düşmez: bir e-posta
        // gövdesinin yerine "SOME_CODE" göndermek kullanıcıya bozuk bir e-posta yollamak olurdu.
        var act = () => EmailTemplates.Resolve("NO_SUCH_TEMPLATE", "tr");

        // ASSERT
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("EMAIL_VERIFICATION")]
    [InlineData("LOGIN_OTP")]
    [InlineData("PASSWORD_RESET")]
    [InlineData("ACCOUNT_DELETION")]
    [InlineData("PASSWORD_CHANGED")]
    [InlineData("ACCOUNT_RECOVERED")]
    public void Resolve_EveryTemplate_HasBothTurkishAndGermanTranslations(string code)
    {
        // ACT — eksik bir çeviri sessizce tr'ye düşerdi, bu test o sessizliği bozar.
        var turkish = EmailTemplates.Resolve(code, "tr");
        var german = EmailTemplates.Resolve(code, "de");

        // ASSERT
        german.Subject.Should().NotBe(turkish.Subject);
        german.HtmlBody.Should().NotBe(turkish.HtmlBody);
    }
}
