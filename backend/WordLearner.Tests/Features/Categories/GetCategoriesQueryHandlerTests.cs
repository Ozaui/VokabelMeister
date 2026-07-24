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
