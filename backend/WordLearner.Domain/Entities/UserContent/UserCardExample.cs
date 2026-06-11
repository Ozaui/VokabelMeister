/// <summary>
/// UserCardExample.cs
///
/// AMAÇ:
///   Bir kişisel karta (UserCard) ait örnek cümleleri saklar.
///   Ön yüz ve arka yüz dili olmak üzere iki dil desteği sunar.
///
/// NEDEN:
///   Bağlam içinde öğrenme için kullanıcı kendi cümlelerini ekleyebilir.
///   Sistem kelimelerindeki WordExample'dan farklı olarak seviye/tür ayrımı yoktur
///   çünkü kişisel içerik standartsızdır.
///
/// BAĞIMLILIKLAR:
///   - UserCard (N:1 — hangi karta ait)
/// </summary>

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kişisel kart örnek cümle entity'si.
///
/// AMAÇ: Kullanıcının kendi kartına eklediği örnek cümleleri saklamak.
/// NEDEN BaseEntity'den miras almaz: UserCard silinince CASCADE ile silinir.
/// </summary>
public class UserCardExample
{
    /// <summary>Birincil anahtar</summary>
    public int Id { get; set; }

    /// <summary>Bağlı kişisel kartın ID'si (FK → UserCards)</summary>
    public int UserCardId { get; set; }

    /// <summary>
    /// Ön yüz dilinde örnek cümle (FrontText ile aynı dil).
    /// ÖRNEK: Kart Almanca kelime içeriyorsa Almanca cümle buraya gelir.
    /// </summary>
    public string SentenceFront { get; set; } = string.Empty;

    /// <summary>
    /// Arka yüz dilinde örnek cümle (BackText ile aynı dil).
    /// ÖRNEK: Kart Türkçe çeviri içeriyorsa Türkçe cümle buraya gelir.
    /// </summary>
    public string SentenceBack { get; set; } = string.Empty;

    /// <summary>Gösterim sırası — küçük değer önce gösterilir.</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Oluşturulma tarihi (UTC)</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncelleme tarihi (UTC)</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Bağlı kişisel kart (N:1)</summary>
    public UserCard UserCard { get; set; } = null!;
}
