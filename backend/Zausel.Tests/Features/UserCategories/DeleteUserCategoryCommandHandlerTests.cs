using AutoMapper;
using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCategories;

public class DeleteUserCategoryCommandHandlerTests
{
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserCategoryProfile>()).CreateMapper();

    private DeleteUserCategoryCommandHandler CreateHandler() => new(_userCategoryRepository.Object, _mapper, _activityLogger.Object);

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userCategoryRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCategory?)null);
        var command = new DeleteUserCategoryCommand(99, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _userCategoryRepository.Verify(r => r.SoftDeleteAsync(It.IsAny<UserCategory>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Owned_SoftDeletesAndLogsActivity()
    {
        // ARRANGE
        var existing = new UserCategory { Id = 1, UserId = 5, Name = "Hayvanlar" };
        _userCategoryRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new DeleteUserCategoryCommand(1, UserId: 5, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue dolu, NewValue null (DELETE_CATEGORY ile AYNI desen)
        _userCategoryRepository.Verify(r => r.SoftDeleteAsync(existing, 5, It.IsAny<CancellationToken>()), Times.Once);
        _activityLogger.Verify(l => l.LogAsync(
            5, "User", "DELETE_USER_CATEGORY", "UserCategory", 1, It.IsAny<object>(), null, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
