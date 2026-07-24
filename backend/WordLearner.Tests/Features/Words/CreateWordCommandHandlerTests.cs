using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class CreateWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateWordCommandHandler CreateHandler() =>
        new(_wordConceptRepo.Object, _categoryRepo.Object, _languageRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private void SetupAddPassthrough() =>
        _wordConceptRepo
            .Setup(r => r.AddAsync(It.IsAny<WordConcept>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept c, int? _, CancellationToken _) => c);

    [Fact]
    public async Task Create_SingleTranslation_ReturnsUnmatchedConceptWithOneTranslation()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(German.Id, "Tisch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Tisch", null, null, null) }
        )
        {
            UserId = 7,
        };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.PartOfSpeech.Should().Be("Noun");
        result.Translations.Should().ContainSingle(t => t.LanguageCode == "de" && t.Text == "Tisch");
    }

    [Fact]
    public async Task Create_TwoTranslations_AddsBothWordsToConcept()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _languageRepo.Setup(r => r.GetByCodeAsync("tr", It.IsAny<CancellationToken>())).ReturnsAsync(Turkish);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[]
            {
                new WordTranslationInput("de", "Tisch", null, null, null),
                new WordTranslationInput("tr", "masa", null, null, null),
            }
        )
        {
            UserId = 7,
        };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_DuplicateTextWithoutForce_ThrowsDuplicateWordException()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(German.Id, "Tisch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Tisch", null, null, null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateWordException>();
        _wordConceptRepo.Verify(
            r => r.AddAsync(It.IsAny<WordConcept>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Create_DuplicateTextWithForce_CreatesAnyway()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Tisch", null, null, null) }
        )
        {
            Force = true,
        };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().ContainSingle();
        _wordConceptRepo.Verify(
            r => r.ExistsWordTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Create_UnknownLanguageCode_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepo
            .Setup(r => r.GetByCodeAsync("en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Language?)null);
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("en", "table", null, null, null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Create_WithCategoryIds_LinksWordCategories()
    {
        // ARRANGE
        var food = new Category { Id = 3 };
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _categoryRepo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(food);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Apfel", null, null, null) },
            new[] { 3 }
        );

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Categories.Should().ContainSingle(c => c.CategoryId == 3);
    }

    [Fact]
    public async Task Create_UnknownCategoryId_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _categoryRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Apfel", null, null, null) },
            new[] { 999 }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Create_Success_LogsCreateWordActivity()
    {
        // ARRANGE
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(German.Id, "Tisch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        SetupAddPassthrough();
        var handler = CreateHandler();
        var command = new CreateWordCommand(
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Tisch", null, null, null) }
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
                    "CREATE_WORD",
                    "WordConcept",
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
