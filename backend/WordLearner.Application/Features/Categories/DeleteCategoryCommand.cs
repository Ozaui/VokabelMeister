using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;

namespace WordLearner.Application.Features.Categories;

public record DeleteCategoryCommand(int Id) : IRequest<Unit>
{
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IActivityLogger _activityLogger;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IActivityLogger activityLogger)
    {
        _categoryRepository = categoryRepository;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category =
            await _categoryRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(Category), request.Id);

        if (await _categoryRepository.HasChildrenAsync(request.Id, ct))
            throw new CategoryHasChildrenException();

        if (await _categoryRepository.HasActiveWordsAsync(request.Id, ct))
            throw new CategoryHasActiveWordsException();

        await _categoryRepository.SoftDeleteAsync(request.Id, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "DELETE_CATEGORY",
            entityType: "Category",
            entityId: request.Id,
            oldValue: new
            {
                category.ParentCategoryId,
                Translations = category.Translations.Select(t => new { LanguageCode = t.Language.Code, t.Name }),
            },
            ct: ct
        );

        return Unit.Value;
    }
}
