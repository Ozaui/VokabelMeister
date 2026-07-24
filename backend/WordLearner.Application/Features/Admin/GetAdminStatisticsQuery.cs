using MediatR;
using WordLearner.Application.DTOs.Admin;
using WordLearner.Application.Interfaces.Repositories;

namespace WordLearner.Application.Features.Admin;

// LoginsByDay bilinçli olarak yok — Users.LastLoginAt yalnızca en son girişin üzerine yazılır,
// bir login-event geçmişi tablosu olmadığı için "her gün kaç login oldu" mevcut şemayla cevaplanamaz.
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

        // Sıfır kayıtlı günler de (Count=0) listeye eklenir ki grafik boşluksuz, N gün uzunluğunda çizilsin.
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
