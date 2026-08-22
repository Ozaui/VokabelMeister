using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCards;

public class GetUserCardByIdQueryHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();

    private GetUserCardByIdQueryHandler CreateHandler() => new(_userCardRepository.Object);

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE — başkasının kartı da AYNI 404'ü döner (sahiplik filtresi repository'de gömülü).
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCardAggregate?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetUserCardByIdQuery(99, 1), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_Owned_ReturnsResponse()
    {
        // ARRANGE
        var card = new UserCard { Id = 1, UserId = 5, FrontText = "laufen", BackText = "koşmak" };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCardAggregate(card, [], [], []));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUserCardByIdQuery(1, 5), default);

        // ASSERT
        result.FrontText.Should().Be("laufen");
    }
}
