namespace Zausel.Application.Interfaces.Services;

public interface IAdminSeedService
{
    Task SeedInitialAdminAsync(CancellationToken cancellationToken = default);
}
