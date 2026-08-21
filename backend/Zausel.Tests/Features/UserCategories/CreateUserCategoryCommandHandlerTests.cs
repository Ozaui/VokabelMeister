using AutoMapper;
using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Tests.Features.UserCategories;

public class CreateUserCategoryCommandHandlerTests
{
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserCategoryProfile>()).CreateMapper();

    private CreateUserCategoryCommandHandler CreateHandler() => new(_userCategoryRepository.Object, _mapper, _activityLogger.Object);

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserCategoryAndReturnsResponse()
    {
        // ARRANGE
        var command = new CreateUserCategoryCommand("Hayvanlar", "Ev hayvanları", "#FF6B00", "paw", UserId: 1, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Name.Should().Be("Hayvanlar");
        result.Description.Should().Be("Ev hayvanları");
        result.Color.Should().Be("#FF6B00");
        result.Icon.Should().Be("paw");
        _userCategoryRepository.Verify(r => r.AddAsync(It.Is<UserCategory>(c => c.UserId == 1 && c.Name == "Hayvanlar"), 1, It.IsAny<CancellationToken>()), Times.Once);
        _userCategoryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Success_LogsCreateUserCategoryActivity()
    {
        // ARRANGE
        var command = new CreateUserCategoryCommand("Hayvanlar", null, null, null, UserId: 7, ActorRole: "User");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT — OldValue=null (yeni kayıt), NewValue dolu
        _activityLogger.Verify(l => l.LogAsync(
            7, "User", "CREATE_USER_CATEGORY", "UserCategory", It.IsAny<int?>(), null, It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
