using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Smtp;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Tests.Features.Smtp;

public class TestSmtpSettingsCommandHandlerTests
{
    private readonly Mock<ISmtpSettingsRepository> _smtpSettingsRepo = new();
    private readonly Mock<IEncryptionService> _encryptionService = new();
    private readonly Mock<ISmtpTestService> _smtpTestService = new();

    private TestSmtpSettingsCommandHandler CreateHandler() =>
        new(_smtpSettingsRepo.Object, _encryptionService.Object, _smtpTestService.Object);

    [Fact]
    public async Task Handle_NoSettingsSaved_ThrowsSmtpSettingsNotConfiguredException()
    {
        // ARRANGE
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync((SmtpSettings?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new TestSmtpSettingsCommand { ToEmail = "admin@example.com" }, default);

        // ASSERT
        await act.Should().ThrowAsync<SmtpSettingsNotConfiguredException>();
        _smtpTestService.Verify(
            s => s.SendTestEmailAsync(It.IsAny<SmtpSettings>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_SettingsSaved_DecryptsPasswordAndSendsTestEmail()
    {
        // ARRANGE
        var settings = new SmtpSettings { Id = 1, PasswordEncrypted = "sifreli-deger" };
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(settings);
        _encryptionService.Setup(e => e.Decrypt("sifreli-deger")).Returns("duz-sifre");
        var handler = CreateHandler();

        // ACT
        var sonuc = await handler.Handle(new TestSmtpSettingsCommand { ToEmail = "admin@example.com" }, default);

        // ASSERT
        _smtpTestService.Verify(
            s => s.SendTestEmailAsync(settings, "duz-sifre", "admin@example.com", default),
            Times.Once
        );
        sonuc.Code.Should().Be("SMTP_TEST_EMAIL_SENT");
    }
}
