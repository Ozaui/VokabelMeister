using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateCategoryCommandHandler CreateHandler() =>
        new(_categoryRepo.Object, _languageRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private void SetupAddPassthrough() =>
        _categoryRepo
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, int? _, CancellationToken _) => c);

    [Fact]
    public async Task Create_TwoTranslations_ReturnsCategoryWithBothTranslations()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _languageRepo.Setup(r => r.GetByCodeAsync("tr", It.IsAny<CancellationToken>())).ReturnsAsync(Turkish);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateCategoryCommand(
            null,
            1,
            "food",
            "#95E1D3",
            "A1",
            null,
            new[]
            {
                new CategoryTranslationInput("de", "Essen", null),
                new CategoryTranslationInput("tr", "Yemek", null),
            }
        )
        {
            UserId = 7,
        };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().HaveCount(2);
        result.Translations.Should().Contain(t => t.LanguageCode == "de" && t.Name == "Essen");
    }

    [Fact]
    public async Task Create_ParentCategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = CreateHandler();
        var command = new CreateCategoryCommand(
            999,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _categoryRepo.Verify(
            r => r.AddAsync(It.IsAny<Category>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Create_UnknownLanguageCode_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("en", It.IsAny<CancellationToken>())).ReturnsAsync((Language?)null);
        var handler = CreateHandler();
        var command = new CreateCategoryCommand(
            null,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("en", "Food", null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Create_Success_LogsCreateCategoryActivity()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateCategoryCommand(
            null,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        )
        {
            UserId = 7,
            ActorRole = "Admin",
        };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    7,
                    "Admin",
                    "CREATE_CATEGORY",
                    "Category",
                    It.IsAny<int?>(),
                    null,
                    It.IsAny<object>(),
                    null,
                    null,
                    default
                ),
            Times.Once
        );
    }
}
