// ─────────────────────────────────────────────────────────────────────────────
// AdminUserDtos.cs
//
// AMAÇ: `GET /admin/users`, `GET /admin/users/{id}` yanıtlarının DTO'ları.
// NEDEN: Liste ve detay AYNI zenginlikte değil — CategoryDto'nun aksine burada
//        A-05/A-06'daki List/Detail ayrımı geçerli (WordConceptListItemDto/
//        WordConceptDetailDto ile aynı gerekçe): liste satırı yalnızca tarama için
//        yeterli alanları taşır, detay ekranı (B-05) "istatistik" bölümü için
//        LoginCount/StreakDays/TotalXP/LifetimeXP gibi ek alanlar ister.
//        UserCard/UserProgress'e dayalı öğrenme istatistiği (kart sayısı, SRS
//        durumu) BURADA YOK — o tablolar C-02/C-04'te yazılana kadar yok (bkz.
//        TASK/A_admin_panel_backend.md A-07.1 notu); DetailDto yalnızca A-03/A-03.3'te
//        zaten var olan User alanlarını taşır.
// BAĞIMLILIKLAR: Yok (saf DTO'lar).
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.DTOs.Admin;

// AMAÇ: `GET /admin/users` liste satırı.
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

// AMAÇ: `GET /admin/users/{id}` detay + istatistik.
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

// AMAÇ: `GET /admin/statistics` yanıtındaki tek bir günün kaydı sayısı — yalnızca
//       kayıt (registration) grafiği için, bkz. AdminStatisticsDto NEDEN notu.
public record AdminDailyCountDto(DateOnly Date, int Count);

// AMAÇ: `GET /admin/statistics` yanıtı (GetAdminStatisticsQuery'nin gerçek tüketicisi).
// NEDEN LoginsByDay YOK: TASK dosyasının ilk planı "kayıt/login grafiği" diyordu, ama
//       `Users.LastLoginAt` yalnızca EN SON girişin üzerine yazılan TEK bir alan —
//       geçmiş girişlerin bir OLAY GEÇMİŞİ (login history) tablosu YOK, bu yüzden
//       "son 30 günün HER GÜNÜ kaç login oldu" sorusu MEVCUT şemayla cevaplanamaz.
//       Bilinçli kapsam daraltması: yalnızca RegistrationsByDay (Users.CreatedAt'ten
//       güvenilir şekilde türetilebilir) yazıldı, login grafiği A-04/A-07 sonrası bir
//       login-event log'u (ör. SecurityLog'a yeni bir LogEventType) gerektirir — bkz.
//       TASK/A_admin_panel_backend.md A-07 notu.
public record AdminStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int FrozenUsers,
    int TotalWordConcepts,
    int TotalCategories,
    IReadOnlyList<AdminDailyCountDto> RegistrationsByDay
);
