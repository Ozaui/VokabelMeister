using FluentAssertions;
using Moq;
using WordLearner.Application.Features.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Words;

public class GetLanguagesQueryHandlerTests
{
    private readonly Mock<ILanguageRepository> _languageRepo = new();

    private GetLanguagesQueryHandler CreateHandler() => new(_languageRepo.Object);

    [Fact]
    public async Task GetLanguages_ReturnsActiveLanguagesMappedToDto()
    {
        // ARRANGE
        _languageRepo
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<Language>
                {
                    new()
                    {
                        Id = 1,
                        Code = "de",
                        Name = "German",
                        NativeName = "Deutsch",
                    },
                    new()
                    {
                        Id = 2,
                        Code = "tr",
                        Name = "Turkish",
                        NativeName = "Türkçe",
                    },
                }
            );
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetLanguagesQuery(), default);

        // ASSERT
        result.Should().HaveCount(2);
        result.Should().ContainSingle(l => l.Id == 1 && l.Code == "de" && l.NativeName == "Deutsch");
        result.Should().ContainSingle(l => l.Id == 2 && l.Code == "tr" && l.NativeName == "Türkçe");
    }
}
