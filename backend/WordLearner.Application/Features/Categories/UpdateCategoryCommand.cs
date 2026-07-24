using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Categories;

public record UpdateCategoryCommand(
    int Id,
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    IReadOnlyList<CategoryTranslationInput> Translations
) : IRequest<CategoryDto>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILanguageRepository languageRepository,
        IActivityLogger activityLogger
    )
    {
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category =
            await _categoryRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        // .ToList() — bkz. UpdateWordCommand.cs'teki aynı NEDEN notu (deferred LINQ + mutasyon).
        var oldValue = new
        {
            category.ParentCategoryId,
            category.DisplayOrder,
            Translations = category.Translations.Select(t => new { LanguageCode = t.Language.Code, t.Name }).ToList(),
        };

        if (request.ParentCategoryId is not null && request.ParentCategoryId != category.ParentCategoryId)
        {
            _ =
                await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new EntityNotFoundException(typeof(Category), request.ParentCategoryId.Value);

            if (
                request.ParentCategoryId == category.Id
                || await _categoryRepository.WouldCreateCycleAsync(category.Id, request.ParentCategoryId.Value, ct)
            )
                throw new CategoryParentCycleException();
        }

        category.ParentCategoryId = request.ParentCategoryId;
        category.DisplayOrder = request.DisplayOrder;
        category.Icon = request.Icon;
        category.Color = request.Color;
        category.MinLevel = request.MinLevel;
        category.MaxLevel = request.MaxLevel;

        foreach (var translation in request.Translations)
        {
            var language =
                await _languageRepository.GetByCodeAsync(translation.LanguageCode, ct)
                ?? throw new EntityNotFoundException(typeof(Language), translation.LanguageCode);

            var existing = category.Translations.FirstOrDefault(t => t.LanguageId == language.Id);

            if (existing is null)
            {
                category.Translations.Add(
                    new CategoryTranslation
                    {
                        Language = language,
                        Name = translation.Name,
                        Description = translation.Description,
                        CreatedByUserId = request.UserId,
                        UpdatedByUserId = request.UserId,
                    }
                );
                continue;
            }

            existing.Name = translation.Name;
            existing.Description = translation.Description;
            existing.UpdatedByUserId = request.UserId;
        }

        await _categoryRepository.UpdateAsync(category, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "UPDATE_CATEGORY",
            entityType: "Category",
            entityId: category.Id,
            oldValue: oldValue,
            newValue: new
            {
                category.ParentCategoryId,
                category.DisplayOrder,
                Translations = category.Translations.Select(t => new { LanguageCode = t.Language.Code, t.Name }),
            },
            ct: ct
        );

        return CategoryDtoBuilder.BuildSingle(category);
    }
}
