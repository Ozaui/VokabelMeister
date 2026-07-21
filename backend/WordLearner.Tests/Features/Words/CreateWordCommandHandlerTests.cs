// ─────────────────────────────────────────────────────────────────────────────
// CreateWordCommandHandlerTests.cs
//
// AMAÇ: CreateWordCommandHandler'ın 1/2 dilli oluşturmayı, duplikat 409 + force
//       bypass'ını, bilinmeyen dil kodunu ve CREATE_WORD audit kaydını doğrulamak.
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

public class CreateWordCommandHandlerTests
{
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateWordCommandHandler CreateHandler() =>
        new(_wordConceptRepo.Object, _languageRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private void SetupAddPassthrough() =>
        _wordConceptRepo
            .Setup(r => r.AddAsync(It.IsAny<WordConcept>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WordConcept c, int? _, CancellationToken _) => c);

    /// <summary>
    /// Create_SingleTranslation_ReturnsUnmatchedConceptWithOneTranslation
    ///
    /// AMAÇ: Tek dilde translation verildiğinde kavramın tek bir Word'le
    ///       oluşturulduğunu (Icerik.md "eşleşmemiş" durumu) doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Create_TwoTranslations_AddsBothWordsToConcept
    ///
    /// AMAÇ: translations[]'te iki dil verildiğinde kavramın tek işlemde
    ///       eşleşmiş olarak (2 Word) kurulduğunu doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Create_DuplicateTextWithoutForce_ThrowsDuplicateWordException
    ///
    /// AMAÇ: Aynı dilde aynı Text zaten varsa ve force verilmediyse
    ///       DuplicateWordException (409) fırlatıldığını doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Create_DuplicateTextWithForce_CreatesAnyway
    ///
    /// AMAÇ: Force=true verildiğinde duplikat kontrolü bypass edilip kaydın
    ///       yine de oluşturulduğunu doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Create_UnknownLanguageCode_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: translations[].languageCode Languages tablosunda yoksa
    ///       EntityNotFoundException fırlatıldığını doğrulamak.
    /// </summary>
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

    /// <summary>
    /// Create_Success_LogsCreateWordActivity
    ///
    /// AMAÇ: Başarılı oluşturmada IActivityLogger.LogAsync'in CREATE_WORD action'ı,
    ///       EntityType=WordConcept ve doğru UserId/ActorRole ile ÇAĞRILDIĞINI doğrulamak (A-04).
    /// </summary>
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
