// ─────────────────────────────────────────────────────────────────────────────
// CategoryRepository.cs
//
// AMAÇ: ICategoryRepository'nin EF Core implementasyonu.
// NEDEN: Repository<Category>'yi miras alarak genel CRUD'u yeniden yazmadan
//        yalnızca Category aggregate'ine özgü sorguları ekler (WordConceptRepository
//        ile birebir aynı desen, A-05).
// BAĞIMLILIKLAR: EF Core, Repository<Category>, WordLearnerDbContext, Category/
//                CategoryTranslation/WordCategory entity'leri.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Infrastructure.Data;

namespace WordLearner.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(WordLearnerDbContext db)
        : base(db) { }

    public async Task<IReadOnlyList<Category>> GetAllWithTranslationsAsync(string? level, CancellationToken ct = default)
    {
        // NEDEN önce ToListAsync sonra bellekte filtre: MinLevel/MaxLevel karşılaştırması
        // (string.Compare) EF Core'un SQL Server sağlayıcısında GÜVENİLİR şekilde SQL'e
        // çevrilmez (bkz. Repository ICategoryRepository.cs "NEDEN level filtresi burada"
        // notu) — kategori sayısı küçük olduğu için (onlarca) bellekte filtrelemek
        // hem daha güvenli (davranış C#'ın kendi string sıralamasıyla garanti) hem basit.
        var all = await _set
            .Include(c => c.Translations)
            .ThenInclude(t => t.Language)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

        if (string.IsNullOrWhiteSpace(level))
            return all;

        return all.Where(c =>
                (c.MinLevel is null || string.CompareOrdinal(c.MinLevel, level) <= 0)
                && (c.MaxLevel is null || string.CompareOrdinal(level, c.MaxLevel) <= 0)
            )
            .ToList();
    }

    public Task<Category?> GetWithTranslationsAsync(int id, CancellationToken ct = default) =>
        _set.Include(c => c.Translations).ThenInclude(t => t.Language).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyDictionary<int, int>> GetWordCountsAsync(CancellationToken ct = default) =>
        await _db
            .WordCategories.Where(wc => !wc.WordConcept.IsDeleted)
            .GroupBy(wc => wc.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);

    public Task<bool> HasChildrenAsync(int categoryId, CancellationToken ct = default) =>
        _set.AnyAsync(c => c.ParentCategoryId == categoryId, ct);

    public Task<bool> HasActiveWordsAsync(int categoryId, CancellationToken ct = default) =>
        _db.WordCategories.AnyAsync(wc => wc.CategoryId == categoryId && !wc.WordConcept.IsDeleted, ct);

    public async Task<bool> WouldCreateCycleAsync(int categoryId, int newParentId, CancellationToken ct = default)
    {
        var current = (int?)newParentId;
        var visited = new HashSet<int>();

        while (current is not null)
        {
            if (current.Value == categoryId)
                return true;

            // NEDEN visited kontrolü: DB'de zaten bozuk (kendinden önceki bir kod hatasıyla
            // oluşmuş) bir döngü varsa bu ÖNLEM olmadan while sonsuza kadar dönerdi.
            if (!visited.Add(current.Value))
                return false;

            current = await _set.Where(c => c.Id == current.Value).Select(c => c.ParentCategoryId).FirstOrDefaultAsync(ct);
        }

        return false;
    }

    // AMAÇ: Admin istatistik kartı — toplam (soft-delete edilmemiş) Category sayısı.
    public Task<int> GetTotalCountAsync(CancellationToken ct = default) => _set.CountAsync(ct);
}
