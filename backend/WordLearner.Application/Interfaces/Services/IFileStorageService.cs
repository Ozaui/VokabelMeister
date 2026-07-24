namespace WordLearner.Application.Interfaces.Services;

public interface IFileStorageService
{
    // Orijinal dosya adı asla olduğu gibi diske yazılmaz (çakışma + path traversal riski) —
    // yalnızca uzantısı korunur, Guid tabanlı benzersiz bir ad üretilir.
    Task<string> SaveImageAsync(
        Stream fileStream,
        string originalFileName,
        long fileSizeBytes,
        CancellationToken ct = default
    );
}
