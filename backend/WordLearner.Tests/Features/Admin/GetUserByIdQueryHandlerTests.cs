// ─────────────────────────────────────────────────────────────────────────────
// GetUserByIdQueryHandlerTests.cs
//
// AMAÇ: GetUserByIdQueryHandler'ın bulunamayan kullanıcı için 404, bulunanı için
//       doğru alanları taşıyan AdminUserDetailDto döndürdüğünü doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Tests.Features.Admin;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();

    private GetUserByIdQueryHandler CreateHandler() => new(_userRepo.Object);

    /// <summary>
    /// Handle_UserNotFound_ThrowsEntityNotFoundException
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetUserByIdQuery(99), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Handle_UserFound_ReturnsDetailDto
    /// </summary>
    [Fact]
    public async Task Handle_UserFound_ReturnsDetailDto()
    {
        // ARRANGE
        var user = new User
        {
            Id = 5,
            Email = "test@vokabelmeister.dev",
            FirstName = "Ada",
            LastName = "Lovelace",
            Role = "Admin",
            IsActive = true,
            TotalXP = 100,
            LifetimeXP = 500,
            StreakDays = 3,
            LoginCount = 10,
        };
        _userRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetUserByIdQuery(5), default);

        // ASSERT
        result.Id.Should().Be(5);
        result.Role.Should().Be("Admin");
        result.LifetimeXP.Should().Be(500);
        result.LoginCount.Should().Be(10);
    }
}
