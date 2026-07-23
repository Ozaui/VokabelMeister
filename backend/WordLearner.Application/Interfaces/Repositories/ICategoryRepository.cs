// ─────────────────────────────────────────────────────────────────────────────
// ICategoryRepository.cs
//
// AMAÇ: Category aggregate root'una özel sorgular — hiyerarşik liste, tüm
//       dilleriyle detay, silme koruması kontrolleri (alt kategori/aktif kelime).
// NEDEN: CategoryTranslation için AYRI top-level bir repository AÇILMAZ — A-05'teki
//        IWordConceptRepository/Word ilişkisiyle AYNI aggregate root kararı
//        (IWordConceptRepository.cs "NEDEN" açıklamasıyla birebir gerekçe, YAGNI).
// BAĞIMLILIKLAR: IRepository<Category>.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    // AMAÇ: `GET /categories` için TÜM (silinmemiş) kategorilerin düz listesi —
    //       Translations+Language dahil. Ağaç, bu düz listeden BELLEKTE (CategoryDtoBuilder)
    //       kurulur; sınırsız derinlikte bir hiyerarşiyi tek sorguda Include zinciriyle
    //       çekmenin EF Core'da doğrudan bir yolu olmadığı için (ParentCategory'yi N kez
    //       zincirlemek gerekirdi) bu, kategori sayısı küçük olduğu sürece (onlarca,
    //       binlerce değil) en basit ve doğru yaklaşım.
    // NEDEN level parametresi burada (imzada) ama SQL'de DEĞİL: bir kategori, `level` NULL
    //       ise ya da (MinLevel NULL VEYA MinLevel<=level) VE (MaxLevel NULL VEYA
    //       level<=MaxLevel) ise eşleşir — ama bu karşılaştırma DB'ye WHERE olarak
    //       GÖNDERİLMEZ, tüm liste çekildikten SONRA bellekte uygulanır (bkz.
    //       CategoryRepository.cs implementasyonundaki "NEDEN bellekte filtre" notu:
    //       string.Compare/CompareTo EF Core'un SQL Server sağlayıcısında GÜVENİLİR
    //       şekilde SQL'e çevrilmez). Parametre yine de burada, arayüzde durur çünkü
    //       filtrenin KİM tarafından uygulandığı (repository) tüketiciyi (GetCategoriesQuery)
    //       ilgilendirmez — yalnızca sonucun DOĞRU filtrelenmiş olması ilgilendirir.
    Task<IReadOnlyList<Category>> GetAllWithTranslationsAsync(string? level, CancellationToken ct = default);

    // AMAÇ: Detay/güncelleme — tek bir kategoriyi Translations'ıyla birlikte yükler.
    Task<Category?> GetWithTranslationsAsync(int id, CancellationToken ct = default);

    // AMAÇ: `includeWordCount=true` olduğunda her kategorinin bağlı AKTİF (soft-delete
    //       edilmemiş WordConcept'e ait) WordCategory sayısını döner — yalnızca
    //       istendiğinde hesaplanır, her `GET /categories`'de gereksiz JOIN yapılmaz.
    Task<IReadOnlyDictionary<int, int>> GetWordCountsAsync(CancellationToken ct = default);

    // AMAÇ: `categoryId`'nin en az bir alt kategorisi (soft-delete edilmemiş) var mı —
    //       DeleteCategoryCommand'ın 409 koruması (CategoryHasChildrenException).
    Task<bool> HasChildrenAsync(int categoryId, CancellationToken ct = default);

    // AMAÇ: `categoryId`'ye bağlı en az bir aktif WordConcept var mı — DeleteCategoryCommand'ın
    //       409 koruması (CategoryHasActiveWordsException).
    Task<bool> HasActiveWordsAsync(int categoryId, CancellationToken ct = default);

    // AMAÇ: `categoryId`'nin ParentCategoryId'sini `newParentId` yapmak bir DÖNGÜ yaratır mı —
    //       yani newParentId, categoryId'nin KENDİSİ veya alt ağacındaki bir kategori mi?
    //       UpdateCategoryCommandHandler'ın CategoryParentCycleException koruması.
    Task<bool> WouldCreateCycleAsync(int categoryId, int newParentId, CancellationToken ct = default);
}
