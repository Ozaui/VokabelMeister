using MediatR;
using Zausel.Application.DTOs.Categories;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Categories;

public record CreateCategoryCommand(
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    List<CategoryTranslationRequest> Translations,
    int? UserId,
    string? ActorRole) : IRequest<CategoryResponse>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository, ILanguageRepository languageRepository, IActivityLogger activityLogger)
    {
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId is not null
            && await _categoryRepository.GetAggregateAsync(request.ParentCategoryId.Value, cancellationToken) is null)
            throw new EntityNotFoundException($"Category not found: Id={request.ParentCategoryId}");

        var category = new Category
        {
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            Icon = request.Icon,
            Color = request.Color,
            MinLevel = request.MinLevel,
            MaxLevel = request.MaxLevel
        };
        await _categoryRepository.AddCategoryAsync(category, request.UserId, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        foreach (var translation in request.Translations)
        {
            var language = await _languageRepository.GetByCodeAsync(translation.LanguageCode, cancellationToken)
                ?? throw new EntityNotFoundException($"Language not found: Code={translation.LanguageCode}");

            await _categoryRepository.AddTranslationAsync(new CategoryTranslation
            {
                CategoryId = category.Id,
                LanguageId = language.Id,
                Name = translation.Name,
                Description = translation.Description
            }, cancellationToken);
        }

        await _categoryRepository.SaveChangesAsync(cancellationToken);

        var aggregate = await _categoryRepository.GetAggregateAsync(category.Id, cancellationToken)
            ?? throw new EntityNotFoundException($"Category not found after creation: Id={category.Id}");
        var response = CategoryMapping.ToResponse(aggregate);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "CREATE_CATEGORY", entityType: "Category", entityId: category.Id,
            oldValue: null, newValue: response, cancellationToken: cancellationToken);

        return response;
    }
}
