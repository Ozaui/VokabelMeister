// ─────────────────────────────────────────────────────────────────────────────
// DeleteCategoryCommandHandlerTests.cs
//
// AMAÇ: DeleteCategoryCommandHandler'ın alt kategori/aktif kelime silme
//       korumasını (409) ve DELETE_CATEGORY audit kaydını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Categories;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private DeleteCategoryCommandHandler CreateHandler() => new(_categoryRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    private static Category ExistingCategory() =>
        new()
        {
            Id = 10,
            Translations = new List<CategoryTranslation>
            {
                new()
                {
                    LanguageId = German.Id,
                    Language = German,
                    Name = "Essen",
                },
            },
        };

    /// <summary>
    /// Delete_CategoryNotFound_ThrowsEntityNotFoundException
    /// </summary>
    [Fact]
    public async Task Delete_CategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DeleteCategoryCommand(10), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Delete_HasChildren_ThrowsCategoryHasChildrenException
    ///
    /// AMAÇ: Alt kategorisi olan bir kategori silinmeye çalışıldığında 409
    ///       fırlatıldığını ve SoftDeleteAsync'in HİÇ ÇAĞRILMADIĞINI doğrulamak.
    /// </summary>
    [Fact]
    public async Task Delete_HasChildren_ThrowsCategoryHasChildrenException()
    {
        // ARRANGE
        _categoryRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingCategory());
        _categoryRepo.Setup(r => r.HasChildrenAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DeleteCategoryCommand(10), default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryHasChildrenException>();
        _categoryRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Delete_HasActiveWords_ThrowsCategoryHasActiveWordsException
    /// </summary>
    [Fact]
    public async Task Delete_HasActiveWords_ThrowsCategoryHasActiveWordsException()
    {
        // ARRANGE
        _categoryRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingCategory());
        _categoryRepo.Setup(r => r.HasChildrenAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepo.Setup(r => r.HasActiveWordsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DeleteCategoryCommand(10), default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryHasActiveWordsException>();
        _categoryRepo.Verify(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Delete_NoBlockers_SoftDeletesAndLogsActivity
    /// </summary>
    [Fact]
    public async Task Delete_NoBlockers_SoftDeletesAndLogsActivity()
    {
        // ARRANGE
        _categoryRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingCategory());
        _categoryRepo.Setup(r => r.HasChildrenAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepo.Setup(r => r.HasActiveWordsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = CreateHandler();
        var command = new DeleteCategoryCommand(10) { UserId = 7, ActorRole = "Admin" };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _categoryRepo.Verify(r => r.SoftDeleteAsync(10, 7, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    7,
                    "Admin",
                    "DELETE_CATEGORY",
                    "Category",
                    10,
                    It.IsAny<object>(),
                    null,
                    null,
                    null,
                    default
                ),
            Times.Once
        );
    }
}
