using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    // Sosyal girişte (Google/Apple) null kalır.
    public string? PasswordHash { get; set; }

    public string? GoogleId { get; set; }
    public string? AppleId { get; set; }

    // Local/Google/Apple — enum değil string, çünkü yalnızca DB CHECK constraint'i var, dışarı (JWT/DTO) yansımıyor.
    public string AuthProvider { get; set; } = "Local";

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public int DailyWordGoal { get; set; } = 10;
    public int DailyNewWordLimit { get; set; } = 5;

    // A1-C2.
    public string CurrentLevel { get; set; } = "A1";

    // Light/Dark/System — kayıt sonrası onboarding'de set edilir (C-01), JWT claim'ine girmez.
    public string ThemePreference { get; set; } = "System";

    // tr/de — istemcinin (admin panel, ileride web/mobil) arayüz dili; Languages tablosundaki
    // kelime içeriği diliyle KARIŞTIRILMAMALI (o WordConcept'e bağlı, bu kullanıcıya). ThemePreference
    // ile aynı desen: yazma ucu C-01'de (PUT /users/me), şimdilik yalnızca login yanıtından okunur.
    public string LanguagePreference { get; set; } = "tr";

    public int TotalXP { get; set; }
    public int LifetimeXP { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastStreakDate { get; set; }
    public string? PendingOtpCodeHash { get; set; }
    public DateTime? PendingOtpCodeExpiresAt { get; set; }

    // Aynı hash/expiry alan seti tüm OTP akışlarında paylaşıldığı için hangi işlem olduğunu ayırt eder.
    public OtpPurpose? PendingOtpCodePurpose { get; set; }

    public bool IsOnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIP { get; set; }
    public int LoginCount { get; set; }

    // Hesap silme isteği onaylandığında, kalıcı anonimleştirmenin planlandığı an (istek + 30 gün).
    public DateTime? ScheduledDeletionAt { get; set; }

    public bool IsAnonymized { get; set; }

    // Silinen bir e-postayla tekrar kayıt açılmasını engellemek için, gerçek e-posta anonimleştirildikten sonra tek referans.
    public string? OriginalEmailHash { get; set; }

    public string? OneSignalPlayerId { get; set; }

    // User/Admin — JWT claim'ine (ClaimTypes.Role) doğrudan string olarak yazılır.
    public string Role { get; set; } = "User";

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
