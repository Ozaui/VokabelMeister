using FluentAssertions;
using Moq;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Categories;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private DeleteCategoryCommandHandler CreateHandler() => new(_categoryRepository.Object, _activityLogger.Object);

    private static CategoryAggregate BuildAggregate(int categoryId, string name)
    {
        var category = new Category { Id = categoryId, DisplayOrder = 1, MinLevel = "A1" };
        var language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
        var translation = new CategoryTranslation { Id = 1, CategoryId = categoryId, LanguageId = 1, Name = name };
        return new CategoryAggregate(category, [new CategoryTranslationAggregate(translation, language)]);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((CategoryAggregate?)null);
        var command = new DeleteCategoryCommand(99, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_HasChildren_ThrowsCategoryHasChildrenException()
    {
        // ARRANGE — "orphan terfi" YOK (A_backend.md A-06 notu) — çocuğu olan kategori silinemez
        _categoryRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(1, "Tiere"));
        _categoryRepository.Setup(r => r.HasChildrenAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var command = new DeleteCategoryCommand(1, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryHasChildrenException>();
        _categoryRepository.Verify(r => r.SoftDeleteCategoryAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_HasActiveWords_ThrowsCategoryHasActiveWordsException()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(1, "Tiere"));
        _categoryRepository.Setup(r => r.HasChildrenAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepository.Setup(r => r.HasActiveWordsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var command = new DeleteCategoryCommand(1, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryHasActiveWordsException>();
        _categoryRepository.Verify(r => r.SoftDeleteCategoryAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoChildrenOrActiveWords_SoftDeletesAndLogsActivity()
    {
        // ARRANGE
        _categoryRepository.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildAggregate(1, "Tiere"));
        _categoryRepository.Setup(r => r.HasChildrenAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepository.Setup(r => r.HasActiveWordsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var command = new DeleteCategoryCommand(1, UserId: 5, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue dolu, NewValue null (Word'ün DELETE_WORD'üyle AYNI desen)
        _categoryRepository.Verify(r => r.SoftDeleteCategoryAsync(1, 5, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(l => l.LogAsync(
            5, "Admin", "DELETE_CATEGORY", "Category", 1, It.IsAny<object>(), null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
