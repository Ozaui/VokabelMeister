using WordLearner.Application.Interfaces.Services;

namespace WordLearner.API.BackgroundServices;

// Günde bir kez 03:00 UTC'de çalışır — trafiğin en düşük olduğu saat, toplu UPDATE'in kullanıcıyı
// etkileme ihtimali en az. Kaçırılan bir çalışma ertesi güne kalır, bu kabul edilebilir:
// anonimleştirme zaten 30 günlük bir grace period'ın sonunda yapılıyor.
public class AccountCleanupBackgroundService : BackgroundService
{
    private static readonly TimeSpan DailyRunTimeUtc = TimeSpan.FromHours(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountCleanupBackgroundService> _logger;

    public AccountCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AccountCleanupBackgroundService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(DelayUntilNextRun(DateTime.UtcNow), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                // Hosted service singleton, repository/DbContext scoped — her çalışma kendi scope'unu açar.
                // GetRequiredService de try içinde: DI çözümü bir gün başarısız olursa bile döngü ölmemeli.
                using var scope = _scopeFactory.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<IAccountCleanupService>();
                await cleanupService.AnonymizeExpiredAccountsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Yakalanmayan bir hata döngüyü sonlandırır ve servis bir daha HİÇ çalışmaz —
                // bir günlük başarısızlık, kalıcı olarak durmaktan iyidir.
                _logger.LogError(ex, "Account cleanup run failed; will retry tomorrow.");
            }
        }
    }

    private static TimeSpan DelayUntilNextRun(DateTime utcNow)
    {
        var todaysRun = utcNow.Date + DailyRunTimeUtc;
        var nextRun = utcNow < todaysRun ? todaysRun : todaysRun.AddDays(1);
        return nextRun - utcNow;
    }
}
