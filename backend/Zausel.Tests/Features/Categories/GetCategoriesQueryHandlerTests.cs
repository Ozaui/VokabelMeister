using FluentAssertions;
using Moq;
using Zausel.Application.Features.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Entities.Content;

namespace Zausel.Tests.Features.Categories;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();

    private GetCategoriesQueryHandler CreateHandler() => new(_categoryRepository.Object);

    private static CategoryAggregate BuildAggregate(int categoryId, string name, int? parentCategoryId = null, string? minLevel = "A1")
    {
        var category = new Category { Id = categoryId, ParentCategoryId = parentCategoryId, DisplayOrder = categoryId, MinLevel = minLevel };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var translation = new CategoryTranslation { Id = categoryId, CategoryId = categoryId, LanguageId = 1, Name = name };
        return new CategoryAggregate(category, [new CategoryTranslationAggregate(translation, language)]);
    }

    [Fact]
    public async Task Handle_FlatCategoriesWithParentChild_BuildsHierarchicalTree()
    {
        // ARRANGE — kategori 2, kategori 1'in çocuğu
        var root = BuildAggregate(1, "Menschen");
        var child = BuildAggregate(2, "Familie", parentCategoryId: 1);
        _categoryRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([root, child]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery(null, false), default);

        // ASSERT — düz liste yerine iç içe ağaç: yalnızca 1 kök, 2 onun children[]'ında
        result.Should().ContainSingle(c => c.Id == 1);
        result[0].Children.Should().ContainSingle(c => c.Id == 2);
    }

    [Fact]
    public async Task Handle_LevelFilterProvided_ExcludesNonMatchingCategories()
    {
        // ARRANGE
        var a1Category = BuildAggregate(1, "Menschen", minLevel: "A1");
        var a2Category = BuildAggregate(2, "Arbeit", minLevel: "A2");
        _categoryRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([a1Category, a2Category]);
        var handler = CreateHandler();

        // ACT — level=A1 iken MinLevel=A2 olan kategori A1'den YÜKSEK sayılır, sonuçtan ELENİR
        var result = await handler.Handle(new GetCategoriesQuery("A1", false), default);

        // ASSERT
        result.Should().ContainSingle(c => c.Id == 1);
    }

    [Fact]
    public async Task Handle_IncludeWordCountTrue_AttachesWordCounts()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([BuildAggregate(1, "Tiere")]);
        _categoryRepository.Setup(r => r.GetActiveWordCountsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<int, int> { [1] = 5 });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery(null, true), default);

        // ASSERT
        result[0].WordCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_IncludeWordCountFalse_WordCountIsNullAndRepositoryNotCalled()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([BuildAggregate(1, "Tiere")]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoriesQuery(null, false), default);

        // ASSERT — gereksiz sorgu YAPILMAZ (?includeWordCount=false iken performans maliyeti yok)
        result[0].WordCount.Should().BeNull();
        _categoryRepository.Verify(r => r.GetActiveWordCountsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
