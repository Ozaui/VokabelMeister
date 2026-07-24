// ─────────────────────────────────────────────────────────────────────────────
// GetAdminStatisticsQuery.cs
//
// AMAÇ: GET /admin/statistics — toplam kullanıcı/kelime/kategori, aktif/dondurulmuş
//       kullanıcı sayısı, son N günün kayıt grafiği için ham sayılar.
// NEDEN: `AdminStatisticsDto`/`IUserRepository.GetStatisticsAsync` bu Handler'dan ÖNCE
//        (A-07'nin Kullanıcı Yönetimi diliminde) bir kez yazılmış, tüketicisiz olduğu
//        kod denetiminde bulunup geri alınmıştı (bkz. TASK/A_admin_panel_backend.md
//        A-07 notu) — bu Handler o ikisinin GERÇEK, ilk tüketicisi.
//        LoginsByDay BİLİNÇLİ OLARAK YOK: `Users.LastLoginAt` yalnızca en son girişin
//        üzerine yazıldığı TEK bir alan, bir login-event geçmişi tablosu YOK — "son N
//        günün HER GÜNÜ kaç login oldu" sorusu mevcut şemayla cevaplanamaz.
// BAĞIMLILIKLAR: IUserRepository, IWordConceptRepository, ICategoryRepository.
// ─────────────────────────────────────────────────────────────────────────────

using MediatR;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Admin;

public record GetAdminStatisticsQuery(int DaysForGraph = 30) : IRequest<AdminStatisticsDto>;

public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWordConceptRepository _wordConceptRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetAdminStatisticsQueryHandler(
        IUserRepository userRepository,
        IWordConceptRepository wordConceptRepository,
        ICategoryRepository categoryRepository
    )
    {
        _userRepository = userRepository;
        _wordConceptRepository = wordConceptRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken ct)
    {
        var (totalUsers, activeUsers, frozenUsers) = await _userRepository.GetStatisticsAsync(ct);
        var totalWordConcepts = await _wordConceptRepository.GetTotalCountAsync(ct);
        var totalCategories = await _categoryRepository.GetTotalCountAsync(ct);

        var fromUtc = DateTime.UtcNow.Date.AddDays(-(request.DaysForGraph - 1));
        var registrationDates = await _userRepository.GetRegistrationDatesAsync(fromUtc, ct);

        // NEDEN bellekte gruplama: Repository katmanı ham CreatedAt listesini döner
        // (bkz. IUserRepository.GetRegistrationDatesAsync "NEDEN" notu) — burada
        // DateOnly'e indirgenip GroupBy ile günlere toplanır, sıfır kayıtlı günler de
        // (Count=0) listeye eklenir ki admin panelin grafiği boşluksuz, N gün uzunluğunda
        // bir eksen çizebilsin.
        var countsByDate = registrationDates
            .GroupBy(d => DateOnly.FromDateTime(d))
            .ToDictionary(g => g.Key, g => g.Count());

        var registrationsByDay = Enumerable
            .Range(0, request.DaysForGraph)
            .Select(offset =>
            {
                var date = DateOnly.FromDateTime(fromUtc).AddDays(offset);
                return new AdminDailyCountDto(date, countsByDate.GetValueOrDefault(date));
            })
            .ToList();

        return new AdminStatisticsDto(
            totalUsers,
            activeUsers,
            frozenUsers,
            totalWordConcepts,
            totalCategories,
            registrationsByDay
        );
    }
}
