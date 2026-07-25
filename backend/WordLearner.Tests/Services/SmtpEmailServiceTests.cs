using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities.System;

namespace WordLearner.Tests.Services;

// Gerçek bir SMTP sunucusuna bağlanılmaz — testler "ayarlar okunamadığında ne olur" sözleşmesini
// doğrular; kritik/bilgilendirme ayrımı tam olarak burada görünür hâle gelir.
public class SmtpEmailServiceTests
{
    private readonly Mock<ISmtpSettingsRepository> _smtpSettingsRepository = new();
    private readonly Mock<IEncryptionService> _encryptionService = new();

    private SmtpEmailService CreateService() =>
        new(
            _smtpSettingsRepository.Object,
            _encryptionService.Object,
            NullLogger<SmtpEmailService>.Instance
        );

    [Fact]
    public async Task SendEmailVerificationOtpAsync_NoSmtpSettings_ThrowsEmailSendFailedException()
    {
        // ARRANGE
        _smtpSettingsRepository
            .Setup(r => r.GetCurrentAsync(default))
            .ReturnsAsync((SmtpSettings?)null);
        var service = CreateService();

        // ACT
        var act = () => service.SendEmailVerificationOtpAsync("test@example.com", "123456", "tr");

        // ASSERT — "önce SMTP ayarlarını kaydedin" admin'e ait bir cümle, kayıt olan kullanıcıya değil.
        var exception = await act.Should().ThrowAsync<EmailSendFailedException>();
        exception.Which.Code.Should().Be("EMAIL_SEND_FAILED");
    }

    [Fact]
    public async Task SendLoginOtpAsync_NoSmtpSettings_ThrowsEmailSendFailedException()
    {
        // ARRANGE
        _smtpSettingsRepository
            .Setup(r => r.GetCurrentAsync(default))
            .ReturnsAsync((SmtpSettings?)null);
        var service = CreateService();

        // ACT
        var act = () => service.SendLoginOtpAsync("test@example.com", "123456", "de");

        // ASSERT
        await act.Should().ThrowAsync<EmailSendFailedException>();
    }

    [Fact]
    public async Task SendPasswordChangedNotificationAsync_NoSmtpSettings_DoesNotThrow()
    {
        // ARRANGE
        _smtpSettingsRepository
            .Setup(r => r.GetCurrentAsync(default))
            .ReturnsAsync((SmtpSettings?)null);
        var service = CreateService();

        // ACT
        var act = () => service.SendPasswordChangedNotificationAsync("test@example.com", "tr");

        // ASSERT — şifre zaten değişti; e-posta gönderilemedi diye o işlem geri alınamaz.
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAccountRecoveredNotificationAsync_NoSmtpSettings_DoesNotThrow()
    {
        // ARRANGE
        _smtpSettingsRepository
            .Setup(r => r.GetCurrentAsync(default))
            .ReturnsAsync((SmtpSettings?)null);
        var service = CreateService();

        // ACT
        var act = () => service.SendAccountRecoveredNotificationAsync("test@example.com", "de");

        // ASSERT
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmailVerificationOtpAsync_DecryptionFails_ThrowsEmailSendFailedException()
    {
        // ARRANGE
        _smtpSettingsRepository
            .Setup(r => r.GetCurrentAsync(default))
            .ReturnsAsync(
                new SmtpSettings
                {
                    Host = "smtp.example.com",
                    Port = 587,
                    Username = "user",
                    PasswordEncrypted = "bozuk-veri",
                    FromEmail = "no-reply@example.com",
                    FromName = "VokabelMeister",
                }
            );
        _encryptionService
            .Setup(e => e.Decrypt(It.IsAny<string>()))
            .Throws(new FormatException("invalid base64"));
        var service = CreateService();

        // ACT
        var act = () => service.SendEmailVerificationOtpAsync("test@example.com", "123456", "tr");

        // ASSERT — ham FormatException 500'e düşerdi; sözleşme 503 + EMAIL_SEND_FAILED.
        await act.Should().ThrowAsync<EmailSendFailedException>();
    }
}
