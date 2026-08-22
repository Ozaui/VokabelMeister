using FluentAssertions;
using Moq;
using Zausel.Application.Features.Progress;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.Srs;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.Srs;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.Progress;

public class LearnSystemWordCommandHandlerTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<ILanguageRepository> _languageRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly Mock<IAchievementService> _achievementService = new();

    private LearnSystemWordCommandHandler CreateHandler() =>
        new(_userProgressRepository.Object, _wordConceptRepository.Object, _languageRepository.Object, _activityLogger.Object, _achievementService.Object);

    private void SetupWordAndGerman(int wordId, int wordConceptId, string germanText)
    {
        _wordConceptRepository.Setup(r => r.GetWordByIdAsync(wordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = wordId, WordConceptId = wordConceptId, LanguageId = 2, Text = "koşmak" });
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _wordConceptRepository.Setup(r => r.FindWordAsync(wordConceptId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 5, WordConceptId = wordConceptId, LanguageId = 1, Text = germanText });
    }

    [Fact]
    public async Task Handle_WordNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _wordConceptRepository.Setup(r => r.GetWordByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Word?)null);
        var command = new LearnSystemWordCommand(99, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _userProgressRepository.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoExistingProgress_CreatesUserProgressAndReturnsAlreadyExistsFalse()
    {
        // ARRANGE
        SetupWordAndGerman(wordId: 5, wordConceptId: 10, germanText: "laufen");
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync((UserProgress?)null);
        _userProgressRepository
            .Setup(r => r.AddAsync(It.IsAny<UserProgress>(), 1, It.IsAny<CancellationToken>()))
            .Callback<UserProgress, int, CancellationToken>((p, _, _) => p.Id = 12)
            .Returns(Task.CompletedTask);
        var command = new LearnSystemWordCommand(5, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.UserProgressId.Should().Be(12);
        result.WordId.Should().Be(5);
        result.GermanWord.Should().Be("laufen");
        result.AlreadyExists.Should().BeFalse();
        _userProgressRepository.Verify(r => r.AddAsync(It.Is<UserProgress>(p => p.UserId == 1 && p.WordId == 5), 1, It.IsAny<CancellationToken>()), Times.Once);
        _achievementService.Verify(a => a.EvaluateAndUnlockAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingProgress_DoesNotCreateAndReturnsAlreadyExistsTrue()
    {
        // ARRANGE — kullanıcı bu kelimeyi ZATEN öğreniyor, ikinci bir UserProgress satırı YARATILMAMALI.
        SetupWordAndGerman(wordId: 5, wordConceptId: 10, germanText: "laufen");
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { Id = 7, UserId = 1, WordId = 5 });
        var command = new LearnSystemWordCommand(5, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.UserProgressId.Should().Be(7);
        result.AlreadyExists.Should().BeTrue();
        _userProgressRepository.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _achievementService.Verify(a => a.EvaluateAndUnlockAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConceptNotMatchedInGerman_FallsBackToOwnText()
    {
        // ARRANGE — kavramın Almanca çevirisi yok (tek dilli), kendi metnine düşülür.
        _wordConceptRepository.Setup(r => r.GetWordByIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 8, WordConceptId = 20, LanguageId = 2, Text = "kelime" });
        _languageRepository.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" });
        _wordConceptRepository.Setup(r => r.FindWordAsync(20, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Word?)null);
        _userProgressRepository.Setup(r => r.GetByUserAndWordAsync(1, 8, It.IsAny<CancellationToken>())).ReturnsAsync((UserProgress?)null);
        _userProgressRepository
            .Setup(r => r.AddAsync(It.IsAny<UserProgress>(), 1, It.IsAny<CancellationToken>()))
            .Callback<UserProgress, int, CancellationToken>((p, _, _) => p.Id = 1)
            .Returns(Task.CompletedTask);
        var command = new LearnSystemWordCommand(8, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.GermanWord.Should().Be("kelime");
    }
}
