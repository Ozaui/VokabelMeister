// ─────────────────────────────────────────────────────────────────────────────
// UpdateSmtpSettingsCommandHandlerTests.cs
//
// AMAÇ: UpdateSmtpSettingsCommandHandler'ın (1) kayıt yokken yeni satır oluşturup
//       şifreyi şifrelediğini, (2) kayıt varken "***" gönderildiğinde eski şifreli
//       değeri KORUDUĞUNU, (3) gerçek yeni bir şifre gönderildiğinde YENİDEN
//       şifrelediğini, (4) hem IActivityLogger hem ISecurityLogger'ı çağırdığını ve
//       (5) kayıt YOKKEN maske literal'i gönderilirse SmtpPasswordRequiredException
//       fırlattığını (kod denetiminde bulunan bir açık tasarım sorusu) doğrulamak —
//       CLAUDE.md "admin'e özel hassas işlem" kuralı (UpdateUserRoleCommand ile aynı
//       çift-loglama deseni).
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Smtp;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.System;
using WordLearner.Domain.Enums.Logging;

namespace WordLearner.Tests.Features.Smtp;

public class UpdateSmtpSettingsCommandHandlerTests
{
    private readonly Mock<ISmtpSettingsRepository> _smtpSettingsRepo = new();
    private readonly Mock<IEncryptionService> _encryptionService = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly Mock<ISecurityLogger> _securityLogger = new();

    private UpdateSmtpSettingsCommandHandler CreateHandler() =>
        new(_smtpSettingsRepo.Object, _encryptionService.Object, _activityLogger.Object, _securityLogger.Object);

    private static UpdateSmtpSettingsCommand CreateCommand(string password = "YeniSifre123!") =>
        new("smtp.example.com", 587, true, "noreply@example.com", password, "noreply@example.com", "VokabelMeister")
        {
            UserId = 1,
            ActorRole = "Admin",
            IpAddress = "1.2.3.4",
        };

    /// <summary>
    /// Handle_NoExistingSettings_CreatesNewRowWithEncryptedPassword
    /// </summary>
    [Fact]
    public async Task Handle_NoExistingSettings_CreatesNewRowWithEncryptedPassword()
    {
        // ARRANGE — hiç kayıt yok, Encrypt sabit bir değer döner
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync((SmtpSettings?)null);
        _encryptionService.Setup(e => e.Encrypt("YeniSifre123!")).Returns("sifreli-deger");
        _smtpSettingsRepo
            .Setup(r => r.AddAsync(It.IsAny<SmtpSettings>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SmtpSettings s, int? _, CancellationToken _) =>
            {
                s.Id = 99;
                return s;
            });
        var handler = CreateHandler();

        // ACT
        await handler.Handle(CreateCommand(), default);

        // ASSERT
        _smtpSettingsRepo.Verify(
            r => r.AddAsync(It.Is<SmtpSettings>(s => s.PasswordEncrypted == "sifreli-deger"), 1, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _smtpSettingsRepo.Verify(r => r.UpdateAsync(It.IsAny<SmtpSettings>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Handle_ExistingSettingsWithMaskedPassword_KeepsOldEncryptedPassword
    ///
    /// AMAÇ: Admin panel şifre alanını DEĞİŞTİRMEDEN geri gönderdiğinde ("***")
    ///       eski PasswordEncrypted'in KORUNDUĞUNU, Encrypt'in HİÇ çağrılmadığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Handle_ExistingSettingsWithMaskedPassword_KeepsOldEncryptedPassword()
    {
        // ARRANGE
        var mevcut = new SmtpSettings { Id = 5, PasswordEncrypted = "eski-sifreli-deger" };
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mevcut);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(CreateCommand(password: "***"), default);

        // ASSERT
        mevcut.PasswordEncrypted.Should().Be("eski-sifreli-deger");
        _encryptionService.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        _smtpSettingsRepo.Verify(r => r.UpdateAsync(mevcut, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Handle_NoExistingSettingsWithMaskedPassword_ThrowsSmtpPasswordRequiredException
    ///
    /// AMAÇ: Kod denetiminde bulunan gerçek bir hatayı regresyona karşı kilitler —
    ///       hiç ayar kaydedilmemişken maske literal'i ("***") gönderilirse, bu literal
    ///       SESSİZCE şifrelenip DB'ye "gerçek şifre" olarak YAZILMAMALI.
    /// </summary>
    [Fact]
    public async Task Handle_NoExistingSettingsWithMaskedPassword_ThrowsSmtpPasswordRequiredException()
    {
        // ARRANGE
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync((SmtpSettings?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(CreateCommand(password: "***"), default);

        // ASSERT
        await act.Should().ThrowAsync<SmtpPasswordRequiredException>();
        _encryptionService.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
        _smtpSettingsRepo.Verify(r => r.AddAsync(It.IsAny<SmtpSettings>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Handle_ExistingSettingsWithNewPassword_ReEncryptsPassword
    /// </summary>
    [Fact]
    public async Task Handle_ExistingSettingsWithNewPassword_ReEncryptsPassword()
    {
        // ARRANGE
        var mevcut = new SmtpSettings { Id = 5, PasswordEncrypted = "eski-sifreli-deger" };
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mevcut);
        _encryptionService.Setup(e => e.Encrypt("YeniSifre123!")).Returns("yeni-sifreli-deger");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(CreateCommand(password: "YeniSifre123!"), default);

        // ASSERT
        mevcut.PasswordEncrypted.Should().Be("yeni-sifreli-deger");
    }

    /// <summary>
    /// Handle_ValidCommand_LogsActivityAndSecurity
    ///
    /// AMAÇ: CLAUDE.md'nin admin'e özel hassas işlem kuralı gereği HEM IActivityLogger
    ///       HEM ISecurityLogger'ın çağrıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_LogsActivityAndSecurity()
    {
        // ARRANGE
        var mevcut = new SmtpSettings { Id = 5, PasswordEncrypted = "eski-sifreli-deger" };
        _smtpSettingsRepo.Setup(r => r.GetCurrentAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mevcut);
        _encryptionService.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("yeni-sifreli-deger");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(CreateCommand(), default);

        // ASSERT
        _activityLogger.Verify(
            l => l.LogAsync(
                1,
                "Admin",
                "UPDATE_SMTP_SETTINGS",
                "SmtpSettings",
                5,
                It.IsAny<object>(),
                It.IsAny<object>(),
                "1.2.3.4",
                null,
                default
            ),
            Times.Once
        );
        _securityLogger.Verify(
            l => l.LogAsync(LogEventType.AdminAction, 1, null, "1.2.3.4", null, "SMTP_SETTINGS_CHANGED", default),
            Times.Once
        );
    }
}
