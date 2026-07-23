// ─────────────────────────────────────────────────────────────────────────────
// UpdateCategoryCommandHandlerTests.cs
//
// AMAÇ: UpdateCategoryCommandHandler'ın mevcut çevirileri güncellediğini, eksik
//       dili eklediğini, üst kategori 404/döngü korumasını ve UPDATE_CATEGORY
//       audit kaydını doğrulamak.
// BAĞIMLILIKLAR: xUnit, Moq, FluentAssertions.
// ─────────────────────────────────────────────────────────────────────────────

using System.Text.Json;
using FluentAssertions;
using Moq;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Features.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Tests.Features.Categories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<ILanguageRepository> _languageRepo = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UpdateCategoryCommandHandler CreateHandler() =>
        new(_categoryRepo.Object, _languageRepo.Object, _activityLogger.Object);

    private static readonly Language German = new() { Id = 1, Code = "de", Name = "German", NativeName = "Deutsch" };
    private static readonly Language Turkish = new() { Id = 2, Code = "tr", Name = "Turkish", NativeName = "Türkçe" };

    private static Category ExistingCategory() =>
        new()
        {
            Id = 10,
            ParentCategoryId = null,
            DisplayOrder = 1,
            Translations = new List<CategoryTranslation>
            {
                new()
                {
                    Id = 100,
                    LanguageId = German.Id,
                    Language = German,
                    Name = "Essen",
                },
            },
        };

    /// <summary>
    /// Update_ExistingTranslation_UpdatesName
    ///
    /// AMAÇ: Kategoride zaten var olan bir dilin Name'inin yerinde güncellendiğini
    ///       (yeni CategoryTranslation EKLENMEDİĞİNİ) doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_ExistingTranslation_UpdatesName()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            null,
            2,
            "food",
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen & Trinken", null) }
        );

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().ContainSingle(t => t.Name == "Essen & Trinken");
        category.Translations.Should().HaveCount(1);
    }

    /// <summary>
    /// Update_NewLanguageAdded_AddsSecondTranslation
    /// </summary>
    [Fact]
    public async Task Update_NewLanguageAdded_AddsSecondTranslation()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        _languageRepo.Setup(r => r.GetByCodeAsync("tr", It.IsAny<CancellationToken>())).ReturnsAsync(Turkish);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            null,
            1,
            null,
            null,
            null,
            null,
            new[]
            {
                new CategoryTranslationInput("de", "Essen", null),
                new CategoryTranslationInput("tr", "Yemek", null),
            }
        );

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Translations.Should().HaveCount(2);
    }

    /// <summary>
    /// Update_ParentNotFound_ThrowsEntityNotFoundException
    /// </summary>
    [Fact]
    public async Task Update_ParentNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            999,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    /// <summary>
    /// Update_ParentIsSelf_ThrowsCategoryParentCycleException
    ///
    /// AMAÇ: Bir kategorinin kendisini üst kategori olarak göstermeye çalışması
    ///       durumunda döngü korumasının devreye girdiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_ParentIsSelf_ThrowsCategoryParentCycleException()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            10,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryParentCycleException>();
    }

    /// <summary>
    /// Update_ParentIsDescendant_ThrowsCategoryParentCycleException
    ///
    /// AMAÇ: Bir kategorinin kendi alt ağacındaki bir kategoriye taşınmaya
    ///       çalışılması durumunda WouldCreateCycleAsync üzerinden döngü
    ///       korumasının devreye girdiğini doğrulamak.
    /// </summary>
    [Fact]
    public async Task Update_ParentIsDescendant_ThrowsCategoryParentCycleException()
    {
        // ARRANGE
        var category = ExistingCategory();
        var descendant = new Category { Id = 20, ParentCategoryId = 10 };
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepo.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(descendant);
        _categoryRepo.Setup(r => r.WouldCreateCycleAsync(10, 20, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            20,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        );

        // ACT
        var act = () => handler.Handle(command, default);

        // ASSERT
        await act.Should().ThrowAsync<CategoryParentCycleException>();
    }

    /// <summary>
    /// Update_Success_LogsUpdateCategoryActivity
    /// </summary>
    [Fact]
    public async Task Update_Success_LogsUpdateCategoryActivity()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            null,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen", null) }
        )
        {
            UserId = 7,
            ActorRole = "Admin",
        };

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _activityLogger.Verify(
            l =>
                l.LogAsync(
                    7,
                    "Admin",
                    "UPDATE_CATEGORY",
                    "Category",
                    10,
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    null,
                    null,
                    default
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Update_Success_OldValueSnapshotReflectsPreUpdateName
    ///
    /// AMAÇ: Audit log'a yazılan `oldValue.Translations`'ın, güncellemeden ÖNCEKİ
    ///       adı taşıdığını (SONRAKİ adı DEĞİL) doğrulamak — `category.Translations.
    ///       Select(...)` tembel (deferred) bir IEnumerable olduğu için `.ToList()`
    ///       ile materyalize edilmezse, LogAsync JSON'a serileştirirken bu listeyi
    ///       mutasyonlardan SONRA okur ve "eski" değer olarak YENİ değeri yazardı
    ///       (A-06 denetiminde bulunan regresyon — bu test o hatayı bir daha
    ///       fark edilmeden geri gelmesini önler).
    /// </summary>
    [Fact]
    public async Task Update_Success_OldValueSnapshotReflectsPreUpdateName()
    {
        // ARRANGE
        var category = ExistingCategory();
        _categoryRepo.Setup(r => r.GetWithTranslationsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _languageRepo.Setup(r => r.GetByCodeAsync("de", It.IsAny<CancellationToken>())).ReturnsAsync(German);
        object? capturedOldValue = null;
        _activityLogger
            .Setup(l =>
                l.LogAsync(
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<object?>(),
                    It.IsAny<object?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<int?, string?, string, string?, int?, object?, object?, string?, string?, CancellationToken>(
                (_, _, _, _, _, oldValue, _, _, _, _) => capturedOldValue = oldValue
            )
            .Returns(Task.CompletedTask);
        var handler = CreateHandler();
        var command = new UpdateCategoryCommand(
            10,
            null,
            1,
            null,
            null,
            null,
            null,
            new[] { new CategoryTranslationInput("de", "Essen & Trinken", null) }
        );

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        var json = JsonSerializer.Serialize(capturedOldValue);
        json.Should().Contain("Essen").And.NotContain("Essen & Trinken");
    }
}
