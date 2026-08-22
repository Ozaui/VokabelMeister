using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCards;

public class DeleteUserCardCommandHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private DeleteUserCardCommandHandler CreateHandler() => new(_userCardRepository.Object, _activityLogger.Object);

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCardAggregate?)null);
        var command = new DeleteUserCardCommand(99, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _userCardRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<UserCard>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Owned_SoftDeletesAndLogsActivity()
    {
        // ARRANGE
        var existing = new UserCard { Id = 1, UserId = 5, FrontText = "laufen", BackText = "koşmak" };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCardAggregate(existing, [], [], []));
        var command = new DeleteUserCardCommand(1, UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue dolu, NewValue null (DELETE_USER_CATEGORY ile AYNI desen).
        _userCardRepository.Verify(r => r.SoftDeleteAsync(existing, 5, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(l => l.LogAsync(
            5, "User", "DELETE_USER_CARD", "UserCard", 1, It.IsAny<object>(), null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
