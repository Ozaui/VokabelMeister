using FluentAssertions;
using Moq;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Tests.Features.UserCards;

public class CreateUserCardCommandHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly Mock<IWordConceptRepository> _wordConceptRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private CreateUserCardCommandHandler CreateHandler() =>
        new(_userCardRepository.Object, _categoryRepository.Object, _userCategoryRepository.Object, _wordConceptRepository.Object, _activityLogger.Object);

    private static UserCardAggregate Aggregate(UserCard card) => new(card, [], [], []);

    private void SetupNoDuplicateAndAggregate(int userId, string frontText)
    {
        _userCardRepository
            .Setup(r => r.FindByUserAndFrontTextAsync(userId, frontText, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCard?)null);
        _userCardRepository
            .Setup(r => r.AddAsync(It.IsAny<UserCard>(), userId, It.IsAny<CancellationToken>()))
            .Callback<UserCard, int, CancellationToken>((card, _, _) => card.Id = 1)
            .Returns(Task.CompletedTask);
        _userCardRepository
            .Setup(r => r.GetByIdForUserAsync(1, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Aggregate(new UserCard { Id = 1, UserId = userId, FrontText = frontText, BackText = "arka" }));
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserCardAndReturnsResponse()
    {
        // ARRANGE
        SetupNoDuplicateAndAggregate(1, "laufen");
        var command = new CreateUserCardCommand("laufen", "koşmak", null, null, null, null, Force: false, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.FrontText.Should().Be("laufen");
        result.SuggestedSystemWordId.Should().BeNull();
        _userCardRepository.Verify(r => r.AddAsync(It.Is<UserCard>(c => c.UserId == 1 && c.FrontText == "laufen"), 1, It.IsAny<CancellationToken>()), Times.Once);
        _userCardRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_DuplicateWithoutForce_ThrowsUserCardDuplicateException()
    {
        // ARRANGE — AYNI FrontText'le zaten bir kart var, force=false.
        _userCardRepository
            .Setup(r => r.FindByUserAndFrontTextAsync(1, "laufen", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCard { Id = 5, UserId = 1, FrontText = "laufen", BackText = "koşmak" });
        var command = new CreateUserCardCommand("laufen", "koşmak", null, null, null, null, Force: false, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<UserCardDuplicateException>();
        _userCardRepository.Verify(r => r.AddAsync(It.IsAny<UserCard>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DuplicateWithForce_CreatesAnywayWithoutCheckingDuplicate()
    {
        // ARRANGE — force=true iken duplikat sorgusu HİÇ çalışmamalı.
        _userCardRepository
            .Setup(r => r.AddAsync(It.IsAny<UserCard>(), 1, It.IsAny<CancellationToken>()))
            .Callback<UserCard, int, CancellationToken>((card, _, _) => card.Id = 1)
            .Returns(Task.CompletedTask);
        _userCardRepository
            .Setup(r => r.GetByIdForUserAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Aggregate(new UserCard { Id = 1, UserId = 1, FrontText = "laufen", BackText = "koşmak" }));
        var command = new CreateUserCardCommand("laufen", "koşmak", null, null, null, null, Force: true, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _userCardRepository.Verify(r => r.FindByUserAndFrontTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        _userCardRepository.Verify(r => r.AddAsync(It.IsAny<UserCard>(), 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FrontTextMatchesSystemWord_ReturnsSuggestedSystemWordId()
    {
        // ARRANGE — sistemde AYNI metinli bir Word var, kart yine de oluşur ama öneri döner.
        SetupNoDuplicateAndAggregate(1, "laufen");
        _wordConceptRepository
            .Setup(r => r.FindWordByTextAsync("laufen", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Word { Id = 42, LanguageId = 1, WordConceptId = 10, Text = "laufen" });
        var command = new CreateUserCardCommand("laufen", "koşmak", null, null, null, null, Force: false, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.SuggestedSystemWordId.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Success_LogsCreateUserCardActivity()
    {
        // ARRANGE
        SetupNoDuplicateAndAggregate(7, "laufen");
        var command = new CreateUserCardCommand("laufen", "koşmak", null, null, null, null, Force: false, UserId: 7, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue=null (yeni kayıt), NewValue dolu.
        _activityLogger.Verify(l => l.LogAsync(
            7, "User", "CREATE_USER_CARD", "UserCard", 1, null, It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
