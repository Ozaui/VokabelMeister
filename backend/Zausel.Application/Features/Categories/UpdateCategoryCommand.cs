using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Categories;

public record UpdateCategoryCommand(
    int CategoryId,
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    List<CategoryTranslationRequest> Translations,
    int? UserId,
    string? ActorRole) : IRequest<CategoryResponse>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository, ILanguageRepository languageRepository, IActivityLogger activityLogger)
    {
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var beforeAggregate = await _categoryRepository.GetAggregateAsync(request.CategoryId, cancellationToken)
            ?? throw new EntityNotFoundException($"Category not found: Id={request.CategoryId}");
        var beforeResponse = CategoryMapping.ToResponse(beforeAggregate);
        var category = beforeAggregate.Category;

        // Kategori TAŞIMA — ParentCategoryId gerçekten değişiyorsa döngü kontrolü + var olma kontrolü.
        if (request.ParentCategoryId != category.ParentCategoryId)
        {
            if (request.ParentCategoryId is not null)
            {
                if (await _categoryRepository.GetAggregateAsync(request.ParentCategoryId.Value, cancellationToken) is null)
                    throw new EntityNotFoundException($"Category not found: Id={request.ParentCategoryId}");
                if (await _categoryRepository.WouldCreateCycleAsync(request.CategoryId, request.ParentCategoryId.Value, cancellationToken))
                    throw new CategoryParentCycleException(request.CategoryId, request.ParentCategoryId.Value);
            }

            category.ParentCategoryId = request.ParentCategoryId;
        }

        category.DisplayOrder = request.DisplayOrder;
        category.Icon = request.Icon;
        category.Color = request.Color;
        category.MinLevel = request.MinLevel;
        category.MaxLevel = request.MaxLevel;
        await _categoryRepository.UpdateCategoryAsync(category, request.UserId, cancellationToken);

        foreach (var translation in request.Translations)
        {
            var language = await _languageRepository.GetByCodeAsync(translation.LanguageCode, cancellationToken)
                ?? throw new EntityNotFoundException($"Language not found: Code={translation.LanguageCode}");

            var existing = await _categoryRepository.FindTranslationAsync(category.Id, language.Id, cancellationToken);
            if (existing is null)
            {
                await _categoryRepository.AddTranslationAsync(new CategoryTranslation
                {
                    CategoryId = category.Id,
                    LanguageId = language.Id,
                    Name = translation.Name,
                    Description = translation.Description
                }, cancellationToken);
            }
            else
            {
                existing.Name = translation.Name;
                existing.Description = translation.Description;
                await _categoryRepository.UpdateTranslationAsync(existing, cancellationToken);
            }
        }

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        var aggregate = await _categoryRepository.GetAggregateAsync(category.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Category not found after update: Id={category.Id}");
        var response = CategoryMapping.ToResponse(aggregate);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPDATE_CATEGORY", entityType: "Category", entityId: category.Id,
            oldValue: beforeResponse, newValue: response, cancellationToken: cancellationToken);

        return response;
    }
}
