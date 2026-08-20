using FluentAssertions;
using Moq;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Features.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateCategoryCommandHandler CreateHandler() =>
        new(_categoryRepository.Object, _languageRepository.Object, _activityLogger.Object);

    private static CategoryAggregate BuildAggregate(int categoryId, string name)
    {
        var category = new Category { Id = categoryId, DisplayOrder = 1, MinLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var translation = new CategoryTranslation { Id = 1, CategoryId = categoryId, LanguageId = 1, Name = name };
        return new CategoryAggregate(category, [new CategoryTranslationAggregate(translation, language)]);
    }

    private void SetupGermanLanguage() =>
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });

    [Fact]
    public async Task Handle_ValidRequest_CreatesCategoryAndReturnsResponse()
    {
        // ARRANGE
        SetupGermanLanguage();
        _categoryRepository.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, "Tiere"));
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new CreateCategoryCommand(null, 1, "animal", "#FFFFFF", "A1", null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().ContainSingle(t => t.Name == "Tiere");
        _categoryRepository.Verify(r => r.AddCategoryAsync(It.IsAny<Category>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepository.Verify(r => r.AddTranslationAsync(It.Is<CategoryTranslation>(t => t.Name == "Tiere"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ParentCategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((CategoryAggregate?)null);
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new CreateCategoryCommand(99, 1, null, null, null, null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_UnknownLanguageCode_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepository.Setup(r => r.GetByCodeAsync("xx", It.IsAny<CancellationToken>())).ReturnsAsync((Language?)null);
        var translations = new List<CategoryTranslationRequest> { new("xx", "test", null) };
        var command = new CreateCategoryCommand(null, 1, null, null, null, null, translations, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_Success_LogsCreateCategoryActivity()
    {
        // ARRANGE
        SetupGermanLanguage();
        _categoryRepository.Setup(r => r.GetAggregateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildAggregate(1, "Tiere"));
        var translations = new List<CategoryTranslationRequest> { new("de", "Tiere", null) };
        var command = new CreateCategoryCommand(null, 1, null, null, null, null, translations, UserId: 7, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — CREATE_CATEGORY, OldValue=null (yeni kayıt), NewValue dolu
        _activityLogger.Verify(l => l.LogAsync(
            7, "Admin", "CREATE_CATEGORY", "Category", It.IsAny<int?>(), null, It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
