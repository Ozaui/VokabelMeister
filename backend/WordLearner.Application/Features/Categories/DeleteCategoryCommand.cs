// ─────────────────────────────────────────────────────────────────────────────
// DeleteCategoryCommand.cs
//
// AMAÇ: DELETE /categories/{id} — bir Category'yi soft-delete eder.
// NEDEN: API_ENDPOINTS.md §6 "alt kategori/aktif kelime varsa silme 409" kuralı —
//        önce HasChildrenAsync (CategoryHasChildrenException), sonra HasActiveWordsAsync
//        (CategoryHasActiveWordsException) kontrol edilir. Category/CategoryTranslation
//        arasında WordConcept/Word'deki gibi CASCADE bir "birlikte soft-delete" YOK —
//        CategoryTranslationConfiguration'daki FK zaten Cascade (DB seviyesinde), ama
//        bu yalnızca gerçek DELETE'te devreye girer; soft-delete'te Category.IsDeleted
//        yeterli, Translation'lar kategori zaten silinmiş sayıldığı için hiçbir sorguda
//        AYRICA görünmez (WordCategory'nin aksine, CategoryTranslation'ın kendi başına
//        bir "aktif mi" anlamı yok, her zaman sahibi Category'nin durumuna bağlı okunur).
// BAĞIMLILIKLAR: ICategoryRepository, IActivityLogger.
// ─────────────────────────────────────────────────────────────────────────────

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
