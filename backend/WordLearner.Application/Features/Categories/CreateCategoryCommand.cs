// ─────────────────────────────────────────────────────────────────────────────
// CreateCategoryCommand.cs
//
// AMAÇ: POST /categories — bir Category'yi 1+ dilde (translations[]) tek
//       işlemde oluşturur.
// NEDEN: ParentCategoryId verilirse önce VAR OLUP OLMADIĞI kontrol edilir —
//        var olmayan bir üst kategoriye bağlanmaya çalışmak (yazım hatası/silinmiş
//        Id) 404 ile REDDEDİLİR, DB'de "hayalet" bir FK bırakılmaz.
// BAĞIMLILIKLAR: ICategoryRepository, ILanguageRepository, IActivityLogger, CategoryDtoBuilder.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Categories;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Categories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Categories;

// AMAÇ: Bir dildeki kategori adı/kısa açıklama girdisi.
public record CategoryTranslationInput(string LanguageCode, string Name, string? Description);

public record CreateCategoryCommand(
    int? ParentCategoryId,
    int DisplayOrder,
    string? Icon,
    string? Color,
    string? MinLevel,
    string? MaxLevel,
    IReadOnlyList<CategoryTranslationInput> Translations
) : IRequest<CategoryDto>
{
    // NEDEN init-property: JWT'den (CurrentUserId/Role) gelir, gövdede yer almaz —
    //        controller model binding'den SONRA `with` ile ekler (CreateWordCommand deseni).
    public int? UserId { get; init; }
    public string? ActorRole { get; init; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IActivityLogger _activityLogger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILanguageRepository languageRepository,
        IActivityLogger activityLogger
    )
    {
        _categoryRepository = categoryRepository;
        _languageRepository = languageRepository;
        _activityLogger = activityLogger;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        if (request.ParentCategoryId is not null)
            _ =
                await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new EntityNotFoundException(typeof(Category), request.ParentCategoryId.Value);

        var category = new Category
        {
            ParentCategoryId = request.ParentCategoryId,
            DisplayOrder = request.DisplayOrder,
            Icon = request.Icon,
            Color = request.Color,
            MinLevel = request.MinLevel,
            MaxLevel = request.MaxLevel,
        };

        foreach (var translation in request.Translations)
        {
            var language =
                await _languageRepository.GetByCodeAsync(translation.LanguageCode, ct)
                ?? throw new EntityNotFoundException(typeof(Language), translation.LanguageCode);

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
        }

        await _categoryRepository.AddAsync(category, request.UserId, ct);

        await _activityLogger.LogAsync(
            request.UserId,
            request.ActorRole,
            "CREATE_CATEGORY",
            entityType: "Category",
            entityId: category.Id,
            newValue: new
            {
                category.ParentCategoryId,
                category.DisplayOrder,
                Translations = request.Translations.Select(t => new { t.LanguageCode, t.Name }),
            },
            ct: ct
        );

        return CategoryDtoBuilder.BuildSingle(category);
    }
}
