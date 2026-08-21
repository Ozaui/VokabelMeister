using FluentAssertions;
using Moq;
using Zausel.Application.Features.Progress;
using Zausel.Application.Interfaces.Repositories.Srs;

namespace Zausel.Tests.Features.Progress;

public class GetSuspendedWordsQueryHandlerTests
{
    private readonly Mock<IUserProgressRepository> _userProgressRepository = new();

    private GetSuspendedWordsQueryHandler CreateHandler() => new(_userProgressRepository.Object);

    [Fact]
    public async Task Handle_RepositoryReturnsSuspendedItems_MapsToResponseWithConsecutiveIncorrect()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetSuspendedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new WordProgressItem(5, "laufen", "koşmak", 2, 18m, null, true, ConsecutiveIncorrect: 6)
        ]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetSuspendedWordsQuery(1), default);

        // ASSERT
        result.Should().ContainSingle();
        result[0].WordId.Should().Be(5);
        result[0].ConsecutiveIncorrect.Should().Be(6);
    }

    [Fact]
    public async Task Handle_NoSuspendedWords_ReturnsEmptyList()
    {
        // ARRANGE
        _userProgressRepository.Setup(r => r.GetSuspendedAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetSuspendedWordsQuery(1), default);

        // ASSERT
        result.Should().BeEmpty();
    }
}
