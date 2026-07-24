// ─────────────────────────────────────────────────────────────────────────────
// GetApplicationLogsQueryHandlerTests.cs
//
// AMAÇ: GetApplicationLogsQueryHandler'ın filtreleri repository'ye ilettiğini ve
//       entity alanlarını doğru DTO'ya taşıdığını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Logging;

namespace WordLearner.Tests.Features.Admin;

public class GetApplicationLogsQueryHandlerTests
{
    private readonly Mock<IApplicationLogRepository> _applicationLogRepo = new();

    private GetApplicationLogsQueryHandler CreateHandler() => new(_applicationLogRepo.Object);

    /// <summary>
    /// Handle_ForwardsFiltersAndMapsFields
    /// </summary>
    [Fact]
    public async Task Handle_ForwardsFiltersAndMapsFields()
    {
        // ARRANGE
        var log = new ApplicationLog
        {
            Id = 1,
            Level = "Error",
            Message = "Something failed",
            SourceContext = "WordLearner.Application.Features.Auth.LoginCommandHandler",
            RequestPath = "/api/v1/auth/login",
        };
        _applicationLogRepo
            .Setup(r => r.GetPagedAsync("Error", null, null, "failed", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ApplicationLog>(new List<ApplicationLog> { log }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetApplicationLogsQuery("Error", null, null, "failed"), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.Level == "Error" && i.RequestPath == "/api/v1/auth/login");
    }
}
