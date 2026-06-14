/// <summary>
/// CategoryRepository.cs
///
/// AMAÇ: Sistem kategorisi sorgularının implementasyonu — hiyerarşi ve seviye filtresi.
/// NEDEN: Self-referencing entity için SubCategories eager loading özel sorgu gerektirir.
/// BAĞIMLILIKLAR: Repository&lt;T&gt; (base), ICategoryRepository (Application), WordLearnerDbContext
/// </summary>

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories.Vocabulary;

/// <summary>
/// Sistem kategori repository implementasyonu.
///
/// AMAÇ: ICategoryRepository sözleşmesini karşılamak.
/// NEDEN: Ağaç yapısında listeleme ve seviye filtresi generic CRUD'a ek operasyonlar.
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(WordLearnerDbContext db) : base(db) { }

    /// <summary>
    /// AMAÇ: Tüm kök kategorileri alt kategorileriyle birlikte getirir.
    /// NEDEN: Kategori ekranı ağaç görünüm için hem üst hem alt kategoriler ister.
    /// NASIL: ParentCategoryId=null filtresi kök kategorileri ayırır; SubCategories eager load edilir.
    /// </summary>
    public async Task<IEnumerable<Category>> GetHierarchyAsync(CancellationToken ct = default)
        => await _set
            .Where(c => c.ParentCategoryId == null)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    /// <summary>
    /// AMAÇ: Belirli bir seviyeyle eşleşen kategorileri getirir.
    /// NEDEN: Kullanıcı seviyesine uygun kategoriler filtrelenir — A1 kullanıcısına C1 kategorisi gösterilmez.
    /// NASIL: CEFR seviyeleri sıralanabilir string — basit eşitlik karşılaştırması yeterli.
    ///        MinLevel ve MaxLevel null ise o filtre uygulanmaz (her seviyeye uygun).
    /// </summary>
    public async Task<IEnumerable<Category>> GetByLevelAsync(string level, CancellationToken ct = default)
        => await _set
            .Where(c =>
                (c.MinLevel == null || string.Compare(c.MinLevel, level) <= 0) &&
                (c.MaxLevel == null || string.Compare(c.MaxLevel, level) >= 0))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
}
