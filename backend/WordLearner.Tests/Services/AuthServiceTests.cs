// ─────────────────────────────────────────────────────────────────────────────
// AuthServiceTests.cs
//
// AMAÇ: AuthService'in 13 akışını (register, e-posta doğrulama, 2 adımlı OTP login,
//       Google/Apple girişi, refresh/replay tespiti, logout, şifre sıfırlama, hesap
//       silme) ve bunların altında yatan güvenlik kararlarını (bilgi sızıntısı
//       önleme, hesap bağlama, grace period kurtarma) doğrulamak.
// NEDEN: Bu servis projenin en güvenlik-kritik parçasıdır — repository/dış servisler
//        her zaman mock'lanır (CODING_STANDARDS.md §7.4), gerçek DB/HTTP çağrısı yapılmaz.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions, Microsoft.Extensions.Configuration,
//                WordLearner.Application.Services.AuthService.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Services;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Enums;

namespace WordLearner.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IGoogleTokenValidator> _googleValidator = new();
    private readonly Mock<IAppleTokenValidator> _appleValidator = new();

    // AMAÇ: Testlerde gerçek appsettings.json okumadan sabit bir Jwt:ExpirationMinutes sağlar.
    // NEDEN: AuthService.ExpiresInSeconds bu değeri IConfiguration'dan okur (AuthTokenResponse.ExpiresIn için).
    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:ExpirationMinutes"] = "15" })
            .Build();

    private AuthService CreateService() =>
        new(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _passwordService.Object,
            _tokenService.Object,
            _emailService.Object,
            _googleValidator.Object,
            _appleValidator.Object,
            CreateConfiguration()
        );

    private static User CreateActiveUser(int id = 1, string email = "test@example.com", string? passwordHash = "hash") =>
        new()
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            IsEmailVerified = true,
        };

    // AMAÇ: ITokenService mock'unun her testte tutarlı bir access/refresh token üretmesini sağlar.
    private void SetupTokenService()
    {
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));
    }

    // ═══ RegisterAsync ═══

    /// <summary>
    /// RegisterAsync_NewEmail_CreatesUserAndSendsVerificationOtp
    ///
    /// AMAÇ: Daha önce kayıtlı olmayan bir e-postayla kayıt olunduğunda kullanıcının
    ///       oluşturulduğunu ve doğrulama e-postasının gönderildiğini doğrulamak.
    /// NEDEN: Register akışının mutlu yolu — şifre hash'lenmeli, OTP üretilmeli, e-posta atılmalı.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndSendsVerificationOtp()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("new@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _passwordService.Setup(p => p.Hash("Deneme123!@#")).Returns("hashed-password");
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        var servis = CreateService();
        var request = new RegisterRequest("new@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var sonuc = await servis.RegisterAsync(request);

        // ASSERT
        sonuc.Email.Should().Be("new@example.com");
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.PasswordHash == "hashed-password"), null, default), Times.Once);
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync("new@example.com", It.IsAny<string>(), default), Times.Once);
    }

    /// <summary>
    /// RegisterAsync_EmailAlreadyRegistered_ThrowsDuplicateEmailException
    ///
    /// AMAÇ: Aktif bir kullanıcının e-postasıyla tekrar kayıt denendiğinde
    ///       DuplicateEmailException fırlatıldığını doğrulamak.
    /// NEDEN: E-posta benzersizliği DB constraint'i değil, servis katmanında zorlanır.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_EmailAlreadyRegistered_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("var@example.com", default)).ReturnsAsync(CreateActiveUser());
        var servis = CreateService();
        var request = new RegisterRequest("var@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => servis.RegisterAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    /// <summary>
    /// RegisterAsync_EmailPreviouslyAnonymized_ThrowsDuplicateEmailException
    ///
    /// AMAÇ: Daha önce anonimleştirilmiş bir hesabın orijinal e-postasıyla kayıt
    ///       denendiğinde de DuplicateEmailException fırlatıldığını doğrulamak.
    /// NEDEN: REFERENCE/SECURITY.md §9 — anonimleştirilmiş bir e-posta tekrar kullanılamaz;
    ///        bu kontrol GetByEmailAsync'in bulamadığı (anonimleştirme sonrası e-posta
    ///        değiştiği için) durumları OriginalEmailHash üzerinden yakalar.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_EmailPreviouslyAnonymized_ThrowsDuplicateEmailException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("eski@example.com", default)).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.HashToken("eski@example.com")).Returns("email-hash");
        _userRepo.Setup(r => r.OriginalEmailHashExistsAsync("email-hash", default)).ReturnsAsync(true);
        var servis = CreateService();
        var request = new RegisterRequest("eski@example.com", "Deneme123!@#", "Test", "Kullanici");

        // ACT
        var act = () => servis.RegisterAsync(request);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    // ═══ VerifyEmailAsync ═══

    /// <summary>
    /// VerifyEmailAsync_ValidOtp_MarksEmailVerifiedAndClearsOtp
    ///
    /// AMAÇ: Doğru OTP kodu girildiğinde IsEmailVerified'ın true'ya çekildiğini ve
    ///       bekleyen OTP alanlarının temizlendiğini doğrulamak.
    /// NEDEN: Bu adım tamamlanmadan hesap login akışında (IsActive kontrolü ayrı, ama
    ///        onboarding vb. akışlarda) doğrulanmamış olarak kalır.
    /// </summary>
    [Fact]
    public async Task VerifyEmailAsync_ValidOtp_MarksEmailVerifiedAndClearsOtp()
    {
        // ARRANGE
        var user = new User
        {
            Email = "test@example.com",
            PendingOtpCodeHash = "otp-hash",
            PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            PendingOtpCodePurpose = OtpPurpose.EmailVerification,
        };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        var servis = CreateService();

        // ACT
        await servis.VerifyEmailAsync(new VerifyEmailRequest("test@example.com", "123456"));

        // ASSERT
        user.IsEmailVerified.Should().BeTrue();
        user.PendingOtpCodeHash.Should().BeNull();
        _userRepo.Verify(r => r.UpdateAsync(user, null, default), Times.Once);
    }

    /// <summary>
    /// VerifyEmailAsync_WrongOtpCode_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP kodu girildiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// NEDEN: ValidateOtp'nin hash karşılaştırması EmailVerification akışında da geçerli olmalı.
    /// </summary>
    [Fact]
    public async Task VerifyEmailAsync_WrongOtpCode_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = new User
        {
            Email = "test@example.com",
            PendingOtpCodeHash = "dogru-hash",
            PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5),
            PendingOtpCodePurpose = OtpPurpose.EmailVerification,
        };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("999999")).Returns("yanlis-hash");
        var servis = CreateService();

        // ACT
        var act = () => servis.VerifyEmailAsync(new VerifyEmailRequest("test@example.com", "999999"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    // ═══ ResendVerificationAsync ═══

    /// <summary>
    /// ResendVerificationAsync_UnverifiedUserExists_SendsNewOtp
    ///
    /// AMAÇ: Doğrulanmamış bir kullanıcı için yeni OTP üretilip e-posta gönderildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResendVerificationAsync_UnverifiedUserExists_SendsNewOtp()
    {
        // ARRANGE
        var user = new User { Email = "test@example.com", IsEmailVerified = false };
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        var servis = CreateService();

        // ACT
        await servis.ResendVerificationAsync(new ResendVerificationRequest("test@example.com"));

        // ASSERT
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync("test@example.com", It.IsAny<string>(), default), Times.Once);
    }

    /// <summary>
    /// ResendVerificationAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage
    ///
    /// AMAÇ: Kayıtlı olmayan bir e-posta için de aynı mesajın döndüğünü ama e-posta
    ///       GÖNDERİLMEDİĞİNİ doğrulamak.
    /// NEDEN: E-posta numaralandırma (enumeration) saldırısını önlemek için — yanıt
    ///        farklı olsaydı bir saldırgan hangi e-postaların kayıtlı olduğunu anlayabilirdi.
    /// </summary>
    [Fact]
    public async Task ResendVerificationAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        var servis = CreateService();

        // ACT
        var sonuc = await servis.ResendVerificationAsync(new ResendVerificationRequest("yok@example.com"));

        // ASSERT
        sonuc.Message.Should().NotBeNullOrEmpty();
        _emailService.Verify(e => e.SendEmailVerificationOtpAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    // ═══ LoginAsync ═══

    /// <summary>
    /// LoginAsync_ValidCredentials_SendsLoginOtp
    ///
    /// AMAÇ: Doğru şifre ile login adım 1'in OTP gönderdiğini (token DÖNMEDİĞİNİ) doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_SendsLoginOtp()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        var servis = CreateService();

        // ACT
        await servis.LoginAsync(new LoginRequest(user.Email, "Deneme123!@#"));

        // ASSERT
        _emailService.Verify(e => e.SendLoginOtpAsync(user.Email, It.IsAny<string>(), default), Times.Once);
    }

    /// <summary>
    /// LoginAsync_WrongPassword_ThrowsInvalidCredentialsException
    ///
    /// AMAÇ: Yanlış şifre girildiğinde InvalidCredentialsException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginAsync(new LoginRequest(user.Email, "YanlisSifre"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    /// <summary>
    /// LoginAsync_UserNotFound_StillCallsVerifyWithFakeHashForTimingSafety
    ///
    /// AMAÇ: Kullanıcı bulunamadığında bile PasswordService.Verify'ın (FakePasswordHashForTiming
    ///       ile) ÇAĞRILDIĞINI ve InvalidCredentialsException fırlatıldığını doğrulamak.
    /// NEDEN: Timing attack önlemi — kullanıcı var/yok fark etmeksizin aynı süre harcanmalı;
    ///        Verify çağrılmazsa "kullanıcı yok" yanıtı ölçülebilir şekilde daha hızlı döner,
    ///        bu da bir saldırganın e-posta numaralandırması yapmasına izin verir.
    /// </summary>
    [Fact]
    public async Task LoginAsync_UserNotFound_StillCallsVerifyWithFakeHashForTimingSafety()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        _passwordService.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginAsync(new LoginRequest("yok@example.com", "HerhangiBirSifre123!"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _passwordService.Verify(p => p.Verify("HerhangiBirSifre123!", It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// LoginAsync_AccountNotActive_ThrowsAccountNotActiveException
    ///
    /// AMAÇ: Şifre doğru olsa bile dondurulmuş (IsActive=false) bir hesapla login
    ///       denendiğinde AccountNotActiveException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginAsync_AccountNotActive_ThrowsAccountNotActiveException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.IsActive = false;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginAsync(new LoginRequest(user.Email, "Deneme123!@#"));

        // ASSERT
        await act.Should().ThrowAsync<AccountNotActiveException>();
    }

    // ═══ VerifyLoginOtpAsync ═══

    /// <summary>
    /// VerifyLoginOtpAsync_ValidOtp_ReturnsAccessAndRefreshTokens
    ///
    /// AMAÇ: Doğru OTP kodu girildiğinde access+refresh token içeren yanıtın döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task VerifyLoginOtpAsync_ValidOtp_ReturnsAccessAndRefreshTokens()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "otp-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.LoginOtp;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        _passwordService.Setup(p => p.HashToken("refresh-token")).Returns("refresh-token-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        var sonuc = await servis.VerifyLoginOtpAsync(new VerifyOtpRequest(user.Email, "123456"), "1.2.3.4");

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        sonuc.RefreshToken.Should().Be("refresh-token");
        sonuc.AccountWasRecovered.Should().BeFalse();
    }

    /// <summary>
    /// VerifyLoginOtpAsync_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP kodu girildiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task VerifyLoginOtpAsync_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "dogru-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.LoginOtp;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("999999")).Returns("yanlis-hash");
        var servis = CreateService();

        // ACT
        var act = () => servis.VerifyLoginOtpAsync(new VerifyOtpRequest(user.Email, "999999"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    /// <summary>
    /// VerifyLoginOtpAsync_AccountWithinGracePeriod_RecoversAccountAndFlagsResponse
    ///
    /// AMAÇ: Soft-delete'li (IsDeleted=true) ama grace period içindeki bir hesabın OTP
    ///       doğrulaması sırasında otomatik kurtarıldığını ve yanıtta accountWasRecovered=true
    ///       döndüğünü doğrulamak.
    /// NEDEN: REFERENCE/SECURITY.md §1 — kullanıcı 30 gün içinde tekrar login olursa
    ///        hesap silme işlemi geri alınır; bu davranış CompleteLoginAsync'in kritik dalı.
    /// </summary>
    [Fact]
    public async Task VerifyLoginOtpAsync_AccountWithinGracePeriod_RecoversAccountAndFlagsResponse()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "otp-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.LoginOtp;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow.AddDays(-10);
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(20);
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        var sonuc = await servis.VerifyLoginOtpAsync(new VerifyOtpRequest(user.Email, "123456"), null);

        // ASSERT
        sonuc.AccountWasRecovered.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
        user.ScheduledDeletionAt.Should().BeNull();
    }

    // ═══ LoginWithGoogleAsync ═══

    /// <summary>
    /// LoginWithGoogleAsync_ExistingGoogleUser_ReturnsTokensWithoutCreatingNewAccount
    ///
    /// AMAÇ: GoogleId ile eşleşen bir kullanıcı bulunduğunda yeni hesap açılmadan
    ///       doğrudan token döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogleAsync_ExistingGoogleUser_ReturnsTokensWithoutCreatingNewAccount()
    {
        // ARRANGE
        var user = CreateActiveUser(passwordHash: null);
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-123", user.Email, "Test", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        var sonuc = await servis.LoginWithGoogleAsync(new GoogleLoginRequest("id-token"), null);

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), null, default), Times.Never);
    }

    /// <summary>
    /// LoginWithGoogleAsync_NoExistingAccount_CreatesNewUser
    ///
    /// AMAÇ: Ne GoogleId ne de e-posta ile eşleşen bir kullanıcı bulunmadığında yeni
    ///       hesap oluşturulduğunu doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogleAsync_NoExistingAccount_CreatesNewUser()
    {
        // ARRANGE
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-999", "yeni@example.com", "Yeni", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-999", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("yeni@example.com", default)).ReturnsAsync((User?)null);
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        await servis.LoginWithGoogleAsync(new GoogleLoginRequest("id-token"), null);

        // ASSERT
        _userRepo.Verify(
            r => r.AddAsync(It.Is<User>(u => u.GoogleId == "google-999" && u.AuthProvider == "Google"), null, default),
            Times.Once
        );
    }

    /// <summary>
    /// LoginWithGoogleAsync_EmailMatchesExistingLocalAccount_LinksGoogleIdToAccount
    ///
    /// AMAÇ: Google'dan gelen e-posta, mevcut bir yerel (Local) hesapla eşleştiğinde
    ///       yeni hesap açmak yerine GoogleId'nin o hesaba bağlandığını (account linking)
    ///       doğrulamak.
    /// NEDEN: Aksi hâlde aynı kişi aynı e-posta için iki ayrı hesaba sahip olur —
    ///        "user register olurken hangi hesaba giriyor" tasarım kararının kalbi.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogleAsync_EmailMatchesExistingLocalAccount_LinksGoogleIdToAccount()
    {
        // ARRANGE
        var mevcutYerelHesap = CreateActiveUser(email: "ortak@example.com");
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-777", "ortak@example.com", "Ortak", "Kullanici"));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-777", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("ortak@example.com", default)).ReturnsAsync(mevcutYerelHesap);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        await servis.LoginWithGoogleAsync(new GoogleLoginRequest("id-token"), null);

        // ASSERT — yeni kayıt AÇILMAMALI, mevcut hesaba GoogleId bağlanmalı
        mevcutYerelHesap.GoogleId.Should().Be("google-777");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), null, default), Times.Never);
    }

    /// <summary>
    /// LoginWithGoogleAsync_InvalidToken_ThrowsInvalidSocialTokenException
    ///
    /// AMAÇ: Google token doğrulaması null döndüğünde InvalidSocialTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogleAsync_InvalidToken_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _googleValidator.Setup(g => g.ValidateAsync("gecersiz-token", default)).ReturnsAsync((GoogleTokenPayload?)null);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginWithGoogleAsync(new GoogleLoginRequest("gecersiz-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }

    /// <summary>
    /// LoginWithGoogleAsync_AccountNotActive_ThrowsAccountNotActiveException
    ///
    /// AMAÇ: Google ile eşleşen hesap dondurulmuşsa (IsActive=false) login'in reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithGoogleAsync_AccountNotActive_ThrowsAccountNotActiveException()
    {
        // ARRANGE
        var user = CreateActiveUser(passwordHash: null);
        user.IsActive = false;
        _googleValidator
            .Setup(g => g.ValidateAsync("id-token", default))
            .ReturnsAsync(new GoogleTokenPayload("google-123", user.Email, null, null));
        _userRepo.Setup(r => r.GetByGoogleIdAsync("google-123", default)).ReturnsAsync(user);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginWithGoogleAsync(new GoogleLoginRequest("id-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<AccountNotActiveException>();
    }

    // ═══ LoginWithAppleAsync ═══

    /// <summary>
    /// LoginWithAppleAsync_ExistingAppleUser_ReturnsTokens
    ///
    /// AMAÇ: AppleId ile eşleşen mevcut bir kullanıcı için token döndüğünü doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithAppleAsync_ExistingAppleUser_ReturnsTokens()
    {
        // ARRANGE
        var user = CreateActiveUser(passwordHash: null);
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-123", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-123", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        var sonuc = await servis.LoginWithAppleAsync(new AppleLoginRequest("identity-token"), null);

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
    }

    /// <summary>
    /// LoginWithAppleAsync_FirstAuthorizationWithEmail_CreatesNewAccount
    ///
    /// AMAÇ: İlk yetkilendirmede (e-posta dolu) ne AppleId ne de e-posta eşleşmesi
    ///       olmadığında yeni hesap açıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithAppleAsync_FirstAuthorizationWithEmail_CreatesNewAccount()
    {
        // ARRANGE
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-999", "yeni@example.com"));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-999", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("yeni@example.com", default)).ReturnsAsync((User?)null);
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), null, default))
            .ReturnsAsync((User u, int? _, CancellationToken _) => u);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        await servis.LoginWithAppleAsync(new AppleLoginRequest("identity-token"), null);

        // ASSERT
        _userRepo.Verify(
            r => r.AddAsync(It.Is<User>(u => u.AppleId == "apple-999" && u.AuthProvider == "Apple"), null, default),
            Times.Once
        );
    }

    /// <summary>
    /// LoginWithAppleAsync_SubsequentLoginWithoutEmail_MatchesByAppleIdOnly
    ///
    /// AMAÇ: Apple'ın yalnızca İLK yetkilendirmede e-posta verdiği, sonraki girişlerde
    ///       payload.Email'in null geldiği senaryoda AppleId ile hâlâ doğru kullanıcının
    ///       bulunduğunu ve mevcut e-postanın ÜZERİNE YAZILMADIĞINI doğrulamak.
    /// NEDEN: AppleTokenValidator.cs'in NEDEN açıklamasındaki kritik kısıt — email
    ///        yoksa DB'deki mevcut e-posta korunmalı.
    /// </summary>
    [Fact]
    public async Task LoginWithAppleAsync_SubsequentLoginWithoutEmail_MatchesByAppleIdOnly()
    {
        // ARRANGE
        var user = CreateActiveUser(email: "korunan@example.com", passwordHash: null);
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-123", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-123", default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("refresh-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        await servis.LoginWithAppleAsync(new AppleLoginRequest("identity-token"), null);

        // ASSERT — e-posta hiç değişmemeli, GetByEmailAsync hiç çağrılmamalı (email null olduğu için)
        user.Email.Should().Be("korunan@example.com");
        _userRepo.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), default), Times.Never);
    }

    /// <summary>
    /// LoginWithAppleAsync_NoEmailAndNoExistingAccount_ThrowsInvalidSocialTokenException
    ///
    /// AMAÇ: Ne AppleId ile eşleşme ne de e-posta (savunmacı/teorik olarak beklenmeyen
    ///       bir durum) varken yeni hesap açılamayacağını, InvalidSocialTokenException
    ///       fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task LoginWithAppleAsync_NoEmailAndNoExistingAccount_ThrowsInvalidSocialTokenException()
    {
        // ARRANGE
        _appleValidator
            .Setup(a => a.ValidateAsync("identity-token", default))
            .ReturnsAsync(new AppleTokenPayload("apple-000", null));
        _userRepo.Setup(r => r.GetByAppleIdAsync("apple-000", default)).ReturnsAsync((User?)null);
        var servis = CreateService();

        // ACT
        var act = () => servis.LoginWithAppleAsync(new AppleLoginRequest("identity-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidSocialTokenException>();
    }

    // ═══ RefreshAsync ═══

    /// <summary>
    /// RefreshAsync_ValidToken_RotatesTokenAndReturnsNewPair
    ///
    /// AMAÇ: Geçerli bir refresh token ile yeni bir access+refresh token çifti
    ///       üretildiğini ve eski token'ın kullanıldı (IsUsed=true) olarak işaretlendiğini
    ///       doğrulamak.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesTokenAndReturnsNewPair()
    {
        // ARRANGE
        var user = CreateActiveUser();
        var mevcutToken = new RefreshToken
        {
            UserId = user.Id,
            TokenFamily = "family-1",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
        };
        _passwordService.Setup(p => p.HashToken("eski-refresh-token")).Returns("eski-hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("eski-hash", default)).ReturnsAsync(mevcutToken);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("refresh-token")).Returns("yeni-hash");
        SetupTokenService();
        var servis = CreateService();

        // ACT
        var sonuc = await servis.RefreshAsync(new RefreshRequest("eski-refresh-token"), "1.2.3.4");

        // ASSERT
        sonuc.AccessToken.Should().Be("access-token");
        mevcutToken.IsUsed.Should().BeTrue();
        _refreshTokenRepo.Verify(
            r => r.AddAsync(It.Is<RefreshToken>(t => t.TokenFamily == "family-1"), null, default),
            Times.Once
        );
    }

    /// <summary>
    /// RefreshAsync_TokenNotFound_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: DB'de bulunamayan bir refresh token için InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync((RefreshToken?)null);
        var servis = CreateService();

        // ACT
        var act = () => servis.RefreshAsync(new RefreshRequest("gecersiz-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    /// <summary>
    /// RefreshAsync_TokenExpired_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Süresi dolmuş bir refresh token için InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_TokenExpired_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var suresiGecmisToken = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(-1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(suresiGecmisToken);
        var servis = CreateService();

        // ACT
        var act = () => servis.RefreshAsync(new RefreshRequest("suresi-gecmis-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    /// <summary>
    /// RefreshAsync_TokenAlreadyUsed_RevokesEntireFamilyAndThrows
    ///
    /// AMAÇ: Zaten kullanılmış (IsUsed=true) bir refresh token TEKRAR kullanıldığında
    ///       (replay saldırısı) aynı TokenFamily'deki TÜM token'ların iptal edildiğini
    ///       ve InvalidRefreshTokenException fırlatıldığını doğrulamak.
    /// NEDEN: Token Family Pattern'in en kritik davranışı — bir token çalınıp kullanılmışsa
    ///        gerçek kullanıcı bir sonraki refresh'te bunu (replay) tetikler ve tüm
    ///        family (dolayısıyla saldırganın elindeki token da) iptal edilir.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_TokenAlreadyUsed_RevokesEntireFamilyAndThrows()
    {
        // ARRANGE
        var kullanilmisToken = new RefreshToken
        {
            TokenFamily = "family-replay",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsUsed = true,
        };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(kullanilmisToken);
        var servis = CreateService();

        // ACT
        var act = () => servis.RefreshAsync(new RefreshRequest("kullanilmis-token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _refreshTokenRepo.Verify(r => r.RevokeFamilyAsync("family-replay", default), Times.Once);
    }

    /// <summary>
    /// RefreshAsync_UserAnonymized_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Token geçerli olsa bile ait olduğu kullanıcı anonimleştirilmişse
    ///       (IsAnonymized=true) refresh'in reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_UserAnonymized_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.IsAnonymized = true;
        var token = new RefreshToken { UserId = user.Id, TokenFamily = "family-1", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(token);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        var servis = CreateService();

        // ACT
        var act = () => servis.RefreshAsync(new RefreshRequest("token"), null);

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    // ═══ LogoutAsync ═══

    /// <summary>
    /// LogoutAsync_OwnToken_RevokesToken
    ///
    /// AMAÇ: Kullanıcının kendi refresh token'ını iptal edebildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_OwnToken_RevokesToken()
    {
        // ARRANGE
        var token = new RefreshToken { UserId = 1 };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(token);
        var servis = CreateService();

        // ACT
        await servis.LogoutAsync(userId: 1, new RefreshRequest("token"));

        // ASSERT
        token.RevokedAt.Should().NotBeNull();
    }

    /// <summary>
    /// LogoutAsync_TokenBelongsToDifferentUser_ThrowsInvalidRefreshTokenException
    ///
    /// AMAÇ: Başka bir kullanıcıya ait refresh token'ı iptal etmeye çalışıldığında
    ///       reddedildiğini doğrulamak.
    /// NEDEN: Sahiplik kontrolü olmazsa bir kullanıcı başka birinin oturumunu kapatabilirdi.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_TokenBelongsToDifferentUser_ThrowsInvalidRefreshTokenException()
    {
        // ARRANGE
        var baskasininTokeni = new RefreshToken { UserId = 2 };
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("hash");
        _refreshTokenRepo.Setup(r => r.GetByTokenHashAsync("hash", default)).ReturnsAsync(baskasininTokeni);
        var servis = CreateService();

        // ACT
        var act = () => servis.LogoutAsync(userId: 1, new RefreshRequest("baskasinin-tokeni"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    // ═══ ForgotPasswordAsync ═══

    /// <summary>
    /// ForgotPasswordAsync_ExistingUser_SendsResetOtp
    ///
    /// AMAÇ: Kayıtlı bir kullanıcı için şifre sıfırlama OTP'sinin gönderildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_SendsResetOtp()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        var servis = CreateService();

        // ACT
        await servis.ForgotPasswordAsync(new ForgotPasswordRequest(user.Email));

        // ASSERT
        _emailService.Verify(e => e.SendPasswordResetOtpAsync(user.Email, It.IsAny<string>(), default), Times.Once);
    }

    /// <summary>
    /// ForgotPasswordAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage
    ///
    /// AMAÇ: Kayıtlı olmayan bir e-posta için de e-posta GÖNDERİLMEDİĞİNİ ama aynı
    ///       mesajın döndüğünü doğrulamak (e-posta numaralandırma önlemi).
    /// </summary>
    [Fact]
    public async Task ForgotPasswordAsync_UserNotFound_DoesNotSendEmailButReturnsSameMessage()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByEmailAsync("yok@example.com", default)).ReturnsAsync((User?)null);
        var servis = CreateService();

        // ACT
        var sonuc = await servis.ForgotPasswordAsync(new ForgotPasswordRequest("yok@example.com"));

        // ASSERT
        sonuc.Message.Should().NotBeNullOrEmpty();
        _emailService.Verify(e => e.SendPasswordResetOtpAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    // ═══ ResetPasswordAsync ═══

    /// <summary>
    /// ResetPasswordAsync_ValidOtp_UpdatesPasswordAndRevokesAllRefreshTokens
    ///
    /// AMAÇ: Doğru OTP ile şifrenin güncellendiğini ve kullanıcının TÜM refresh
    ///       token'larının iptal edildiğini (tüm cihazlardan çıkış) doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_ValidOtp_UpdatesPasswordAndRevokesAllRefreshTokens()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "otp-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.PasswordReset;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        _passwordService.Setup(p => p.Hash("YeniSifre123!@#")).Returns("yeni-hash");
        var servis = CreateService();

        // ACT
        await servis.ResetPasswordAsync(new ResetPasswordRequest(user.Email, "123456", "YeniSifre123!@#"));

        // ASSERT
        user.PasswordHash.Should().Be("yeni-hash");
        _refreshTokenRepo.Verify(r => r.RevokeAllForUserAsync(user.Id, default), Times.Once);
    }

    /// <summary>
    /// ResetPasswordAsync_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP ile şifre sıfırlama denendiğinde InvalidOtpException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task ResetPasswordAsync_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "dogru-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.PasswordReset;
        _userRepo.Setup(r => r.GetByEmailAsync(user.Email, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("999999")).Returns("yanlis-hash");
        var servis = CreateService();

        // ACT
        var act = () => servis.ResetPasswordAsync(new ResetPasswordRequest(user.Email, "999999", "YeniSifre123!@#"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }

    // ═══ RequestAccountDeletionAsync ═══

    /// <summary>
    /// RequestAccountDeletionAsync_ExistingUser_SendsDeletionOtp
    ///
    /// AMAÇ: Var olan bir kullanıcı için hesap silme onay OTP'sinin gönderildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task RequestAccountDeletionAsync_ExistingUser_SendsDeletionOtp()
    {
        // ARRANGE
        var user = CreateActiveUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken(It.IsAny<string>())).Returns("otp-hash");
        var servis = CreateService();

        // ACT
        await servis.RequestAccountDeletionAsync(user.Id);

        // ASSERT
        _emailService.Verify(e => e.SendAccountDeletionOtpAsync(user.Email, It.IsAny<string>(), default), Times.Once);
    }

    /// <summary>
    /// RequestAccountDeletionAsync_UserNotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Var olmayan bir userId ile hesap silme talebi oluşturulduğunda
    ///       EntityNotFoundException fırlatıldığını doğrulamak.
    /// NEDEN: Bu userId [Authorize] token'ından geldiği için normalde her zaman var
    ///        olmalı, ama hesap bu arada silinmiş olabilir — savunmacı kontrol.
    /// </summary>
    [Fact]
    public async Task RequestAccountDeletionAsync_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((User?)null);
        var servis = CreateService();

        // ACT
        var act = () => servis.RequestAccountDeletionAsync(999);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ═══ ConfirmAccountDeletionAsync ═══

    /// <summary>
    /// ConfirmAccountDeletionAsync_ValidOtpAndPassword_SoftDeletesAndSchedulesAnonymization
    ///
    /// AMAÇ: Doğru OTP + doğru şifre ile hesabın soft-delete edildiğini ve 30 gün
    ///       sonrasına anonimleştirme zamanlandığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_ValidOtpAndPassword_SoftDeletesAndSchedulesAnonymization()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "otp-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.AccountDeletion;
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        _passwordService.Setup(p => p.Verify("Deneme123!@#", "hash")).Returns(true);
        var servis = CreateService();

        // ACT
        await servis.ConfirmAccountDeletionAsync(user.Id, new DeleteAccountConfirmRequest("123456", "Deneme123!@#"));

        // ASSERT
        user.IsDeleted.Should().BeTrue();
        user.ScheduledDeletionAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
        _refreshTokenRepo.Verify(r => r.RevokeAllForUserAsync(user.Id, default), Times.Once);
    }

    /// <summary>
    /// ConfirmAccountDeletionAsync_WrongPassword_ThrowsInvalidCredentialsException
    ///
    /// AMAÇ: OTP doğru olsa bile yanlış şifre ile hesap silmenin onaylanamadığını doğrulamak.
    /// NEDEN: Geri alınamaz bir işlem olduğu için çift onay (OTP + şifre) zorunlu.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "otp-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.AccountDeletion;
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("123456")).Returns("otp-hash");
        _passwordService.Setup(p => p.Verify("YanlisSifre", "hash")).Returns(false);
        var servis = CreateService();

        // ACT
        var act = () => servis.ConfirmAccountDeletionAsync(user.Id, new DeleteAccountConfirmRequest("123456", "YanlisSifre"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    /// <summary>
    /// ConfirmAccountDeletionAsync_WrongOtp_ThrowsInvalidOtpException
    ///
    /// AMAÇ: Yanlış OTP ile hesap silme onayının reddedildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task ConfirmAccountDeletionAsync_WrongOtp_ThrowsInvalidOtpException()
    {
        // ARRANGE
        var user = CreateActiveUser();
        user.PendingOtpCodeHash = "dogru-hash";
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.PendingOtpCodePurpose = OtpPurpose.AccountDeletion;
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _passwordService.Setup(p => p.HashToken("999999")).Returns("yanlis-hash");
        var servis = CreateService();

        // ACT
        var act = () => servis.ConfirmAccountDeletionAsync(user.Id, new DeleteAccountConfirmRequest("999999", "Deneme123!@#"));

        // ASSERT
        await act.Should().ThrowAsync<InvalidOtpException>();
    }
}
