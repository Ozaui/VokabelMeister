using FluentAssertions;
using Moq;
using Zausel.Application.DTOs;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Tests.Features.UserCards;

public class GetUserCardsQueryHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();

    private GetUserCardsQueryHandler CreateHandler() => new(_userCardRepository.Object);

    [Fact]
    public async Task Handle_PassesFiltersToRepositoryAndMapsResults()
    {
        // ARRANGE — sahiplik filtresi repository'nin GetPagedForUserAsync'inde; Handler yalnızca parametreleri iletir.
        var card = new UserCard { Id = 1, UserId = 1, FrontText = "laufen", BackText = "koşmak" };
        _userCardRepository
            .Setup(r => r.GetPagedForUserAsync(1, 2, 3, "lauf", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UserCardAggregate>
            {
                Items = [new UserCardAggregate(card, [], [], [])],
                TotalCount = 1,
                Page = 1,
                PageSize = 20
            });
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUserCardsQuery(1, 2, 3, "lauf", 1, 20), default);

        // ASSERT
        result.Items.Should().ContainSingle(c => c.FrontText == "laufen");
        result.TotalCount.Should().Be(1);
    }
}
