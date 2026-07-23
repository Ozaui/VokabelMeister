// ─────────────────────────────────────────────────────────────────────────────
// CategoryDtoBuilder.cs
//
// AMAÇ: Düz bir Category listesini (ParentCategoryId ile bağlı) `CategoryDto`
//       AĞACINA çeviren paylaşılan yardımcı — WordConceptDtoBuilder (A-05) ile
//       BİREBİR aynı gerekçeyle AutoMapper Profile DEĞİL, elle yazılır: bu bir
//       tek-entity→tek-DTO dönüşümü değil, DÜZ bir listeyi RECURSIVE bir ağaca
//       (+ opsiyonel kelime sayısı birleştirmesi) dönüştüren bir projeksiyon —
//       CLAUDE.md §3'ün koşullu AutoMapper kuralı bu senaryoyu kapsam DIŞI bırakır.
// NEDEN "level filtresinden sonra kalan ORPHAN düğümler KÖK yapılır": GetCategoriesQuery
//       `level` filtresi uygulandığında (ICategoryRepository.GetAllWithTranslationsAsync)
//       bir üst kategori filtreye takılıp elenebilir ama alt kategorisi filtreyi
//       GEÇEBİLİR — bu durumda alt kategori havada KALMAMALI (ağaçtan tamamen
//       düşmemeli), bu yüzden ParentCategoryId'si artık flat listede YOK olan her
//       düğüm kök seviyeye TERFİ ettirilir (bkz. GetCategoriesQuery.cs "NEDEN").
// BAĞIMLILIKLAR: Category/CategoryTranslation entity'leri, CategoryDtos.cs.
// ─────────────────────────────────────────────────────────────────────────────

using WordLearner.Application.DTOs.Categories;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Features.Categories;

public static class CategoryDtoBuilder
{
    // AMAÇ: Düz kategori listesini kök→yaprak ağaca çevirir; wordCounts NULL ise
    //       (includeWordCount=false) her düğümün WordCount'u NULL kalır.
    public static IReadOnlyList<CategoryDto> BuildTree(
        IReadOnlyList<Category> flat,
        IReadOnlyDictionary<int, int>? wordCounts
    )
    {
        var idSet = flat.Select(c => c.Id).ToHashSet();
        var byParent = flat.ToLookup(c => c.ParentCategoryId);

        var roots = flat.Where(c => c.ParentCategoryId is null || !idSet.Contains(c.ParentCategoryId.Value));

        return roots.OrderBy(c => c.DisplayOrder).Select(c => Build(c, byParent, wordCounts)).ToList();
    }

    // AMAÇ: Tek bir kategoriyi (Children'ı OLMADAN) DTO'ya çevirir — POST/PUT
    //       yanıtında yeni/güncellenmiş kategorinin kendisini döndürmek için,
    //       ağacın tamamını yeniden kurmaya gerek yoktur.
    public static CategoryDto BuildSingle(Category category) =>
        new(
            category.Id,
            category.ParentCategoryId,
            category.DisplayOrder,
            category.Icon,
            category.Color,
            category.MinLevel,
            category.MaxLevel,
            category.Translations.OrderBy(t => t.LanguageId).Select(BuildTranslation).ToList(),
            WordCount: null,
            Children: Array.Empty<CategoryDto>()
        );

    private static CategoryDto Build(
        Category category,
        ILookup<int?, Category> byParent,
        IReadOnlyDictionary<int, int>? wordCounts
    ) =>
        new(
            category.Id,
            category.ParentCategoryId,
            category.DisplayOrder,
            category.Icon,
            category.Color,
            category.MinLevel,
            category.MaxLevel,
            category.Translations.OrderBy(t => t.LanguageId).Select(BuildTranslation).ToList(),
            wordCounts is null ? null : wordCounts.GetValueOrDefault(category.Id),
            byParent[category.Id]
                .OrderBy(c => c.DisplayOrder)
                .Select(c => Build(c, byParent, wordCounts))
                .ToList()
        );

    private static CategoryTranslationDto BuildTranslation(CategoryTranslation t) =>
        new(t.Language.Code, t.Name, t.Description);
}
