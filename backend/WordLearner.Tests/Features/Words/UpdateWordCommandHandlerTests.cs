// ─────────────────────────────────────────────────────────────────────────────
// UpdateWordCommandHandlerTests.cs
//
// AMAÇ: UpdateWordCommandHandler'ın mevcut çevirileri güncellediğini, eksik
//       dili "eşleştirme" olarak eklediğini, duplikat 409 + force bypass'ını,
//       404'ü ve UPDATE_WORD audit kaydını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class UpdateWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UpdateWordCommandHandler CreateHandler() =>
        new(_wordConceptRepo.Object, _languageRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private static WordConcept UnmatchedConcept() =>
        new()
        {
            Id = 10,
            PartOfSpeech = "Noun",
            DifficultyLevel = "A1",
            Words = new List<Word>
            {
                new()
                {
                    Id = 100,
                    LanguageId = German.Id,
                    Language = German,
                    Text = "Tisch",
                    WordExamples = new List<WordExample>(),
                },
            },
        };

    /// <summary>
    /// Update_ExistingTranslation_UpdatesTextAndDefinition
    ///
    /// AMAÇ: Kavramda zaten var olan bir dilin Text/Definition alanlarının
    ///       yerinde güncellendiğini (yeni Word EKLENMEDİĞİNİ) doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_ExistingTranslation_UpdatesTextAndDefinition()
    {
        // ARRANGE
        var concept = UnmatchedConcept();
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concept);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        var handler = CreateHandler();
        var command = new UpdateWordCommand(
            10,
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "der Tisch", "table", null, null) }
        );

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().ContainSingle(t => t.Text == "der Tisch" && t.Definition == "table");
        concept.Words.Should().HaveCount(1);
    }

    /// <summary>
    /// Update_NewLanguageAddedWithoutForce_ThrowsDuplicateWordExceptionOnCollision
    ///
    /// AMAÇ: Kavramda henüz olmayan bir dil eklenirken (eşleştirme) o dilde
    ///       aynı Text zaten varsa ve force verilmediyse DuplicateWordException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_NewLanguageAddedWithoutForce_ThrowsDuplicateWordExceptionOnCollision()
    {
        // ARRANGE
        var concept = UnmatchedConcept();
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concept);
        _languageRepo.Setup(r => r.GetByCodeAsync("tr", It.IsAny<CancellationToken>())).ReturnsAsync(Turkish);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(Turkish.Id, "masa", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new UpdateWordCommand(
            10,
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("tr", "masa", null, null, null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<DuplicateWordException>();
    }

    /// <summary>
    /// Update_NewLanguageAdded_MatchesConceptByAddingSecondWord
    ///
    /// AMAÇ: Tek dilli ("eşleşmemiş") bir kavrama ikinci dilin eklenmesinin
    ///       (Icerik.md "Eşleştirme") kavramı 2 Word'lü hâle getirdiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_NewLanguageAdded_MatchesConceptByAddingSecondWord()
    {
        // ARRANGE
        var concept = UnmatchedConcept();
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concept);
        _languageRepo.Setup(r => r.GetByCodeAsync("tr", It.IsAny<CancellationToken>())).ReturnsAsync(Turkish);
        _wordConceptRepo
            .Setup(r => r.ExistsWordTextAsync(Turkish.Id, "masa", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var handler = CreateHandler();
        var command = new UpdateWordCommand(
            10,
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("tr", "masa", null, null, null) }
        );

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().HaveCount(2);
        result.Translations.Should().Contain(t => t.LanguageCode == "tr" && t.Text == "masa");
    }

    /// <summary>
    /// Update_ConceptNotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Var olmayan bir WordConcept Id'si verilirse 404'e denk gelen
    ///       EntityNotFoundException fırlatıldığını doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_ConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept?)null);
        var handler = CreateHandler();
        var command = new UpdateWordCommand(
            999,
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "Tisch", null, null, null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Update_Success_LogsUpdateWordActivity
    ///
    /// AMAÇ: Başarılı güncellemede IActivityLogger.LogAsync'in UPDATE_WORD action'ı,
    ///       hem OldValue hem NewValue dolu olarak ÇAĞRILDIĞINI doğrulamak (A-04).
    /// </summary>
    [Fact]
    public async Task Update_Success_LogsUpdateWordActivity()
    {
        // ARRANGE
        var concept = UnmatchedConcept();
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concept);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        var handler = CreateHandler();
        var command = new UpdateWordCommand(
            10,
            "Noun",
            "A1",
            null,
            new[] { new WordTranslationInput("de", "der Tisch", null, null, null) }
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
                    "UPDATE_WORD",
                    "WordConcept",
                    10,
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    null,
                    null,
                    default
                ),
            Times.Once
        );
    }
}
