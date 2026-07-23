// ─────────────────────────────────────────────────────────────────────────────
// GetCategoriesQueryHandlerTests.cs
//
// AMAÇ: GetCategoriesQueryHandler'ın düz listeyi doğru ağaca çevirdiğini, orphan
//       (üstü filtrelenmiş) düğümleri köke terfi ettirdiğini ve includeWordCount
//       davranışını doğrulamak (CategoryDtoBuilder.BuildTree, dolaylı yoldan).
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Features.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Categories;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();

    private GetCategoriesQueryHandler CreateHandler() => new(_categoryRepo.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    private static Category MakeCategory(int id, int? parentId, string name) =>
        new()
        {
            Id = id,
            ParentCategoryId = parentId,
            Translations = new List<CategoryTranslation> { new() { LanguageId = German.Id, Language = German, Name = name } },
        };

    /// <summary>
    /// GetCategories_ParentAndChild_BuildsNestedTree
    ///
    /// AMAÇ: ParentCategoryId ile bağlı düz bir listenin, kök→çocuk ağacına
    ///       doğru şekilde çevrildiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetCategories_ParentAndChild_BuildsNestedTree()
    {
        // ARRANGE
        var flat = new List<Category> { MakeCategory(1, null, "Yiyecek"), MakeCategory(2, 1, "Meyve") };
        _categoryRepo.Setup(r => r.GetAllWithTranslationsAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(flat);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery(null, false), default);

        // ASSERT
        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
        result[0].Children.Should().ContainSingle(c => c.Id == 2);
        result[0].WordCount.Should().BeNull();
    }

    /// <summary>
    /// GetCategories_ParentFilteredOut_PromotesOrphanChildToRoot
    ///
    /// AMAÇ: `level` filtresi sonucu bir üst kategori repository katmanında
    ///       elendiğinde (flat listede artık YOK), alt kategorinin ağaçtan
    ///       DÜŞMEDİĞİNİ, kök seviyeye terfi ettiğini doğrulamak (CategoryDtoBuilder
    ///       "NEDEN orphan düğümler kök yapılır" kararı).
    /// </summary>
    [Fact]
    public async Task GetCategories_ParentFilteredOut_PromotesOrphanChildToRoot()
    {
        // ARRANGE — yalnızca id=2 (parent'ı id=1 flat listede YOK) dönüyor
        var flat = new List<Category> { MakeCategory(2, 1, "Meyve") };
        _categoryRepo.Setup(r => r.GetAllWithTranslationsAsync("A2", It.IsAny<CancellationToken>())).ReturnsAsync(flat);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery("A2", false), default);

        // ASSERT
        result.Should().ContainSingle(c => c.Id == 2);
    }

    /// <summary>
    /// GetCategories_IncludeWordCountTrue_PopulatesWordCount
    /// </summary>
    [Fact]
    public async Task GetCategories_IncludeWordCountTrue_PopulatesWordCount()
    {
        // ARRANGE
        var flat = new List<Category> { MakeCategory(1, null, "Yiyecek") };
        _categoryRepo.Setup(r => r.GetAllWithTranslationsAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(flat);
        _categoryRepo
            .Setup(r => r.GetWordCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, int> { [1] = 5 });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery(null, true), default);

        // ASSERT
        result[0].WordCount.Should().Be(5);
    }

    /// <summary>
    /// GetCategories_IncludeWordCountFalse_DoesNotCallGetWordCounts
    ///
    /// AMAÇ: includeWordCount=false iken GetWordCountsAsync'in (gereksiz GROUP BY
    ///       sorgusu) HİÇ ÇAĞRILMADIĞINI doğrulamak.
    /// </summary>
    [Fact]
    public async Task GetCategories_IncludeWordCountFalse_DoesNotCallGetWordCounts()
    {
        // ARRANGE
        _categoryRepo
            .Setup(r => r.GetAllWithTranslationsAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new GetCategoriesQuery(null, false), default);

        // ASSERT
        _categoryRepo.Verify(r => r.GetWordCountsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
