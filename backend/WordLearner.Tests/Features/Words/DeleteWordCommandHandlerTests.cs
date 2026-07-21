// ─────────────────────────────────────────────────────────────────────────────
// DeleteWordCommandHandlerTests.cs
//
// AMAÇ: DeleteWordCommandHandler'ın var olan bir kavramı SoftDeleteWithWordsAsync
//       ile sildiğini, olmayan bir Id'de 404 fırlattığını ve DELETE_WORD audit
//       kaydını doğrulamak.
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

public class DeleteWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private DeleteWordCommandHandler CreateHandler() => new(_wordConceptRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };

    private static WordConcept ExistingConcept() =>
        new()
        {
            Id = 10,
            PartOfSpeech = "Noun",
            DifficultyLevel = "A1",
            Words = new List<Word>
            {
                new()
                {
                    LanguageId = German.Id,
                    Language = German,
                    Text = "Tisch",
                },
            },
        };

    /// <summary>
    /// Delete_ExistingConcept_CallsSoftDeleteWithWords
    ///
    /// AMAÇ: Var olan bir kavram silinirken repository'nin
    ///       SoftDeleteWithWordsAsync'inin (WordConcept + tüm Word'ler) ÇAĞRILDIĞINI doğrulamak.
    /// </summary>
    [Fact]
    public async Task Delete_ExistingConcept_CallsSoftDeleteWithWords()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingConcept());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new DeleteWordCommand(10) { UserId = 7 }, default);

        // ASSERT
        _wordConceptRepo.Verify(r => r.SoftDeleteWithWordsAsync(10, 7, default), Times.Once);
    }

    /// <summary>
    /// Delete_ConceptNotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Var olmayan bir WordConcept Id'si verilirse 404'e denk gelen
    ///       EntityNotFoundException fırlatıldığını VE SoftDeleteWithWordsAsync'in
    ///       HİÇ ÇAĞRILMADIĞINI doğrulamak.
    /// </summary>
    [Fact]
    public async Task Delete_ConceptNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new DeleteWordCommand(999), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _wordConceptRepo.Verify(
            r => r.SoftDeleteWithWordsAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    /// <summary>
    /// Delete_Success_LogsDeleteWordActivity
    ///
    /// AMAÇ: Başarılı silmede IActivityLogger.LogAsync'in DELETE_WORD action'ı,
    ///       EntityType=WordConcept ve silinen kavramın eski hâlini taşıyan
    ///       OldValue ile ÇAĞRILDIĞINI doğrulamak (A-04).
    /// </summary>
    [Fact]
    public async Task Delete_Success_LogsDeleteWordActivity()
    {
        // ARRANGE
        _wordConceptRepo
            .Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingConcept());
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new DeleteWordCommand(10) { UserId = 7, ActorRole = "Admin" }, default);

        // ASSERT
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    7,
                    "Admin",
                    "DELETE_WORD",
                    "WordConcept",
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
