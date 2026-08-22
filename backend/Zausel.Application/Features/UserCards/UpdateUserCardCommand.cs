using MediatR;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCards;

public record UpdateUserCardCommand(
    int UserCardId,
    string FrontText,
    string BackText,
    string? Notes,
    bool IsActive,
    List<int>? CategoryIds,
    List<int>? UserCategoryIds,
    List<UserCardExampleRequest>? Examples,
    bool Force,
    int UserId,
    string? ActorRole) : IRequest<UserCardResponse>;

public class UpdateUserCardCommandHandler : IRequestHandler<UpdateUserCardCommand, UserCardResponse>
{
    private readonly IUserCardRepository _userCardRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IActivityLogger _activityLogger;

    public UpdateUserCardCommandHandler(
        IUserCardRepository userCardRepository, ICategoryRepository categoryRepository,
        IUserCategoryRepository userCategoryRepository, IActivityLogger activityLogger)
    {
        _userCardRepository = userCardRepository;
        _categoryRepository = categoryRepository;
        _userCategoryRepository = userCategoryRepository;
        _activityLogger = activityLogger;
    }

    public async Task<UserCardResponse> Handle(UpdateUserCardCommand request, CancellationToken cancellationToken)
    {
        // Sahiplik filtresi burada gömülü — başkasının kartı da AYNI 404'ü döner.
        var existingAggregate = await _userCardRepository.GetByIdForUserAsync(request.UserCardId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found: Id={request.UserCardId}");
        var beforeResponse = UserCardMapping.ToResponse(existingAggregate);

        var categoryIds = request.CategoryIds ?? [];
        var userCategoryIds = request.UserCategoryIds ?? [];

        if (categoryIds.Count > 0 && !await _categoryRepository.AllExistAsync(categoryIds, cancellationToken))
            throw new EntityNotFoundException($"One or more categories not found: Ids={string.Join(",", categoryIds)}");
        if (userCategoryIds.Count > 0 && !await _userCategoryRepository.AllExistForUserAsync(userCategoryIds, request.UserId, cancellationToken))
            throw new EntityNotFoundException($"One or more user categories not found: Ids={string.Join(",", userCategoryIds)}");

        if (!request.Force)
        {
            var duplicate = await _userCardRepository.FindByUserAndFrontTextAsync(
                request.UserId, request.FrontText, excludeUserCardId: request.UserCardId, cancellationToken);
            if (duplicate is not null)
                throw new UserCardDuplicateException(request.FrontText);
        }

        var card = existingAggregate.Card;
        card.FrontText = request.FrontText;
        card.BackText = request.BackText;
        card.Notes = request.Notes;
        card.IsActive = request.IsActive;
        await _userCardRepository.UpdateAsync(card, request.UserId, cancellationToken);

        await _userCardRepository.ReplaceCategoriesAsync(card.Id, categoryIds, cancellationToken);
        await _userCardRepository.ReplaceUserCategoriesAsync(card.Id, userCategoryIds, cancellationToken);
        await _userCardRepository.ReplaceExamplesAsync(card.Id, ToExamples(card.Id, request.Examples), cancellationToken);
        await _userCardRepository.SaveChangesAsync(cancellationToken);

        var updatedAggregate = await _userCardRepository.GetByIdForUserAsync(card.Id, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found after update: Id={card.Id}");
        var response = UserCardMapping.ToResponse(updatedAggregate);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPDATE_USER_CARD", entityType: "UserCard", entityId: card.Id,
            oldValue: beforeResponse, newValue: response, cancellationToken: cancellationToken);

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
