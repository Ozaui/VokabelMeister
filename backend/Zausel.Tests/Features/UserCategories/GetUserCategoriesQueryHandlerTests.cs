using AutoMapper;
using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Tests.Features.UserCategories;

public class GetUserCategoriesQueryHandlerTests
{
    private readonly Mock<IUserCategoryRepository> _userCategoryRepository = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserCategoryProfile>()).CreateMapper();

    private GetUserCategoriesQueryHandler CreateHandler() => new(_userCategoryRepository.Object, _mapper);

    [Fact]
    public async Task Handle_ReturnsOnlyRequestedUsersCategories()
    {
        // ARRANGE — sahiplik filtresi repository'nin GetByUserIdAsync'inde; Handler yalnızca UserId'yi iletir.
        _userCategoryRepository.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UserCategory { Id = 1, UserId = 1, Name = "Hayvanlar" }]);
        _userCategoryRepository.Setup(r => r.GetCardCountsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, int> { [1] = 3 });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUserCategoriesQuery(1), default);

        // ASSERT — CardCount, GetCardCountsAsync'in döndürdüğü sözlükten (ara tablo üzerinden hesaplanan) geliyor.
        result.Should().ContainSingle(c => c.Name == "Hayvanlar" && c.CardCount == 3);
        _userCategoryRepository.Verify(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoCategories_ReturnsEmptyList()
    {
        // ARRANGE
        _userCategoryRepository.Setup(r => r.GetByUserIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _userCategoryRepository.Setup(r => r.GetCardCountsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUserCategoriesQuery(2), default);

        // ASSERT
        result.Should().BeEmpty();
    }
}
