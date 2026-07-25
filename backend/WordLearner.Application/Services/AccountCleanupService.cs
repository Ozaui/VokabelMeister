using Microsoft.Extensions.Logging;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Application.Interfaces.Services;
using WordLearner.Domain.Entities.Auth;

namespace WordLearner.Application.Services;

// Anonimleştirme mantığı BackgroundService'in değil bu servisin içinde — zamanlamadan bağımsız
// olarak birim testi yazılabilsin ve gerekirse elle de tetiklenebilsin diye.
public class AccountCleanupService : IAccountCleanupService
{
    // Anonimleştirilen hesabın adı — kullanıcı silinse de sınıf/paylaşım listelerinde
    // bir isim alanı görünmeye devam eder, boş bırakılamaz.
    private const string AnonymizedName = "Silindi";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IActivityLogger _activityLogger;
    private readonly ILogger<AccountCleanupService> _logger;

    public AccountCleanupService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IActivityLogger activityLogger,
        ILogger<AccountCleanupService> logger
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<int> AnonymizeExpiredAccountsAsync(CancellationToken ct = default)
    {
        var expiredUsers = await _userRepository.GetPendingAnonymizationAsync(DateTime.UtcNow, ct);

        foreach (var user in expiredUsers)
        {
            Anonymize(user);
            await _userRepository.UpdateAsync(user, userId: null, ct);

            // ActorRole null — bu değişikliği bir kişi değil, sistem/zamanlanmış görev yaptı.
            // OldValue yazılmaz: anonimleştirmenin sildiği PII'yi log tablosuna kopyalamak olurdu.
            await _activityLogger.LogAsync(
                user.Id,
                actorRole: null,
                action: "ANONYMIZE_ACCOUNT",
                entityType: nameof(User),
                entityId: user.Id,
                newValue: new { IsAnonymized = true },
                ct: ct
            );
        }

        if (expiredUsers.Count > 0)
            _logger.LogInformation("Anonymized {Count} expired account(s).", expiredUsers.Count);

        return expiredUsers.Count;
    }

    private void Anonymize(User user)
    {
        // OriginalEmailHash ÖNCE hesaplanır — Email üzerine yazıldıktan sonra gerçek adres
        // hiçbir yerde kalmaz, tekrar kayıt engeli bu tek referansa dayanır.
        user.OriginalEmailHash = _passwordService.HashToken(user.Email);
        user.Email = $"deleted_{user.Id}@deleted.invalid";
        user.FirstName = AnonymizedName;
        user.LastName = AnonymizedName;
        user.DisplayName = null;

        user.PasswordHash = null;
        user.GoogleId = null;
        user.AppleId = null;

        // Avatar bir fotoğraf, IP ve cihaz kimliği de KVKK/GDPR anlamında kişisel veri —
        // hepsi "kalıcı silme" sözünün kapsamında.
        user.AvatarUrl = null;
        user.LastLoginIP = null;
        user.OneSignalPlayerId = null;

        user.PendingOtpCodeHash = null;
        user.PendingOtpCodeExpiresAt = null;
        user.PendingOtpCodePurpose = null;

        user.IsActive = false;
        user.IsAnonymized = true;
    }
}
