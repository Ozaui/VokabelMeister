/// <summary>
/// IAuthService.cs
///
/// AMAÇ: Tüm kimlik doğrulama ve hesap yönetimi operasyonlarının sözleşmesi.
/// NEDEN: Controller'lar doğrudan repository veya token servise erişmez;
///        bu arayüz aracılığıyla iş mantığına ulaşır — bağımlılık tersine çevrilir.
///
/// AKIŞ ÖZETİ:
///
///   [Kayıt]
///     POST /auth/register → e-posta OTP gönderilir → giriş KAPALI
///     POST /auth/verify-email → OTP doğrula → giriş AÇIK
///     POST /auth/resend-verification → yeni OTP gönder (doğrulanmamışsa)
///
///   [Local Giriş — 2FA Zorunlu]
///     POST /auth/login → şifre + e-posta doğrulama kontrolü → login OTP gönder
///     POST /auth/login/verify-otp → OTP doğrula → token döndür
///       ↳ Hesap grace period içindeyse → otomatik kurtarılır, AccountWasRecovered=true
///       ↳ Hesap kalıcı silinmişse (IsAnonymized=true) → 403, "Bu hesap kalıcı silinmiştir"
///
///   [Google / Apple Giriş — 2FA Gereksiz]
///     POST /auth/google → Google token doğrula → token döndür (OTP yok)
///     POST /auth/apple  → Apple token doğrula → token döndür (OTP yok)
///
///   [Şifre Sıfırlama]
///     POST /auth/forgot-password → 5 dk OTP gönder
///     POST /auth/reset-password  → OTP + yeni şifre → şifreyi güncelle
///
///   [Token Yönetimi]
///     POST /auth/refresh → Token Family Pattern → yeni token çifti
///     POST /auth/logout  → refresh token'ı iptal et
///
///   [Hesap Silme — Grace Period 30 Gün]
///     POST /auth/delete-account/request → 15 dk OTP gönder
///     POST /auth/delete-account/confirm → OTP + şifre → soft delete + 30 gün zamanla
///       ↳ 30 gün içinde giriş yapılırsa → otomatik kurtarma (LoginVerifyOtpAsync içinde)
///       ↳ 30 gün sonunda arka plan görevi → PII anonimleştir (TASK-018)
///       ↳ Anonimleştirilmiş e-posta → OriginalEmailHash kaydı → asla yeniden kayıt yapılamaz
///
/// BAĞIMLILIKLAR:
///   - IUserRepository, IRefreshTokenRepository
///   - IPasswordService, ITokenService
///   - IEmailService, IAppleTokenValidator
///   - Google.Apis.Auth (GoogleJsonWebSignature)
/// </summary>

using WordLearner.Application.DTOs.Auth;

namespace WordLearner.Application.Interfaces.Services;

/// <summary>
/// Kimlik doğrulama servis arayüzü.
///
/// AMAÇ: Tüm auth iş mantığını tek noktada toplamak.
/// NEDEN: AuthController, token/şifre/e-posta detaylarından bağımsız kalır.
/// </summary>
public interface IAuthService
{
    // ════════════════════════════════════════════════════════
    // KAYIT VE E-POSTA DOĞRULAMA
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Yeni kullanıcı kaydeder ve e-posta doğrulama OTP'si gönderir.
    /// NEDEN: Doğrulanmamış e-posta ile sisteme giriş yapılamaz — sahte kayıtları engeller.
    /// NASIL:
    ///   1. E-posta daha önce kullanılmış mı? (aktif + silinmiş + anonimleştirilmiş hepsi kontrol)
    ///   2. Şifre hashle (BCrypt)
    ///   3. Kullanıcı oluştur (IsEmailVerified=false)
    ///   4. 6 haneli OTP üret → SHA-256 hash DB'ye → ham kod e-postaya
    /// </summary>
    Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Kayıt sonrası gönderilen 6 haneli OTP kodu ile e-postayı doğrular.
    /// NEDEN: Doğrulama olmadan giriş akışı başlamaz.
    /// NASIL: OTP hash'i eşleştir → IsEmailVerified=true, EmailVerifiedAt=şimdi → OTP temizle
    /// </summary>
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Doğrulanmamış kullanıcıya yeni e-posta doğrulama OTP'si gönderir.
    /// NEDEN: İlk OTP 24 saat içinde girilmezse süresi dolabilir.
    /// NASIL: Zaten doğrulanmışsa hata döner; değilse yeni OTP üret ve gönder.
    /// </summary>
    Task ResendVerificationEmailAsync(string email, CancellationToken ct = default);

    // ════════════════════════════════════════════════════════
    // LOCAL GİRİŞ (2 ADIMLI — 2FA)
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Giriş adım 1 — e-posta + şifre doğrula, login OTP gönder.
    /// NEDEN: 2FA'nın birinci faktörü (şifre) burada tamamlanır; token henüz verilmez.
    /// NASIL:
    ///   1. Kullanıcıyı e-posta ile bul (aktif, anonimleştirilmemiş)
    ///   2. Hesap aktif mi? E-posta doğrulanmış mı?
    ///      → Doğrulanmamışsa: yeni verification OTP gönder → EmailNotVerifiedException fırlat
    ///   3. BCrypt ile şifre doğrula
    ///   4. 6 haneli login OTP üret → SHA-256 hash DB'ye → kod e-postaya
    ///
    /// GÜVENLİK: Kullanıcı bulunamazsa da BCrypt.Verify çalıştırılır (sahte hash ile)
    ///           → timing attack önlemi — yanıt süresi sabit kalır.
    /// </summary>
    Task LoginRequestOtpAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Giriş adım 2 — login OTP doğrula, JWT token çifti döndür.
    /// NEDEN: 2FA'nın ikinci faktörü (OTP) burada tamamlanır.
    /// NASIL:
    ///   1. E-posta ile kullanıcı bul
    ///   2. OTP hash eşleştir, süre kontrolü
    ///   3. Hesap grace period içinde mi? (IsDeleted=true, ScheduledDeletionAt>şimdi)
    ///      → EVET: hesabı otomatik kurtar (IsDeleted=false, ScheduledDeletionAt=null)
    ///              kurtarma bildirim e-postası gönder → AuthResponse.AccountWasRecovered=true
    ///      → HAYIR: normal akış
    ///   4. Hesap kalıcı silindi mi? (IsAnonymized=true) → 403
    ///   5. LastLoginAt, LoginCount güncelle → OTP temizle
    ///   6. Access token + refresh token üret ve döndür
    /// </summary>
    Task<AuthResponse> LoginVerifyOtpAsync(
        VerifyLoginOtpRequest request,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken ct = default);

    // ════════════════════════════════════════════════════════
    // SOSYAL GİRİŞ (TEK ADIMLI — 2FA GEREKSİZ)
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Google ID token doğrulayıp JWT döndürür.
    /// NEDEN: Google zaten 2FA sağlıyor — ek OTP gereksiz ve kötü UX.
    /// NASIL:
    ///   1. Google.Apis.Auth ile token doğrula
    ///   2. GoogleId ile kullanıcı var mı? → yoksa e-posta ile ara → yoksa yeni kayıt
    ///   3. İlk Google girişinde IsEmailVerified=true (Google verified)
    ///   4. Grace period kontrolü → kalıcı silindi mi kontrolü
    ///   5. Token döndür
    /// </summary>
    Task<AuthResponse> GoogleLoginAsync(
        GoogleLoginRequest request,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Apple identity token doğrulayıp JWT döndürür.
    /// NEDEN: Apple zaten 2FA sağlıyor — ek OTP gereksiz.
    /// NASIL: Google akışıyla aynı mantık; IAppleTokenValidator ile token doğrulanır.
    ///        Apple ismi yalnızca ilk girişte gönderir — FirstName/LastName kayıt edilir.
    /// </summary>
    Task<AuthResponse> AppleLoginAsync(
        AppleLoginRequest request,
        string? ipAddress = null,
        string? deviceInfo = null,
        CancellationToken ct = default);

    // ════════════════════════════════════════════════════════
    // TOKEN YÖNETİMİ
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Geçerli bir refresh token ile yeni JWT çifti döndürür.
    /// NEDEN: Access token 15 dakikada dolar; yenileme için şifre tekrar girilmez.
    /// NASIL:
    ///   Token Family Pattern:
    ///   1. Gelen token → SHA-256 hash → DB'de bul (IsUsed=false, süresi dolmamış)
    ///   2. Aynı aileden başka IsUsed=true token var mı? → EVET: REPLAY ATTACK → tüm aile iptal
    ///   3. Eski token IsUsed=true yap
    ///   4. Aynı TokenFamily ile yeni token çifti üret
    /// </summary>
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Refresh token'ı sunucuda iptal eder — güvenli çıkış.
    /// NEDEN: Yalnızca istemci taraflı silme yeterli değil; token çalınmışsa hâlâ geçerli kalır.
    /// NASIL: Token hash → DB'de bul → RevokedAt=şimdi
    /// </summary>
    Task LogoutAsync(LogoutRequest request, CancellationToken ct = default);

    // ════════════════════════════════════════════════════════
    // ŞİFRE SIFIRLAMA
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Şifre sıfırlama OTP'sini e-posta ile gönderir.
    /// NEDEN: Kullanıcı şifresini unutunca bağımsız kanaldan (e-posta) doğrulama yapılır.
    /// NASIL:
    ///   1. E-posta ile kullanıcı bul — bulunamazsa da başarı yanıtı döner (enumeration önlemi)
    ///   2. 6 haneli OTP üret (5 dakika geçerli)
    ///   3. PendingOtpCodeHash DB'ye yaz, e-postaya gönder
    /// </summary>
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: OTP kodu doğrulayarak yeni şifre belirler.
    /// NEDEN: E-posta + kod çifti iki faktör sağlar — şifre değiştirme güvenli.
    /// NASIL:
    ///   1. E-posta ile kullanıcı bul
    ///   2. OTP hash eşleştir, süre kontrolü (Purpose="PasswordReset")
    ///   3. Yeni şifreyi hashle, güncelle
    ///   4. OTP temizle, tüm refresh token'ları iptal et (tüm cihazlardan çıkış)
    ///   5. Bildirim e-postası gönder
    /// </summary>
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);

    // ════════════════════════════════════════════════════════
    // HESAP SİLME (GRACE PERIOD 30 GÜN)
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// AMAÇ: Hesap silme OTP'sini e-posta ile gönderir.
    /// NEDEN: Geri dönüşü olmayan işlem öncesi e-posta kanalından ek doğrulama.
    /// NASIL: 6 haneli OTP üret (15 dakika geçerli) → Purpose="AccountDeletion" → e-posta
    /// </summary>
    Task RequestAccountDeletionAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: OTP + şifre doğrulayarak hesabı soft delete + 30 gün zamanlar.
    /// NEDEN: Çift faktör (OTP + şifre) geri dönüşü olmayan işlemi korur.
    /// NASIL:
    ///   1. OTP hash eşleştir (Purpose="AccountDeletion")
    ///   2. Şifre doğrula (Local hesap) — Google/Apple için Password=null geçerli
    ///   3. IsDeleted=true, ScheduledDeletionAt=şimdi+30gün
    ///   4. Tüm refresh token'ları iptal et
    ///   5. OTP temizle
    ///
    /// SONRAKI ADIMLAR (otomatik — TASK-018 background job):
    ///   ScheduledDeletionAt geçince: Email, Ad, Soyad anonimleştirilir
    ///   OriginalEmailHash kaydedilir → o e-posta bir daha kayıt yapamaz
    /// </summary>
    Task ConfirmAccountDeletionAsync(int userId, DeleteAccountRequest request, CancellationToken ct = default);
}
