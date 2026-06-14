/// <summary>
/// UserDto.cs
///
/// AMAÇ: Kimlik doğrulama yanıtlarında döndürülen kullanıcı profil verisi.
/// NEDEN: User entity'sini doğrudan döndürmek yerine yalnızca gerekli alanlar seçilir.
///        PasswordHash, GoogleId gibi hassas alanlar istemciye hiç gitmez.
/// BAĞIMLILIKLAR: AuthResponse (içinde gömülü olarak kullanılır)
/// </summary>

namespace WordLearner.Application.DTOs.Auth;

/// <summary>
/// Kimlik doğrulama yanıtlarındaki kullanıcı profili.
///
/// AMAÇ: İstemcinin göstereceği temel kullanıcı bilgilerini taşımak.
/// NEDEN: Entity'yi doğrudan serialize etmek hassas alan sızıntısına yol açar.
/// </summary>
public class UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    /// <summary>NULL ise mobil FirstName + LastName birleştirerek gösterir.</summary>
    public string? DisplayName { get; init; }

    public string? AvatarUrl { get; init; }

    /// <summary>User | Instructor | Admin</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>A1 | A2 | B1 | B2 | C1 | C2</summary>
    public string CurrentLevel { get; init; } = string.Empty;

    public int TotalXP { get; init; }
    public int StreakDays { get; init; }

    /// <summary>FALSE ise mobil onboarding akışını başlatır.</summary>
    public bool IsOnboardingCompleted { get; init; }

    /// <summary>Local | Google | Apple</summary>
    public string AuthProvider { get; init; } = string.Empty;
}
