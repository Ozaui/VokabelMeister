// ─────────────────────────────────────────────────────────────────────────────
// Language.cs
//
// AMAÇ: Sistemde desteklenen dillerin (şu an de/tr) referans listesi.
// NEDEN: BaseEntity'den TÜRETİLMEZ — bu bir audit gerektiren içerik kaydı değil,
//        sabit/seed bir referans tablosu (yeni dil eklemek CLAUDE.md'ye göre tek
//        satırlık bir INSERT, migration bile gerekmez); soft delete/CreatedBy gibi
//        alanlar burada anlamsız (bkz. DATABASE_SCHEMA/Icerik.md Languages şeması).
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Domain.Entities.Words;

public class Language
{
    public int Id { get; set; }

    // AMAÇ: ISO 639-1 dil kodu (ör. "de", "tr").
    public string Code { get; set; } = string.Empty;

    // AMAÇ: Dilin İngilizce adı (ör. "German").
    public string Name { get; set; } = string.Empty;

    // AMAÇ: Dilin kendi dilindeki adı (ör. "Deutsch").
    public string NativeName { get; set; } = string.Empty;

    // AMAÇ: Dilin aktif olup olmadığı — pasife alınan bir dil yeni içerikte kullanılmaz.
    public bool IsActive { get; set; } = true;

    // AMAÇ: Listeleme sırası (ör. admin panelde dil seçim dropdown'u).
    public int DisplayOrder { get; set; }
}
