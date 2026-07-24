// ─────────────────────────────────────────────────────────────────────────────
// GetUserByIdQuery.cs
//
// AMAÇ: GET /admin/users/{id} — bir kullanıcının detay+istatistik görünümü.
// NEDEN: GetWordByIdQuery/GetCategoryWordsQuery ile AYNI desen — bulunamazsa
//        EntityNotFoundException (404), soft-delete'li/anonimleştirilmiş bir hesap
//        GetByIdAsync'in normal soft-delete filtresiyle zaten görünmez (admin bu
//        ekrandan silinmiş bir hesabı YÖNETEMEZ — GetByIdIncludingDeletedAsync
//        BİLEREK kullanılmadı, o yalnızca Auth/QrLogin'in kurtarma akışlarına özgü).
// BAĞIMLILIKLAR: IUserRepository, EntityNotFoundException.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Features.Admin;

public record GetUserByIdQuery(int Id) : IRequest<AdminUserDetailDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AdminUserDetailDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<AdminUserDetailDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user =
            await _userRepository.GetByIdAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(User), request.Id);

        return new AdminUserDetailDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.DisplayName,
            user.Role,
            user.IsActive,
            user.IsEmailVerified,
            user.CurrentLevel,
            user.ThemePreference,
            user.AuthProvider,
            user.TotalXP,
            user.LifetimeXP,
            user.StreakDays,
            user.LoginCount,
            user.LastLoginAt,
            user.LastLoginIP,
            user.CreatedAt
        );
    }
}
