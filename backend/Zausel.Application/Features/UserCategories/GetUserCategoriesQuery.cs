using AutoMapper;
using MediatR;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;

namespace Zausel.Application.Features.UserCategories;

public record GetUserCategoriesQuery(int UserId) : IRequest<List<UserCategoryResponse>>;

public class GetUserCategoriesQueryHandler : IRequestHandler<GetUserCategoriesQuery, List<UserCategoryResponse>>
{
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IMapper _mapper;

    public GetUserCategoriesQueryHandler(IUserCategoryRepository userCategoryRepository, IMapper mapper)
    {
        _userCategoryRepository = userCategoryRepository;
        _mapper = mapper;
    }

    public async Task<List<UserCategoryResponse>> Handle(GetUserCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userCategories = await _userCategoryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<List<UserCategoryResponse>>(userCategories);
    }
}
