using FluentAssertions;
using Moq;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCards;

public class UpdateUserCardCommandHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UpdateUserCardCommandHandler CreateHandler() =>
        new(_userCardRepository.Object, _categoryRepository.Object, _userCategoryRepository.Object, _activityLogger.Object);

    private static UserCardAggregate Aggregate(UserCard card) => new(card, [], [], []);

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCardAggregate?)null);
        var command = new UpdateUserCardCommand(99, "laufen", "koşmak", null, true, null, null, null, Force: false, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _userCardRepository.Verify(r => r.UpdateAsync(It.IsAny<UserCard>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesAndReturnsResponse()
    {
        // ARRANGE
        var existing = new UserCard { Id = 1, UserId = 5, FrontText = "eski", BackText = "eskiArka", IsActive = true };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(Aggregate(existing));
        _userCardRepository
            .Setup(r => r.FindByUserAndFrontTextAsync(5, "laufen", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCard?)null);
        var command = new UpdateUserCardCommand(1, "laufen", "koşmak", "not", false, null, null, null, Force: false, UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.FrontText.Should().Be("laufen");
        result.IsActive.Should().BeFalse();
        _userCardRepository.Verify(r => r.UpdateAsync(It.Is<UserCard>(c => c.FrontText == "laufen" && c.BackText == "koşmak" && !c.IsActive), 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FrontTextMatchesAnotherOwnCard_ThrowsUserCardDuplicateException()
    {
        // ARRANGE — kendi Id'si HARİÇ (excludeUserCardId), AYNI kullanıcının BAŞKA bir kartı ile çakışıyor.
        var existing = new UserCard { Id = 1, UserId = 5, FrontText = "eski", BackText = "eskiArka" };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(Aggregate(existing));
        _userCardRepository
            .Setup(r => r.FindByUserAndFrontTextAsync(5, "laufen", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserCard { Id = 2, UserId = 5, FrontText = "laufen", BackText = "koşmak" });
        var command = new UpdateUserCardCommand(1, "laufen", "koşmak", null, true, null, null, null, Force: false, UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<UserCardDuplicateException>();
        _userCardRepository.Verify(r => r.UpdateAsync(It.IsAny<UserCard>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Success_LogsUpdateUserCardActivity()
    {
        // ARRANGE
        var existing = new UserCard { Id = 1, UserId = 5, FrontText = "eski", BackText = "eskiArka" };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(Aggregate(existing));
        _userCardRepository
            .Setup(r => r.FindByUserAndFrontTextAsync(5, "laufen", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCard?)null);
        var command = new UpdateUserCardCommand(1, "laufen", "koşmak", null, true, null, null, null, Force: false, UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — hem OldValue hem NewValue dolu (bir güncelleme, silme/oluşturma DEĞİL).
        _activityLogger.Verify(l => l.LogAsync(
            5, "User", "UPDATE_USER_CARD", "UserCard", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
