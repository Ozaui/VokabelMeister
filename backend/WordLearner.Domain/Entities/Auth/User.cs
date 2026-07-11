// ─────────────────────────────────────────────────────────────────────────────
// User.cs
//
// AMAÇ: Uygulamaya kayıtlı her kullanıcının hesap bilgilerini tutan entity.
// NEDEN: Kimlik doğrulama (yerel şifre + Google/Apple), 2 adımlı OTP akışları,
//        öğrenme istatistikleri ve GDPR/KVKK uyumlu hesap silme süreci tek bir
//        tabloda toplanır — DATABASE_SCHEMA/Auth.md'deki Users şemasının kod karşılığı.
// BAĞIMLILIKLAR: BaseEntity, OtpPurpose enum, RefreshToken (1:N ilişki).
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

public class User : BaseEntity
{
    // AMAÇ: Kullanıcının benzersiz giriş adresi.
    public string Email { get; set; } = string.Empty;

    // AMAÇ: BCrypt hash'i (60 karakter). Sosyal girişte (Google/Apple) null kalır.
    public string? PasswordHash { get; set; }

    // AMAÇ: Google Sign-In ile eşleşen benzersiz kullanıcı kimliği.
    public string? GoogleId { get; set; }

    // AMAÇ: Apple Sign-In ile eşleşen benzersiz kullanıcı kimliği.
    public string? AppleId { get; set; }

    // AMAÇ: Hesabın hangi yöntemle oluşturulduğu — geçerli değerler: Local, Google, Apple.
    // NEDEN: Enum yerine string tutulur çünkü [Authorize] claim'leri gibi doğrudan
    //        dışarıya (JWT/DTO) yansıyan bir alan değil, yalnızca DB CHECK constraint'i var.
    public string AuthProvider { get; set; } = "Local";

    // AMAÇ: Kullanıcının adı.
    public string FirstName { get; set; } = string.Empty;

    // AMAÇ: Kullanıcının soyadı.
    public string LastName { get; set; } = string.Empty;

    // AMAÇ: Profilde gösterilecek takma ad (opsiyonel).
    public string? DisplayName { get; set; }

    // AMAÇ: Profil fotoğrafının URL'i.
    public string? AvatarUrl { get; set; }

    // AMAÇ: Günlük hedeflenen kelime tekrar/öğrenme sayısı.
    public int DailyWordGoal { get; set; } = 10;

    // AMAÇ: Günlük öğrenilecek YENİ kelime limiti (kalanı SRS tekrarına ayrılır).
    public int DailyNewWordLimit { get; set; } = 5;

    // AMAÇ: Kullanıcının mevcut seviyesi — geçerli değerler: A1, A2, B1, B2, C1, C2.
    public string CurrentLevel { get; set; } = "A1";

    // AMAÇ: Arayüz tema tercihi — geçerli değerler: Light, Dark, System.
    // NEDEN: CurrentLevel ile aynı desen — register'da toplanmaz (kayıt anonim, henüz JWT yok),
    //        kayıt sonrası ilk-login-sonrası onboarding'de (LevelSelectPage ile aynı ekran/an,
    //        PUT /users/me — C-01) set edilir. JWT claim'ine hiç girmez (yetki bilgisi değil).
    public string ThemePreference { get; set; } = "System";

    // AMAÇ: Mevcut seviye içindeki toplanan XP.
    public int TotalXP { get; set; }

    // AMAÇ: Hesap boyunca kazanılan toplam XP (seviye sıfırlansa bile azalmaz).
    public int LifetimeXP { get; set; }

    // AMAÇ: Art arda kaç gündür aktif olunduğu (streak).
    public int StreakDays { get; set; }

    // AMAÇ: Streak'in en son arttığı gün — bir sonraki günün streak'i bozup bozmadığını belirler.
    public DateTime? LastStreakDate { get; set; }

    // AMAÇ: Bekleyen OTP kodunun SHA-256 hash'i. Plaintext asla saklanmaz.
    public string? PendingOtpCodeHash { get; set; }

    // AMAÇ: Bekleyen OTP kodunun geçerlilik süresinin dolacağı an (UTC).
    public DateTime? PendingOtpCodeExpiresAt { get; set; }

    // AMAÇ: Bekleyen OTP kodunun hangi işlem için üretildiği.
    // NEDEN: Aynı alan seti (hash/expiry) tüm OTP akışlarında paylaşıldığı için
    //        "bu kod neyi doğrulamak içindi" sorusu bu alan olmadan cevaplanamaz.
    public OtpPurpose? PendingOtpCodePurpose { get; set; }

    // AMAÇ: İlk kurulum (onboarding) akışının tamamlanıp tamamlanmadığı.
    public bool IsOnboardingCompleted { get; set; }

    // AMAÇ: Hesabın dondurulup dondurulmadığı (admin tarafından).
    public bool IsActive { get; set; } = true;

    // AMAÇ: E-posta adresinin doğrulanıp doğrulanmadığı.
    public bool IsEmailVerified { get; set; }

    // AMAÇ: E-postanın doğrulandığı an (UTC).
    public DateTime? EmailVerifiedAt { get; set; }

    // AMAÇ: En son başarılı girişin yapıldığı an (UTC).
    public DateTime? LastLoginAt { get; set; }

    // AMAÇ: En son başarılı girişin yapıldığı IP adresi.
    public string? LastLoginIP { get; set; }

    // AMAÇ: Toplam başarılı giriş sayısı.
    public int LoginCount { get; set; }

    // AMAÇ: Hesap silme isteği onaylandığında, kalıcı anonimleştirmenin planlandığı an
    //       (istek anından 30 gün sonrası — AccountCleanupBackgroundService bu tarihi kullanır).
    public DateTime? ScheduledDeletionAt { get; set; }

    // AMAÇ: 30 günlük grace period sonunda PII'nin gerçekten anonimleştirilip anonimleştirilmediği.
    public bool IsAnonymized { get; set; }

    // AMAÇ: Anonimleştirilmeden önceki e-postanın SHA-256 hash'i.
    // NEDEN: Silinen bir e-posta ile tekrar kayıt açılmasını engellemek için —
    //        gerçek e-posta anonimleştirildikten sonra bu hash tek referans kalır.
    public string? OriginalEmailHash { get; set; }

    // AMAÇ: OneSignal push bildirim hedefleme kimliği.
    public string? OneSignalPlayerId { get; set; }

    // AMAÇ: Yetkilendirme rolü — geçerli değerler: User, Admin.
    // NEDEN: JWT claim'ine (ClaimTypes.Role) doğrudan string olarak yazılır;
    //        [Authorize(Roles="Admin")] ASP.NET Core'da string karşılaştırması yapar.
    public string Role { get; set; } = "User";

    // AMAÇ: Kullanıcının refresh token geçmişi (login/refresh/logout akışlarında kullanılır).
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
