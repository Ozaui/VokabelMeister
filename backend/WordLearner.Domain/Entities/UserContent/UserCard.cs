/// <summary>
/// UserCard.cs
///
/// AMAÇ:
///   Kullanıcının kendisi için oluşturduğu kişisel flash kartları temsil eder.
///   Sistem kelimeleri (Words) gibi dil kısıtı yoktur — her içerik girilebilir.
///
/// NEDEN:
///   Sistem kelimelerine ek olarak kullanıcılar kendi kart havuzlarını oluşturabilir.
///   Kişisel öğrenme materyalleri merkezi kontrole girmeden yönetilir.
///   Her kart sadece sahibine aittir — gizlilik ve güvenlik zorunludur.
///
/// GÜVENLİK NOTU:
///   Repository katmanında her sorgu UserId filtresi içermek ZORUNDADIR.
///   Kullanıcı başkasının kartına hiçbir şekilde erişememeli.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - User (N:1 — kart sahibi)
///   - UserCardExample (1:N — karta ait örnek cümleler)
///   - UserCardProgress (1:N — SRS ilerlemesi)
///   - UserCardCategory (M:N — sistem kategorisi bağlantısı)
///   - UserCardUserCategory (M:N — kişisel kategori bağlantısı)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kişisel flash kart entity'si.
///
/// AMAÇ: Kullanıcının oluşturduğu özel öğrenme kartlarını saklamak.
/// NEDEN: Sistem kelimelerinin yanına kişisel içerik ekleme özgürlüğü.
/// GÜVENLİK: Sadece kart sahibi erişebilir — repository katmanında zorunlu filtre.
/// </summary>
public class UserCard : BaseEntity
{
    /// <summary>Kart sahibi kullanıcının ID'si (FK → Users)</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Kartın ön yüzü (max 500 karakter) — örn: Almanca kelime.
    /// NOT: Dil kısıtı yoktur, kullanıcı istediği içeriği girebilir.
    /// </summary>
    public string FrontText { get; set; } = string.Empty;

    /// <summary>
    /// Kartın arka yüzü (max 500 karakter) — örn: Türkçe çeviri.
    /// </summary>
    public string BackText { get; set; } = string.Empty;

    /// <summary>Ek notlar, ipuçları veya hafıza kancaları (serbest metin)</summary>
    public string? Notes { get; set; }

    /// <summary>Görsel yardımcı URL'si (opsiyonel)</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Ses dosyası URL'si (opsiyonel)</summary>
    public string? AudioUrl { get; set; }

    /// <summary>Kart aktif mi? Pasif kartlar öğrenme oturumuna dahil edilmez.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Kart sahibi kullanıcı (N:1)</summary>
    public User User { get; set; } = null!;

    /// <summary>Karta ait örnek cümleler (1:N)</summary>
    public ICollection<UserCardExample> UserCardExamples { get; set; } = new List<UserCardExample>();

    /// <summary>Bu kartın SRS ilerlemesi (1:1 aslında, ama UserId'ye göre ayrılır)</summary>
    public ICollection<UserCardProgress> UserCardProgresses { get; set; } = new List<UserCardProgress>();

    /// <summary>Atanan sistem kategorileri (M:N ara tablo)</summary>
    public ICollection<UserCardCategory> UserCardCategories { get; set; } = new List<UserCardCategory>();

    /// <summary>Atanan kişisel kategoriler (M:N ara tablo)</summary>
    public ICollection<UserCardUserCategory> UserCardUserCategories { get; set; } = new List<UserCardUserCategory>();
}
