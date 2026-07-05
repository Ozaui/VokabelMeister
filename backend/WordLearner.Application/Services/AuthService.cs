// ─────────────────────────────────────────────────────────────────────────────
// AuthService.cs
//
// AMAÇ: IAuthService'in implementasyonu — kayıt, e-posta doğrulama, 2 adımlı OTP
//       login, Google/Apple girişi, refresh (Token Family Pattern), logout, şifre
//       sıfırlama ve hesap silme (30 gün grace) akışlarının tamamı.
// NEDEN: Tüm iş kuralı burada toplanır; AuthController (bir sonraki task) yalnızca
//        DTO alıp bu servisi çağıran ince bir katman olacak.
// BAĞIMLILIKLAR: IUserRepository, IRefreshTokenRepository, IPasswordService,
//                ITokenService, IEmailService, IGoogleTokenValidator,
//                IAppleTokenValidator, IConfiguration.
// ─────────────────────────────────────────────────────────────────────────────

using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Auth;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities;
using WordLearner.Domain.Enums;

namespace WordLearner.Application.Services;

public class AuthService : IAuthService
{
    // NEDEN 5dk: REFERENCE/SECURITY.md §1'de pinlenen OTP geçerlilik süresi (login/kayıt/şifre sıfırlama).
    private const int OtpExpiryMinutes = 5;

    // NEDEN 15dk: Hesap silme OTP'si diğerlerinden daha kısa — geri alınamaz bir işlem
    //        olduğu için pencere daraltılır (REFERENCE/API_ENDPOINTS.md §3).
    private const int DeleteAccountOtpExpiryMinutes = 15;

    // NEDEN 30 gün: REFERENCE/SECURITY.md §9 — hesap silme grace period.
    private const int AccountDeletionGraceDays = 30;

    // AMAÇ: Kullanıcı bulunamadığında/şifresi olmadığında (sosyal hesap) bile SABİT
    //       SÜRELİ bir BCrypt karşılaştırması yapılabilmesi için önceden hesaplanmış,
    //       geçerli formatlı ama hiçbir gerçek şifreyle eşleşmeyecek bir hash.
    // NEDEN static + doğrudan BCrypt.Net: Bu alan tip yüklenirken (instance yokken) bir
    //        kez hesaplanır — IPasswordService instance metodu bu bağlamda kullanılamaz;
    //        PasswordService.Hash'in yaptığı işlemin aynısı (workFactor:12) burada tekrarlanır.
    private static readonly string FakePasswordHashForTiming = BCrypt.Net.BCrypt.HashPassword(
        Guid.NewGuid().ToString(),
        workFactor: 12
    );

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IEmailService emailService,
        IGoogleTokenValidator googleTokenValidator,
        IAppleTokenValidator appleTokenValidator,
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _emailService = emailService;
        _googleTokenValidator = googleTokenValidator;
        _appleTokenValidator = appleTokenValidator;
        _configuration = configuration;
    }

    // AMAÇ: Yeni kullanıcı kaydı oluşturur, e-posta doğrulama OTP'si gönderir.
    // NEDEN: Şifre asla düz metin saklanmaz (Hash); e-posta hem aktif kullanıcılar hem
    //        de daha önce anonimleştirilmiş hesaplar arasında benzersiz olmalı.
    // NASIL: 1) E-posta çakışması kontrol et (aktif + anonimleştirilmiş)  2) Şifreyi hash'le
    //        3) OTP üret  4) Kullanıcıyı kaydet  5) Doğrulama e-postası gönder.
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new DuplicateEmailException();

        // NEDEN: Anonimleştirilmiş bir hesabın orijinal e-postasıyla tekrar kayıt
        //        açılmasını engeller (REFERENCE/SECURITY.md §9).
        var emailHash = _passwordService.HashToken(request.Email);
        if (await _userRepository.OriginalEmailHashExistsAsync(emailHash, ct))
            throw new DuplicateEmailException();

        var (otpCode, otpHash) = GenerateOtp();

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordService.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = "Local",
            PendingOtpCodeHash = otpHash,
            PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            PendingOtpCodePurpose = OtpPurpose.EmailVerification,
        };

        await _userRepository.AddAsync(user, ct: ct);
        await _emailService.SendEmailVerificationOtpAsync(user.Email, otpCode, ct);

        return new RegisterResponse(user.Id, user.Email, user.FirstName, user.CurrentLevel);
    }

    // AMAÇ: Kayıt sonrası e-postaya gelen OTP kodunu doğrular, hesabı aktive eder.
    public async Task<MessageResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken ct = default
    )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        ValidateOtp(user, request.OtpCode, OtpPurpose.EmailVerification);

        user!.IsEmailVerified = true;
        user.EmailVerifiedAt = DateTime.UtcNow;
        ClearOtp(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        return new MessageResponse("E-posta doğrulandı.");
    }

    // AMAÇ: E-posta doğrulama kodunu tekrar gönderir.
    // NEDEN: Kullanıcı bulunamasa da aynı yanıt döner — e-posta numaralandırma
    //        saldırısını önlemek için (ForgotPasswordAsync ile aynı desen).
    public async Task<MessageResponse> ResendVerificationAsync(
        ResendVerificationRequest request,
        CancellationToken ct = default
    )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is not null && !user.IsEmailVerified)
        {
            var (otpCode, otpHash) = GenerateOtp();
            user.PendingOtpCodeHash = otpHash;
            user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            user.PendingOtpCodePurpose = OtpPurpose.EmailVerification;
            await _userRepository.UpdateAsync(user, ct: ct);
            await _emailService.SendEmailVerificationOtpAsync(user.Email, otpCode, ct);
        }

        return new MessageResponse("Doğrulama kodu gönderildi.");
    }

    // AMAÇ: Login adım 1 — şifreyi doğrular, başarılıysa OTP gönderir (token DÖNMEZ).
    // NEDEN: REFERENCE/SECURITY.md §1 — 2 adımlı OTP girişinin ilk adımı. Timing attack
    //        önlemi: kullanıcı yoksa veya şifresi yoksa (sosyal hesap) bile SABİT
    //        SÜRELİ bir BCrypt karşılaştırması yapılır, bu yüzden hangi durumun
    //        gerçekleştiği fark etmeksizin aynı sürede aynı hata döner.
    public async Task<MessageResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        var hashToVerify = user?.PasswordHash ?? FakePasswordHashForTiming;
        var passwordValid = _passwordService.Verify(request.Password, hashToVerify);

        if (user is null || user.PasswordHash is null || !passwordValid)
            throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new AccountNotActiveException();

        var (otpCode, otpHash) = GenerateOtp();
        user.PendingOtpCodeHash = otpHash;
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
        user.PendingOtpCodePurpose = OtpPurpose.LoginOtp;
        await _userRepository.UpdateAsync(user, ct: ct);

        await _emailService.SendLoginOtpAsync(user.Email, otpCode, ct);
        return new MessageResponse("OTP gönderildi.");
    }

    // AMAÇ: Login adım 2 — OTP'yi doğrular, başarılıysa access+refresh token üretir.
    public async Task<AuthTokenResponse> VerifyLoginOtpAsync(
        VerifyOtpRequest request,
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        ValidateOtp(user, request.OtpCode, OtpPurpose.LoginOtp);

        return await CompleteLoginAsync(user!, ipAddress, ct);
    }

    // AMAÇ: Google ID token'ı ile giriş yapar; hesap yoksa oluşturur, e-posta eşleşen
    //       yerel hesap varsa GoogleId'yi ona bağlar (account linking).
    // NEDEN: Google zaten kimliği doğruladığı için 2FA OTP adımı GEREKMEZ.
    public async Task<AuthTokenResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        var payload =
            await _googleTokenValidator.ValidateAsync(request.IdToken, ct)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByGoogleIdAsync(payload.GoogleId, ct);
        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email, ct);
            if (user is not null)
            {
                // NEDEN: Aynı e-postayla önceden yerel/başka sağlayıcı hesabı varsa,
                //        yeni bir hesap açmak yerine GoogleId bu hesaba bağlanır —
                //        aksi hâlde kullanıcı aynı e-posta için iki ayrı hesaba sahip olurdu.
                user.GoogleId = payload.GoogleId;
            }
            else
            {
                user = new User
                {
                    Email = payload.Email,
                    FirstName = payload.FirstName ?? "Kullanıcı",
                    LastName = payload.LastName ?? string.Empty,
                    AuthProvider = "Google",
                    GoogleId = payload.GoogleId,
                    IsEmailVerified = true,
                };
                await _userRepository.AddAsync(user, ct: ct);
            }
        }

        if (!user.IsActive)
            throw new AccountNotActiveException();

        return await CompleteLoginAsync(user, ipAddress, ct);
    }

    // AMAÇ: Apple identity token'ı ile giriş yapar; hesap yoksa oluşturur, e-posta
    //       eşleşen yerel hesap varsa AppleId'yi ona bağlar.
    // NEDEN: Apple e-postayı yalnızca İLK yetkilendirmede verir — sonraki girişlerde
    //        payload.Email null olabilir; bu durumda yalnızca AppleId ile aranır,
    //        DB'deki mevcut e-posta asla üzerine yazılmaz.
    public async Task<AuthTokenResponse> LoginWithAppleAsync(
        AppleLoginRequest request,
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        var payload =
            await _appleTokenValidator.ValidateAsync(request.IdentityToken, ct)
            ?? throw new InvalidSocialTokenException();

        var user = await _userRepository.GetByAppleIdAsync(payload.AppleId, ct);
        if (user is null)
        {
            if (payload.Email is not null)
                user = await _userRepository.GetByEmailAsync(payload.Email, ct);

            if (user is not null)
            {
                user.AppleId = payload.AppleId;
            }
            else
            {
                // NEDEN: İlk yetkilendirmede email gelmemesi teorik olarak beklenmez;
                //        savunmacı olarak ele alınır — email yoksa yeni kayıt açılamaz.
                if (payload.Email is null)
                    throw new InvalidSocialTokenException();

                user = new User
                {
                    Email = payload.Email,
                    FirstName = "Kullanıcı",
                    LastName = string.Empty,
                    AuthProvider = "Apple",
                    AppleId = payload.AppleId,
                    IsEmailVerified = true,
                };
                await _userRepository.AddAsync(user, ct: ct);
            }
        }

        if (!user.IsActive)
            throw new AccountNotActiveException();

        return await CompleteLoginAsync(user, ipAddress, ct);
    }

    // AMAÇ: Refresh token'ı doğrular, rotate eder, yeni access+refresh token çifti üretir.
    // NEDEN: Token Family Pattern (REFERENCE/SECURITY.md §1) — eski token tek kullanımlık;
    //        aynı family'den ikinci kullanım replay sayılır ve TÜM family iptal edilir.
    // NASIL: 1) Hash'e göre token'ı bul  2) Geçersiz/süresi dolmuş/iptalse reddet
    //        3) Zaten kullanılmışsa (replay) family'yi iptal et ve reddet  4) Kullanıldı
    //        işaretle  5) Aynı family'de yeni bir token üret.
    public async Task<AuthTokenResponse> RefreshAsync(
        RefreshRequest request,
        string? ipAddress,
        CancellationToken ct = default
    )
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (
            existingToken is null
            || existingToken.RevokedAt is not null
            || existingToken.ExpiresAt < DateTime.UtcNow
        )
            throw new InvalidRefreshTokenException();

        if (existingToken.IsUsed)
        {
            // NEDEN: SecurityLog:TokenReplay entegrasyonu A-04'ten sonra eklenecek
            //        (bkz. TASK/A_admin_panel_backend.md A-03 notu).
            await _refreshTokenRepository.RevokeFamilyAsync(existingToken.TokenFamily, ct);
            throw new InvalidRefreshTokenException();
        }

        existingToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(existingToken, ct: ct);

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, ct);
        if (user is null || !user.IsActive || user.IsAnonymized)
            throw new InvalidRefreshTokenException();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenResult = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(refreshTokenResult.Token),
            TokenFamily = existingToken.TokenFamily,
            ExpiresAt = refreshTokenResult.ExpiresAt,
            DeviceInfo = existingToken.DeviceInfo,
            IpAddress = ipAddress,
        };
        await _refreshTokenRepository.AddAsync(newRefreshToken, ct: ct);

        return new AuthTokenResponse(
            accessToken,
            refreshTokenResult.Token,
            ExpiresInSeconds(),
            new AuthUserDto(user.Id, user.CurrentLevel),
            false
        );
    }

    // AMAÇ: Verilen refresh token'ı kalıcı olarak iptal eder.
    // NEDEN: Sahiplik kontrolü — başkasının refresh token'ı bu userId ile iptal edilemez.
    public async Task LogoutAsync(int userId, RefreshRequest request, CancellationToken ct = default)
    {
        var tokenHash = _passwordService.HashToken(request.RefreshToken);
        var token = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, ct);

        if (token is null || token.UserId != userId)
            throw new InvalidRefreshTokenException();

        token.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(token, ct: ct);
    }

    // AMAÇ: Şifre sıfırlama OTP'si gönderir.
    // NEDEN: Kullanıcı yoksa/anonimleştirilmişse bile AYNI yanıt döner — e-posta
    //        numaralandırma saldırısını önler (REFERENCE/SECURITY.md §7).
    public async Task<MessageResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken ct = default
    )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is not null && !user.IsAnonymized)
        {
            var (otpCode, otpHash) = GenerateOtp();
            user.PendingOtpCodeHash = otpHash;
            user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            user.PendingOtpCodePurpose = OtpPurpose.PasswordReset;
            await _userRepository.UpdateAsync(user, ct: ct);
            await _emailService.SendPasswordResetOtpAsync(user.Email, otpCode, ct);
        }

        return new MessageResponse("Şifre sıfırlama kodu gönderildi.");
    }

    // AMAÇ: OTP + yeni şifre ile şifreyi değiştirir, tüm cihazlardan çıkış yapar.
    public async Task<MessageResponse> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default
    )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        ValidateOtp(user, request.OtpCode, OtpPurpose.PasswordReset);

        user!.PasswordHash = _passwordService.Hash(request.NewPassword);
        ClearOtp(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        // NEDEN: Şifre değiştiğinde tüm cihazlardan çıkış yapılır (REFERENCE/SECURITY.md §7).
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);
        await _emailService.SendPasswordChangedNotificationAsync(user.Email, ct);

        return new MessageResponse("Şifreniz güncellendi.");
    }

    // AMAÇ: Hesap silme OTP'si gönderir (15dk geçerli).
    public async Task<MessageResponse> RequestAccountDeletionAsync(
        int userId,
        CancellationToken ct = default
    )
    {
        var user =
            await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException(typeof(User), userId);

        var (otpCode, otpHash) = GenerateOtp();
        user.PendingOtpCodeHash = otpHash;
        user.PendingOtpCodeExpiresAt = DateTime.UtcNow.AddMinutes(DeleteAccountOtpExpiryMinutes);
        user.PendingOtpCodePurpose = OtpPurpose.AccountDeletion;
        await _userRepository.UpdateAsync(user, ct: ct);

        await _emailService.SendAccountDeletionOtpAsync(user.Email, otpCode, ct);
        return new MessageResponse("Hesap silme onay kodu gönderildi.");
    }

    // AMAÇ: OTP + şifre ile hesap silmeyi onaylar; soft delete yapar, 30 gün sonra
    //       kalıcı anonimleştirme için zamanlar (AccountCleanupBackgroundService, A-10).
    // NEDEN: Geri alınamaz bir işlem olduğu için OTP'ye ek olarak şifre de istenir (çift onay).
    public async Task<MessageResponse> ConfirmAccountDeletionAsync(
        int userId,
        DeleteAccountConfirmRequest request,
        CancellationToken ct = default
    )
    {
        var user =
            await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new EntityNotFoundException(typeof(User), userId);

        ValidateOtp(user, request.OtpCode, OtpPurpose.AccountDeletion);

        if (!_passwordService.Verify(request.Password, user.PasswordHash ?? FakePasswordHashForTiming))
            throw new InvalidCredentialsException();

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.ScheduledDeletionAt = DateTime.UtcNow.AddDays(AccountDeletionGraceDays);
        ClearOtp(user);
        await _userRepository.UpdateAsync(user, ct: ct);

        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, ct);

        return new MessageResponse("Hesabınız silindi. 30 gün içinde tekrar giriş yaparak geri alabilirsiniz.");
    }

    // ═══ Yardımcı Metotlar ═══

    // AMAÇ: 6 haneli rastgele OTP kodu + DB'ye yazılacak hash'ini üretir.
    // NEDEN: RandomNumberGenerator kriptografik olarak güvenli rastgelelik sağlar
    //        (Random sınıfı tahmin edilebilir olduğu için OTP üretiminde kullanılmaz).
    private (string Code, string Hash) GenerateOtp()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        return (code, _passwordService.HashToken(code));
    }

    // AMAÇ: Bir kullanıcının bekleyen OTP'sini amaç/hash/süre bakımından doğrular.
    // NEDEN: EmailVerification/LoginOtp/PasswordReset/AccountDeletion akışlarının HEPSİ
    //        aynı doğrulama mantığını paylaşır — kod tekrarını önler.
    //        NOT: "3 yanlış deneme → kod geçersiz" sayacı SecurityLog'a bağımlı olduğu
    //        için A-04 (loglama) tamamlandıktan sonra buraya entegre edilecek — bkz.
    //        TASK/A_admin_panel_backend.md A-03'ün sonundaki not.
    private void ValidateOtp(User? user, string otpCode, OtpPurpose expectedPurpose)
    {
        var isValid =
            user is not null
            && user.PendingOtpCodePurpose == expectedPurpose
            && user.PendingOtpCodeExpiresAt is not null
            && user.PendingOtpCodeExpiresAt >= DateTime.UtcNow
            && user.PendingOtpCodeHash == _passwordService.HashToken(otpCode);

        if (!isValid)
            throw new InvalidOtpException();
    }

    // AMAÇ: Kullanılan/süresi dolan bir OTP'nin alanlarını temizler.
    private static void ClearOtp(User user)
    {
        user.PendingOtpCodeHash = null;
        user.PendingOtpCodeExpiresAt = null;
        user.PendingOtpCodePurpose = null;
    }

    // AMAÇ: OTP doğrulama/Google/Apple girişlerinin ORTAK son adımı — grace period
    //       kurtarma, anonimleştirme kontrolü, giriş istatistikleri ve token üretimi.
    // NEDEN: Üç farklı giriş yönteminin (OTP, Google, Apple) hepsi aynı noktada
    //        birleşir; kod tekrarını önler (SECURITY.md §1 — Adım 2 mantığı).
    private async Task<AuthTokenResponse> CompleteLoginAsync(
        User user,
        string? ipAddress,
        CancellationToken ct
    )
    {
        if (user.IsAnonymized)
            throw new AccountAnonymizedException();

        var accountWasRecovered = false;
        if (user.IsDeleted)
        {
            // NEDEN: 30 günlük grace period içinde soft-delete'li bir hesap otomatik
            //        kurtarılır (REFERENCE/SECURITY.md §1).
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.ScheduledDeletionAt = null;
            accountWasRecovered = true;
        }

        ClearOtp(user);
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIP = ipAddress;
        user.LoginCount += 1;
        await _userRepository.UpdateAsync(user, ct: ct);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenResult = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _passwordService.HashToken(refreshTokenResult.Token),
            TokenFamily = Guid.NewGuid().ToString(),
            ExpiresAt = refreshTokenResult.ExpiresAt,
            IpAddress = ipAddress,
        };
        await _refreshTokenRepository.AddAsync(refreshToken, ct: ct);

        return new AuthTokenResponse(
            accessToken,
            refreshTokenResult.Token,
            ExpiresInSeconds(),
            new AuthUserDto(user.Id, user.CurrentLevel),
            accountWasRecovered
        );
    }

    // AMAÇ: appsettings.json'daki Jwt:ExpirationMinutes'i saniyeye çevirir.
    // NEDEN: AuthTokenResponse.ExpiresIn saniye cinsinden döner (REFERENCE/API_ENDPOINTS.md §3 örneği).
    private int ExpiresInSeconds() => _configuration.GetValue("Jwt:ExpirationMinutes", 15) * 60;
}
