// ─────────────────────────────────────────────────────────────────────────────
// RegisterCommandHandlerTests.cs
//
// AMAÇ: RegisterCommandHandler'ın (kayıt oluşturma + doğrulama OTP'si gönderme)
//       mutlu yol ve e-posta çakışması senaryolarını doğrulamak.
// NEDEN: Bu servis projenin en güvenlik-kritik parçasıdır — repository/dış servisler
//        her zaman mock'lanır (CODING_STANDARDS.md §7.4), gerçek DB/HTTP çağrısı yapılmaz.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions, AutoMapper (AuthProfile),
//                WordLearner.Application.Features.Auth.RegisterCommand.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;
using WordLearner.Tests.Common;

namespace WordLearner.Tests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IOtpService> _otpService = new();
    private readonly Mock<IEmailService> _emailService = new();

    private RegisterCommandHandler CreateHandler() =>
        new(
            _userRepo.Object,
            _passwordService.Object,
            _otpService.Object,
            _emailService.Object,
            AuthTestMapper.Create()
        );

    /// <summary>
    /// Register_NewEmail_CreatesUserAndSendsVerificationOtp
    ///
    /// AMAÇ: Daha önce kayıtlı olmayan bir e-postayla kayıt olunduğunda kullanıcının
    ///       oluşturulduğunu ve doğrulama e-postasının gönderildiğini doğrulamak.
    /// NEDEN: Register akışının mutlu yolu — şifre hash'lenmeli, OTP üretilmeli, e-posta atılmalı.
    /// </summary>
    [Fact]
    public async Task Register_NewEmail_CreatesUserAndSendsVerificationOtp()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("new@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _passwordService.Setup(p => p.Hash("Deneme123!@#")).Returns("hashed-password");
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        var handler = CreateHandler();
        var command = new RegisterCommand("new@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var sonuc = await handler.Handle(command, default);

        // ASSERT
        sonuc.Email.Should().Be("new@example.com");
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "hashed-password"), null, default), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync("new@example.com", "123456", default), Times.Once);
    }

    /// <summary>
    /// Register_NewEmail_ReturnsDefaultSystemThemePreference
    ///
    /// AMAÇ: RegisterCommand'ın ThemePreference için bir girdi almadığını, response'ta
    ///       DB varsayılanı olan "System"in döndüğünü doğrulamak.
    /// NEDEN: ThemePreference, CurrentLevel ile aynı deseni takip eder — kayıt anonim
    ///        olduğu için (henüz JWT yok) bu alan register'da toplanmaz, gerçek seçim
    ///        ilk-login-sonrası onboarding'de (PUT /users/me, C-01) yapılır.
    /// </summary>
    [Fact]
    public async Task Register_NewEmail_ReturnsDefaultSystemThemePreference()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("tema@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _passwordService.Setup(p => p.Hash("Deneme123!@#")).Returns("hashed-password");
        _otpService.Setup(o => o.Generate()).Returns(("123456", "otp-hash"));
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        var handler = CreateHandler();
        var command = new RegisterCommand("tema@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var sonuc = await handler.Handle(command, default);

        // ASSERT
        sonuc.ThemePreference.Should().Be("System");
    }

    /// <summary>
    /// Register_EmailAlreadyRegistered_ThrowsDuplicateEmailException
    ///
    /// AMAÇ: Aktif bir kullanıcının e-postasıyla tekrar kayıt denendiğinde
    ///       DuplicateEmailException fırlatıldığını doğrulamak.
    /// NEDEN: E-posta benzersizliği DB constraint'i değil, servis katmanında zorlanır.
    /// </summary>
    [Fact]
    public async Task Register_EmailAlreadyRegistered_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo
            .Setup(r => r.GetByEmailAsync("var@example.com", default))
            .ReturnsAsync(new User { Id = 1, Email = "var@example.com", IsActive = true });
        var handler = CreateHandler();
        var command = new RegisterCommand("var@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    /// <summary>
    /// Register_EmailPreviouslyAnonymized_ThrowsDuplicateEmailException
    ///
    /// AMAÇ: Daha önce anonimleştirilmiş bir hesabın orijinal e-postasıyla kayıt
    ///       denendiğinde de DuplicateEmailException fırlatıldığını doğrulamak.
    /// NEDEN: REFERENCE/SECURITY.md §9 — anonimleştirilmiş bir e-posta tekrar kullanılamaz;
    ///        bu kontrol GetByEmailAsync'in bulamadığı (anonimleştirme sonrası e-posta
    ///        değiştiği için) durumları OriginalEmailHash üzerinden yakalar.
    /// </summary>
    [Fact]
    public async Task Register_EmailPreviouslyAnonymized_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("eski@example.com", default)).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.HashToken("eski@example.com")).Returns("email-hash");
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync("email-hash", default)).ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new RegisterCommand("eski@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }
}
