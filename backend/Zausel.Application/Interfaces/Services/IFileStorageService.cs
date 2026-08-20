namespace Zausel.Application.Interfaces.Services;

public interface IFileStorageService
{
    // purpose: dosyanın hangi amaçla yüklendiği (ör. "word-images", A-10'da "user-cards", A-13'te
    // "avatars") — depolama yolunu uploads/<purpose>/<yyyy>/<MM>/<guid>.ext şeklinde alt klasörler,
    // amaçlar birbirine karışmaz ve tek klasörde biriken dosya sayısı ayda bir sıfırlanır.
    Task<string> SaveImageAsync(Stream content, string originalFileName, long contentLength, string purpose, CancellationToken cancellationToken);
}
