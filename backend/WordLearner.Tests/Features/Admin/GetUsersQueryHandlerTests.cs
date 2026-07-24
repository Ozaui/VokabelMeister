// ─────────────────────────────────────────────────────────────────────────────
// GetUsersQueryHandlerTests.cs
//
// AMAÇ: GetUsersQueryHandler'ın repository'den gelen sayfayı doğru DTO'ya
//       dönüştürüp ilettiğini doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Admin;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    private GetUsersQueryHandler CreateHandler() => new(_userRepo.Object);

    /// <summary>
    /// Handle_ForwardsFilterAndMapsItems
    /// </summary>
    [Fact]
    public async Task Handle_ForwardsFilterAndMapsItems()
    {
        // ARRANGE
        var user = new User
        {
            Id = 5,
            Email = "test@vokabelmeister.dev",
            FirstName = "Ada",
            LastName = "Lovelace",
            Role = "User",
            IsActive = true,
            IsEmailVerified = true,
        };
        _userRepo
            .Setup(r => r.GetPagedAsync("ada", "User", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<User>(new List<User> { user }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUsersQuery("ada", "User", 1, 20), default);

        // ASSERT
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(i => i.Id == 5 && i.Email == "test@vokabelmeister.dev" && i.Role == "User");
    }
}
