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
