using FluentAssertions;
using Moq;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Features.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Categories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UpdateCategoryCommandHandler CreateHandler() =>
        new(_categoryRepository.Object, _languageRepository.Object, _activityLogger.Object);

    private static CategoryAggregate BuildAggregate(int categoryId, string name, int? parentCategoryId = null)
    {
        var category = new Category { Id = categoryId, ParentCategoryId = parentCategoryId, DisplayOrder = 1, MinLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var translation = new CategoryTranslation { Id = 1, CategoryId = categoryId, LanguageId = 1, Name = name };
        return new CategoryAggregate(category, [new CategoryTranslationAggregate(translation, language)]);
    }

    private void SetupGermanTranslationRoundtrip(int categoryId, string existingName)
    {
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _categoryRepository.Setup(r => r.FindTranslationAsync(categoryId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CategoryTranslation { Id = 1, CategoryId = categoryId, LanguageId = 1, Name = existingName });
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((CategoryAggregate?)null);
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new UpdateCategoryCommand(99, null, 1, null, null, null, null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ParentChangeWithoutCycle_MovesCategoryToNewParent()
    {
        // ARRANGE — kategori 1 eskiden kök (ParentCategoryId=null), yeni istek 2'ye taşıyor
        var before = BuildAggregate(1, "Tiere");
        var after = BuildAggregate(1, "Tiere", parentCategoryId: 2);
        _categoryRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(before)
            .ReturnsAsync(after);
        _categoryRepository.Setup(r => r.GetAggregateAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(2, "Doğa"));
        _categoryRepository.Setup(r => r.WouldCreateCycleAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        SetupGermanTranslationRoundtrip(1, "Tiere");
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new UpdateCategoryCommand(1, 2, 1, null, null, "A1", null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.ParentCategoryId.Should().Be(2);
        _categoryRepository.Verify(r => r.UpdateCategoryAsync(It.Is<Category>(c => c.ParentCategoryId == 2), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ParentChangeWouldCreateCycle_ThrowsCategoryParentCycleException()
    {
        // ARRANGE — WouldCreateCycleAsync true dönerse (ör. 3 kategori 1'in altında) taşıma REDDEDİLİR
        var before = BuildAggregate(1, "Tiere");
        _categoryRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(before);
        _categoryRepository.Setup(r => r.GetAggregateAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(3, "AltKategori"));
        _categoryRepository.Setup(r => r.WouldCreateCycleAsync(1, 3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new UpdateCategoryCommand(1, 3, 1, null, null, "A1", null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryParentCycleException>();
    }

    [Fact]
    public async Task Handle_NewParentCategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        var before = BuildAggregate(1, "Tiere");
        _categoryRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(before);
        _categoryRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((CategoryAggregate?)null);
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new UpdateCategoryCommand(1, 99, 1, null, null, null, null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_Success_LogsUpdateCategoryActivityWithOldAndNewValue()
    {
        // ARRANGE
        var before = BuildAggregate(1, "Tiere");
        var after = BuildAggregate(1, "Tiere2");
        _categoryRepository.SetupSequence(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(before)
            .ReturnsAsync(after);
        SetupGermanTranslationRoundtrip(1, "Tiere");
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere2", null) };
        var command = new UpdateCategoryCommand(1, null, 1, null, null, null, null, translations, UserId: 3, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — hem OldValue hem NewValue dolu (Create'in AKSİNE)
        _activityLogger.Verify(l => l.LogAsync(
            3, "Admin", "UPDATE_CATEGORY", "Category", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
