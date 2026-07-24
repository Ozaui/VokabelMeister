namespace WordLearner.Application.DTOs.Admin;

public record AdminUserListItemDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record AdminUserDetailDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    string CurrentLevel,
    string ThemePreference,
    string AuthProvider,
    int TotalXP,
    int LifetimeXP,
    int StreakDays,
    int LoginCount,
    DateTime? LastLoginAt,
    string? LastLoginIP,
    DateTime CreatedAt
);

public record AdminDailyCountDto(DateOnly Date, int Count);

// LoginsByDay yok — Users.LastLoginAt yalnızca en son girişin üzerine yazılan tek bir alan,
// bir login-event geçmişi tablosu olmadığı için "her gün kaç login oldu" hesaplanamaz.
public record AdminStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int FrozenUsers,
    int TotalWordConcepts,
    int TotalCategories,
    IReadOnlyList<AdminDailyCountDto> RegistrationsByDay
);
