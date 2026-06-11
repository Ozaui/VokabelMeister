/// <summary>
/// RefreshToken.cs
///
/// AMAÇ:
///   JWT refresh token'larını veritabanında güvenli biçimde saklar.
///   Token Family Pattern ile yeniden kullanım saldırılarını (replay attack) tespit eder.
///
/// NEDEN:
///   Access token'lar kısa ömürlüdür (15 dk). Kullanıcının her 15 dakikada
///   şifresini girmesini önlemek için refresh token sistemi kullanılır.
///   Token hash olarak saklanır — düz metin asla yazılmaz (güvenlik).
///
/// TOKEN FAMILY PATTERN:
///   Her refresh işleminde aynı Family GUID'i taşıyan yeni token üretilir.
///   Eski token kullanılmaya çalışılırsa (TokenFamily eşleşmesi) tüm aile iptal edilir
///   → Çalınmış token tespiti.
///
/// BAĞIMLILIKLAR:
///   - User (N:1 — bir kullanıcının birden fazla aktif token'ı olabilir, farklı cihazlar)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Refresh token entity'si — BaseEntity'den miras almaz (soft delete yoktur, süresi dolunca silinir).
///
/// AMAÇ: JWT yenileme için kriptografik token'ları güvenli saklamak.
/// NEDEN: Token düz metin saklanırsa veritabanı sızıntısında tüm tokenlar ifşa olur.
/// </summary>
public class RefreshToken
{
    /// <summary>Birincil anahtar — otomatik artan</summary>
    public int Id { get; set; }

    /// <summary>Token sahibi kullanıcının ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>
    /// SHA-256 ile hashlenmiş token değeri (max 88 karakter — Base64 encoded).
    /// NEDEN HASH: Veritabanı sızıntısında token'lar kullanılamaz hale gelir.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Token ailesi GUID'i — Token Family Pattern için.
    /// NASIL ÇALIŞIR: Bir refresh işleminde yeni token aynı Family GUID'ini alır.
    ///               Eski token ile yenileme yapılmaya çalışılırsa bu aile tamamen iptal edilir.
    /// </summary>
    public string TokenFamily { get; set; } = string.Empty;

    /// <summary>
    /// Token'ın geçerlilik bitiş tarihi (UTC).
    /// VARSAYILAN: Oluşturulma + 7 gün (appsettings.json'dan okunur).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token kullanıldı mı? Bir kez kullanılan token geçersizleşir.
    /// NEDEN: Her refresh işlemi yeni token üretir — eski token tekrar kullanılamaz.
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Token'ın iptal edildiği tarih (UTC). NULL ise henüz iptal edilmemiş.
    /// NEDEN: Çıkış, şifre değişikliği veya şüpheli aktivitede tüm tokenlar iptal edilir.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Token'ın hangi cihazdan oluşturulduğu bilgisi (User-Agent vb.)</summary>
    public string? DeviceInfo { get; set; }

    /// <summary>Token'ın oluşturulduğu IP adresi (IPv4/IPv6, max 45 karakter)</summary>
    public string? IpAddress { get; set; }

    /// <summary>Token oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Token sahibi kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;
}
