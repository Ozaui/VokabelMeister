// ─────────────────────────────────────────────────────────────────────────────
// GetCategoryWordsQueryHandlerTests.cs
//
// AMAÇ: GetCategoryWordsQueryHandler'ın kategori 404'ünü ve IWordConceptRepository.
//       GetPagedAsync'e categoryId'yi doğru ilettiğini doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Common.Models;
using WordLearner.Application.Features.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Categories;

public class GetCategoryWordsQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<IWordConceptRepository> _wordConceptRepo = new();

    private GetCategoryWordsQueryHandler CreateHandler() => new(_categoryRepo.Object, _wordConceptRepo.Object);

    /// <summary>
    /// GetCategoryWords_CategoryNotFound_ThrowsEntityNotFoundException
    /// </summary>
    [Fact]
    public async Task GetCategoryWords_CategoryNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _categoryRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(new GetCategoryWordsQuery(10), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// GetCategoryWords_CategoryExists_ForwardsCategoryIdAndReturnsMappedPage
    /// </summary>
    [Fact]
    public async Task GetCategoryWords_CategoryExists_ForwardsCategoryIdAndReturnsMappedPage()
    {
        // ARRANGE
        _categoryRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new Category { Id = 10 });
        var concept = new WordConcept
        {
            Id = 5,
            PartOfSpeech = "Noun",
            DifficultyLevel = "A1",
            Words = new List<Word>
            {
                new()
                {
                    LanguageId = 1,
                    Language = new Language { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" },
                    Text = "Apfel",
                },
            },
        };
        _wordConceptRepo
            .Setup(r => r.GetPagedAsync(null, null, null, 10, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<WordConcept>(new List<WordConcept> { concept }, 1, 1, 20));
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(new GetCategoryWordsQuery(10), default);

        // ASSERT
        result.Items.Should().ContainSingle(i => i.WordConceptId == 5);
    }
}
