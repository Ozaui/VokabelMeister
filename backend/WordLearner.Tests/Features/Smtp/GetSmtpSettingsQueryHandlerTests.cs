// ─────────────────────────────────────────────────────────────────────────────
// GetSmtpSettingsQueryHandlerTests.cs
//
// AMAÇ: GetSmtpSettingsQueryHandler'ın kayıt yokken boş varsayılanlar, kayıt
//       varken şifreyi maskeleyerek döndürdüğünü doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Features.Smtp;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Tests.Features.Smtp;

public class GetSmtpSettingsQueryHandlerTests
{
    private readonly Mock<ISmtpSettingsRepository> _smtpSettingsRepo = new();

    private GetSmtpSettingsQueryHandler CreateHandler() => new(_smtpSettingsRepo.Object);

    /// <summary>
    /// Handle_NoSettingsSaved_ReturnsEmptyDefaults
    ///
    /// AMAÇ: Hiç ayar kaydedilmemişken 404 FIRLATILMADIĞINI, boş/varsayılan
    ///       alanlarla bir DTO döndüğünü doğrulamak (admin panel boş bir form gösterir).
    /// </summary>
    [Fact]
    public async Task Handle_NoSettingsSaved_ReturnsEmptyDefaults()
    {
        // ARRANGE
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync((SmtpSettings?)null);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new GetSmtpSettingsQuery(), default);

        // ASSERT
        sonuc.Host.Should().BeEmpty();
        sonuc.Port.Should().Be(587);
        sonuc.EnableSsl.Should().BeTrue();
        sonuc.Password.Should().BeEmpty();
    }

    /// <summary>
    /// Handle_SettingsSaved_MasksPassword
    ///
    /// AMAÇ: Kayıtlı ayarlar varsa gerçek şifrenin (PasswordEncrypted) İSTEMCİYE
    ///       ASLA dönmediğini, sabit "***" maskesiyle değiştirildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Handle_SettingsSaved_MasksPassword()
    {
        // ARRANGE
        var settings = new SmtpSettings
        {
            Id = 1,
            Host = "smtp.example.com",
            Port = 465,
            EnableSsl = true,
            Username = "noreply@example.com",
            PasswordEncrypted = "cok-gizli-sifreli-deger",
            FromEmail = "noreply@example.com",
            FromName = "VokabelMeister",
        };
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new GetSmtpSettingsQuery(), default);

        // ASSERT
        sonuc.Host.Should().Be("smtp.example.com");
        sonuc.Password.Should().Be("***");
        sonuc.Password.Should().NotBe(settings.PasswordEncrypted);
    }
}
