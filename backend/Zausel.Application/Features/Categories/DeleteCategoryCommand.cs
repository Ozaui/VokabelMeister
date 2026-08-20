using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Categories;

public record DeleteCategoryCommand(int CategoryId, int? UserId, string? ActorRole) : IRequest<Unit>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IActivityLogger _activityLogger;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IActivityLogger activityLogger)
    {
        _categoryRepository = categoryRepository;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var beforeAggregate = await _categoryRepository.GetAggregateAsync(request.CategoryId, cancellationToken)
            ?? throw new EntityNotFoundException($"Category not found: Id={request.CategoryId}");
        var beforeResponse = CategoryMapping.ToResponse(beforeAggregate);

        // Silme koruması — çocuğu olan VEYA aktif kelimesi olan kategori silinemez (A_backend.md
        // A-06 notu: "orphan terfi" YOK, korunan davranış budur).
        if (await _categoryRepository.HasChildrenAsync(request.CategoryId, cancellationToken))
            throw new CategoryHasChildrenException(request.CategoryId);
        if (await _categoryRepository.HasActiveWordsAsync(request.CategoryId, cancellationToken))
            throw new CategoryHasActiveWordsException(request.CategoryId);

        await _categoryRepository.SoftDeleteCategoryAsync(request.CategoryId, request.UserId, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "DELETE_CATEGORY", entityType: "Category", entityId: request.CategoryId,
            oldValue: beforeResponse, newValue: null, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
