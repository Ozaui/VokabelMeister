using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCards;

// Force — Controller'da ?force=true query string'inden okunur (CreateWordCommand ile AYNI desen),
// Request DTO'sunun bir alanı DEĞİL.
public record CreateUserCardCommand(
    string FrontText,
    string BackText,
    string? Notes,
    List<int>? CategoryIds,
    List<int>? UserCategoryIds,
    List<UserCardExampleRequest>? Examples,
    bool Force,
    int UserId,
    string? ActorRole) : IRequest<UserCardResponse>;

public class CreateUserCardCommandHandler : IRequestHandler<CreateUserCardCommand, UserCardResponse>
{
    private readonly IUserCardRepository _userCardRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly IActivityLogger _activityLogger;

    public CreateUserCardCommandHandler(
        IUserCardRepository userCardRepository, ICategoryRepository categoryRepository,
        IUserCategoryRepository userCategoryRepository, IWordConceptRepository wordConceptRepository,
        IActivityLogger activityLogger)
    {
        _userCardRepository = userCardRepository;
        _categoryRepository = categoryRepository;
        _userCategoryRepository = userCategoryRepository;
        _wordConceptRepository = wordConceptRepository;
        _activityLogger = activityLogger;
    }

    public async Task<UserCardResponse> Handle(CreateUserCardCommand request, CancellationToken cancellationToken)
    {
        var categoryIds = request.CategoryIds ?? [];
        var userCategoryIds = request.UserCategoryIds ?? [];

        if (categoryIds.Count > 0 && !await _categoryRepository.AllExistAsync(categoryIds, cancellationToken))
            throw new EntityNotFoundException($"One or more categories not found: Ids={string.Join(",", categoryIds)}");
        if (userCategoryIds.Count > 0 && !await _userCategoryRepository.AllExistForUserAsync(userCategoryIds, request.UserId, cancellationToken))
            throw new EntityNotFoundException($"One or more user categories not found: Ids={string.Join(",", userCategoryIds)}");

        if (!request.Force)
        {
            var duplicate = await _userCardRepository.FindByUserAndFrontTextAsync(request.UserId, request.FrontText, excludeUserCardId: null, cancellationToken);
            if (duplicate is not null)
                throw new UserCardDuplicateException(request.FrontText);
        }

        var card = new UserCard
        {
            UserId = request.UserId,
            FrontText = request.FrontText,
            BackText = request.BackText,
            Notes = request.Notes
        };
        await _userCardRepository.AddAsync(card, request.UserId, cancellationToken);
        await _userCardRepository.SaveChangesAsync(cancellationToken);

        await _userCardRepository.ReplaceCategoriesAsync(card.Id, categoryIds, cancellationToken);
        await _userCardRepository.ReplaceUserCategoriesAsync(card.Id, userCategoryIds, cancellationToken);
        await _userCardRepository.ReplaceExamplesAsync(card.Id, ToExamples(card.Id, request.Examples), cancellationToken);
        await _userCardRepository.SaveChangesAsync(cancellationToken);

        // Sistem eşleşmesi — dile bakılmaksızın, engelleyici DEĞİL yalnızca bilgilendirici bir öneri
        // (aşağıdaki kartın kendisi FORCE gerekmeden zaten oluşturuldu).
        var suggestedWord = await _wordConceptRepository.FindWordByTextAsync(request.FrontText, cancellationToken);

        var aggregate = await _userCardRepository.GetByIdForUserAsync(card.Id, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found after creation: Id={card.Id}");
        var response = UserCardMapping.ToResponse(aggregate, suggestedWord?.Id);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "CREATE_USER_CARD", entityType: "UserCard", entityId: card.Id,
            oldValue: null, newValue: response, cancellationToken: cancellationToken);

        return response;
    }

    private static List<UserCardExample> ToExamples(int userCardId, List<UserCardExampleRequest>? examples) =>
        (examples ?? []).Select((example, index) => new UserCardExample
        {
            UserCardId = userCardId,
            SentenceFront = example.SentenceFront,
            SentenceBack = example.SentenceBack,
            DisplayOrder = index
        }).ToList();
}
