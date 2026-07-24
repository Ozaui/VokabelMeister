// ─────────────────────────────────────────────────────────────────────────────
// GetUsersQuery.cs
//
// AMAÇ: GET /admin/users — arama+role filtreli sayfalı kullanıcı listesi.
// NEDEN: GetWordsQuery/GetCategoriesQuery (A-05/A-06) ile AYNI ince Query+Handler
//        deseni — repository zaten filtrelenmiş sayfayı döner, Handler yalnızca
//        User→AdminUserListItemDto dönüşümünü yapar (AutoMapper Profile YAZILMADI,
//        alan sayısı azken elle inşa daha az dolaylılık — CLAUDE.md §3 "koşullu
//        AutoMapper" kuralı: gerçek bir entity→DTO map'i olsa da bu kadar basit bir
//        projeksiyonda Profile açmak gereksiz dolaylılık olurdu, WordConceptDtoBuilder
//        kararındaki gerekçeyle aynı ölçüde değil ama aynı yönde bir tercih).
// BAĞIMLILIKLAR: IUserRepository, PagedResult<T>.
// ─────────────────────────────────────────────────────────────────────────────

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
