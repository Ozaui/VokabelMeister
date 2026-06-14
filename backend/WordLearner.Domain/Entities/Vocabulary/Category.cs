/// <summary>
/// Category.cs
///
/// AMAÇ:
///   Admin tarafından yönetilen sistem kategorilerini temsil eder.
///   Kelimeleri ve kullanıcı kartlarını gruplandırır (Aile, Renkler, İş, vb.)
///
/// NEDEN:
///   Hiyerarşik yapı (ParentCategoryId self-referencing) sayesinde
///   "Hayvanlar > Evcil Hayvanlar > Köpekler" gibi alt kategoriler oluşturulabilir.
///   Çok dilli isimler (NameDE, NameTR, NameEN) UI dilini değiştirmeyi kolaylaştırır.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, zaman damgaları, soft delete)
///   - Category (self-referencing — üst/alt kategori hiyerarşisi)
///   - WordCategory (M:N ara tablo — hangi kelimeler bu kategoride)
///   - UserCardCategory (M:N ara tablo — hangi kullanıcı kartları bu kategoride)
///   - ClassCategory (M:N ara tablo — hangi sınıflar bu kategoriye atanmış)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Sistem kategorisi entity'si.
///
/// AMAÇ: Kelimeleri ve kartları organize eden hiyerarşik kategoriler.
/// NEDEN: Kullanıcılar belirli bir konuyu (örn: "Aile") çalışmak isteyebilir.
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Almanca kategori adı (max 100 karakter).
    /// ÖRNEK: "Familie", "Essen", "Arbeit"
    /// </summary>
    public string NameDE { get; set; } = string.Empty;

    /// <summary>
    /// Türkçe kategori adı (max 100 karakter).
    /// ÖRNEK: "Aile", "Yemek", "İş"
    /// </summary>
    public string NameTR { get; set; } = string.Empty;

    /// <summary>Türkçe açıklama (opsiyonel)</summary>
    public string? DescriptionTR { get; set; }

    /// <summary>
    /// Üst kategorinin ID'si — NULL ise kök kategoridir.
    /// NEDEN: Hiyerarşik navigasyon ve alt kategori filtrelemesi için.
    /// ÖRNEK: "Evcil Hayvanlar" kategorisinin ParentCategoryId'si "Hayvanlar" kategorisinedir.
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>Görüntüleme sırası — küçük değer önce gösterilir.</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>İkon tanımlayıcısı (font ikon adı veya resim adı)</summary>
    public string? Icon { get; set; }

    /// <summary>Hex renk kodu (örn: "#FF5733") — UI'da kart/badge rengi için</summary>
    public string? Color { get; set; }

    /// <summary>
    /// Bu kategorinin gösterildiği minimum CEFR seviyesi.
    /// NULL ise tüm seviyelerde gösterilir.
    /// </summary>
    public string? MinLevel { get; set; }

    /// <summary>
    /// Bu kategorinin gösterildiği maksimum CEFR seviyesi.
    /// NULL ise üst sınır yoktur.
    /// </summary>
    public string? MaxLevel { get; set; }

    /// <summary>Aktif mi? Pasif kategoriler kullanıcılara gösterilmez.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Üst kategori (N:1, self-referencing)</summary>
    public Category? ParentCategory { get; set; }

    /// <summary>Alt kategoriler (1:N, self-referencing)</summary>
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>Bu kategorideki kelimeler (M:N ara tablo)</summary>
    public ICollection<WordCategory> WordCategories { get; set; } = new List<WordCategory>();

    /// <summary>Bu kategoriye atanan kullanıcı kartları (M:N ara tablo)</summary>
    public ICollection<UserCardCategory> UserCardCategories { get; set; } = new List<UserCardCategory>();

    /// <summary>Bu kategoriye atanan sınıflar (M:N ara tablo)</summary>
    public ICollection<ClassCategory> ClassCategories { get; set; } = new List<ClassCategory>();
}
