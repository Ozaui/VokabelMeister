using MediatR;
using Zausel.Application.DTOs;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;

namespace Zausel.Application.Features.UserCards;

public record GetUserCardsQuery(int UserId, int? CategoryId, int? UserCategoryId, string? Search, int Page, int PageSize)
    : IRequest<PagedResult<UserCardResponse>>;

public class GetUserCardsQueryHandler : IRequestHandler<GetUserCardsQuery, PagedResult<UserCardResponse>>
{
    private readonly IUserCardRepository _userCardRepository;

    public GetUserCardsQueryHandler(IUserCardRepository userCardRepository) => _userCardRepository = userCardRepository;

    public async Task<PagedResult<UserCardResponse>> Handle(GetUserCardsQuery request, CancellationToken cancellationToken)
    {
        var page = await _userCardRepository.GetPagedForUserAsync(
            request.UserId, request.CategoryId, request.UserCategoryId, request.Search, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<UserCardResponse>
        {
            Items = page.Items.Select(a => UserCardMapping.ToResponse(a)).ToList(),
            TotalCount = page.TotalCount,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }
}
