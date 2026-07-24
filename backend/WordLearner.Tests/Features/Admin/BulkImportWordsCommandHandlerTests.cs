using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Application.Validators.Words;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Admin;

public class BulkImportWordsCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IValidator<WordGrammarInput>> _grammarValidator = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    private BulkImportWordsCommandHandler CreateHandler() =>
        new(_wordConceptRepo.Object, _categoryRepo.Object, _languageRepo.Object, _grammarValidator.Object, _activityLogger.Object);

    private static BulkImportWordRow Row(string text) =>
        new("Noun", "A1", null, new WordTranslationInput("de", text, null, null, null));

    [Fact]
    public async Task Handle_OneBadRowAmongGoodRows_ImportsGoodRowsAndReportsBadRow()
    {
        // ARRANGE
        _grammarValidator.Setup(v => v.Validate(It.IsAny<WordGrammarInput>())).Returns(new ValidationResult());
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _languageRepo.Setup(r => r.GetByCodeAsync("xx", It.IsAny<CancellationToken>())).ReturnsAsync((Language?)null);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var rows = new List<BulkImportWordRow>
        {
            Row("Haus"),
            new("Noun", "A1", null, new WordTranslationInput("xx", "Unbekannt", null, null, null)),
            Row("Baum"),
        };
        var handler = CreateHandler();
        var command = new BulkImportWordsCommand(rows) { UserId = 1, ActorRole = "Admin" };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.TotalRows.Should().Be(3);
        result.ImportedCount.Should().Be(2);
        result.SkippedCount.Should().Be(1);
        result.Results[1].Success.Should().BeFalse();
        result.Results[1].ErrorCode.Should().Be("LANGUAGE_NOT_FOUND");
        result.Results[0].Success.Should().BeTrue();
        result.Results[2].Success.Should().BeTrue();
        _wordConceptRepo.Verify(r => r.AddAsync(It.IsAny<WordConcept>(), 1, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_DuplicateText_SkipsRowWithoutForce()
    {
        // ARRANGE
        _grammarValidator.Setup(v => v.Validate(It.IsAny<WordGrammarInput>())).Returns(new ValidationResult());
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(German.Id, "Haus", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new BulkImportWordsCommand(new List<BulkImportWordRow> { Row("Haus") }) { UserId = 1 };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.ImportedCount.Should().Be(0);
        result.Results[0].ErrorCode.Should().Be("WORD_TEXT_ALREADY_EXISTS");
        _wordConceptRepo.Verify(r => r.AddAsync(It.IsAny<WordConcept>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GrammarValidationFails_SkipsRowWithGrammarErrorCode()
    {
        // ARRANGE
        var invalidResult = new ValidationResult(
            new List<ValidationFailure> { new("GrammarData", "bad") { ErrorCode = "GRAMMAR_DE_NOUN_GENDER_REQUIRED" } }
        );
        _grammarValidator.Setup(v => v.Validate(It.IsAny<WordGrammarInput>())).Returns(invalidResult);
        var handler = CreateHandler();
        var command = new BulkImportWordsCommand(new List<BulkImportWordRow> { Row("Haus") }) { UserId = 1 };

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Results[0].ErrorCode.Should().Be("GRAMMAR_DE_NOUN_GENDER_REQUIRED");
        _languageRepo.Verify(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AllRowsProcessed_LogsSingleBulkImportActivity()
    {
        // ARRANGE
        _grammarValidator.Setup(v => v.Validate(It.IsAny<WordGrammarInput>())).Returns(new ValidationResult());
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var rows = new List<BulkImportWordRow> { Row("Haus"), Row("Baum") };
        var handler = CreateHandler();
        var command = new BulkImportWordsCommand(rows) { UserId = 1, ActorRole = "Admin" };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    1,
                    "Admin",
                    "BULK_IMPORT_WORDS",
                    "WordConcept",
                    null,
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
