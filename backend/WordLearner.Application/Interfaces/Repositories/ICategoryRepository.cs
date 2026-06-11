/// <summary>
/// ICategoryRepository.cs
///
/// AMAÇ: Sistem kategori sorgularını tanımlar — hiyerarşik yapı ve kelime sayımı.
/// NEDEN: Self-referencing entity için özel sorgular gerekir.
/// BAĞIMLILIKLAR: IRepository (generic), Category entity
/// </summary>

using WordLearner.Domain.Entities;

namespace WordLearner.Application.Interfaces.Repositories;

/// <summary>
/// Sistem kategori repository arayüzü.
///
/// AMAÇ: Hiyerarşik kategori listeleme ve kelime sayısı sorgularını tanımlamak.
/// NEDEN: Alt kategorilerle birlikte yükleme ve kelime sayımı generic CRUD'un ötesinde sorgular gerektirir.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// AMAÇ: Tüm üst kategorileri alt kategorileriyle birlikte getirir.
    /// NEDEN: Kategori ekranı ağaç yapısında gösterim yapar.
    /// NASIL: Sadece ParentCategoryId=null olanlar kök olarak alınır; SubCategories eager load edilir.
    /// </summary>
    Task<IEnumerable<Category>> GetHierarchyAsync(CancellationToken ct = default);

    /// <summary>
    /// AMAÇ: Seviyeye göre filtrelenmiş kategorileri getirir.
    /// NEDEN: Kullanıcı seviyesine (A1, B2 vb.) uygun kategoriler filtrelenir.
    /// NASIL: MinLevel &lt;= level &lt;= MaxLevel koşuluyla karşılaştırma yapılır.
    /// </summary>
    Task<IEnumerable<Category>> GetByLevelAsync(string level, CancellationToken ct = default);
}
