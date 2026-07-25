namespace WordLearner.Application.Interfaces.Services;

public interface IAccountCleanupService
{
    // Grace period'ı dolmuş hesapları anonimleştirir; kaç hesabın işlendiğini döner.
    Task<int> AnonymizeExpiredAccountsAsync(CancellationToken ct = default);
}
