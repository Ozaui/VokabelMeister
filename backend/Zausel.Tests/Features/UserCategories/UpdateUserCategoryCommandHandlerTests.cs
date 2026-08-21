using AutoMapper;
using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCategories;

public class UpdateUserCategoryCommandHandlerTests
{
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserCategoryProfile>()).CreateMapper();

    private UpdateUserCategoryCommandHandler CreateHandler() => new(_userCategoryRepository.Object, _mapper, _activityLogger.Object);

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE — repository sahiplik filtresini kendi içinde uygular, başkasının kategorisi de burada null döner.
        _userCategoryRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCategory?)null);
        var command = new UpdateUserCategoryCommand(99, "Hayvanlar", null, null, null, UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFieldsAndReturnsResponse()
    {
        // ARRANGE
        var existing = new UserCategory { Id = 1, UserId = 1, Name = "Eski", Color = "#000000" };
        _userCategoryRepository.Setup(r => r.GetByIdForUserAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new UpdateUserCategoryCommand(1, "Yeni", "Açıklama", "#FFFFFF", "star", UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Name.Should().Be("Yeni");
        result.Color.Should().Be("#FFFFFF");
        _userCategoryRepository.Verify(r => r.UpdateAsync(existing, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Success_LogsUpdateUserCategoryActivity()
    {
        // ARRANGE
        var existing = new UserCategory { Id = 1, UserId = 7, Name = "Eski" };
        _userCategoryRepository.Setup(r => r.GetByIdForUserAsync(1, 7, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        var command = new UpdateUserCategoryCommand(1, "Yeni", null, null, null, UserId: 7, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue ve NewValue ikisi de dolu
        _activityLogger.Verify(l => l.LogAsync(
            7, "User", "UPDATE_USER_CATEGORY", "UserCategory", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
