using WordLearner.Domain.Enums.Auth;

namespace WordLearner.Domain.Entities.Auth;

// BaseEntity'den BİLİNÇLİ olarak türemez — "kim yaptı" alanları (CreatedByUserId vb.) self-servis
// kayıtta anlamsız (bir User'ı başka biri "oluşturmaz"); DATABASE_SCHEMA/Auth.md'deki Users
// tablosuyla birebir eşleşir.
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? GoogleId { get; set; }
    public string? AppleId { get; set; }
    public string AuthProvider { get; set; } = "Local";
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }

    public int DailyWordGoal { get; set; } = 10;
    public int DailyNewWordLimit { get; set; } = 5;

    public string CurrentLevel { get; set; } = "A1";
    public string ThemePreference { get; set; } = "System";
    public string LanguagePreference { get; set; } = "tr";
    public int TotalXP { get; set; }
    public int LifetimeXP { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastStreakDate { get; set; }

    public string? PendingOtpCodeHash { get; set; }
    public DateTime? PendingOtpCodeExpiresAt { get; set; }
    public OtpPurpose? PendingOtpCodePurpose { get; set; }
    public int PendingOtpCodeAttempts { get; set; }

    public bool IsOnboardingCompleted { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIP { get; set; }
    public int LoginCount { get; set; }

    public DateTime? ScheduledDeletionAt { get; set; }
    public bool IsAnonymized { get; set; }
    public string? OriginalEmailHash { get; set; }
    public string? OneSignalPlayerId { get; set; }

    public string Role { get; set; } = "User";

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
