using MediatR;
using WordLearner.Application.Common.Models;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Admin;

public record GetUsersQuery(string? Search, string? Role, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminUserListItemDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<AdminUserListItemDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<PagedResult<AdminUserListItemDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var paged = await _userRepository.GetPagedAsync(request.Search, request.Role, request.Page, request.PageSize, ct);

        return new PagedResult<AdminUserListItemDto>(
            paged.Items
                .Select(u => new AdminUserListItemDto(
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role,
                    u.IsActive,
                    u.IsEmailVerified,
                    u.CreatedAt,
                    u.LastLoginAt
                ))
                .ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize
        );
    }
}
