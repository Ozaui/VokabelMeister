using Microsoft.Extensions.Configuration;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Interfaces.Services;

namespace Zausel.Application.Services;

// Yalnızca uzantı kontrolü GÜVENSİZ — bir .exe ".png" adıyla yüklenebilir; magic bytes ile
// dosyanın GERÇEK ilk baytları da bildirdiği uzantıyla eşleşmeli (A_backend.md A-07 notu).
public class LocalFileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private readonly IConfiguration _cfg;

    public LocalFileStorageService(IConfiguration cfg) => _cfg = cfg;

    public async Task<string> SaveImageAsync(Stream content, string originalFileName, long contentLength, string purpose, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
            throw new UnsupportedFileTypeException();
        if (contentLength > MaxFileSizeBytes)
            throw new FileTooLargeException();

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (!MatchesDeclaredType(bytes, extension))
            throw new UnsupportedFileTypeException();

        // uploads/<purpose>/<yyyy>/<MM>/<guid>.ext — amaca göre ayrışır, ayda bir yeni klasöre döner.
        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine(purpose, now.Year.ToString(), now.Month.ToString("00"));
        var uploadDir = Path.Combine(_cfg["FileStorage:UploadPath"]!, relativeDir);
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await File.WriteAllBytesAsync(Path.Combine(uploadDir, fileName), bytes, cancellationToken);

        var relativeUrl = string.Join('/', purpose, now.Year, now.Month.ToString("00"), fileName);
        return $"{_cfg["FileStorage:BaseUrl"]!.TrimEnd('/')}/{relativeUrl}";
    }

    // Spoofing regresyonu: içerik bildirilen uzantıyla eşleşmiyorsa (ör. metin dosyası ".png" adıyla) reddedilir.
    private static bool MatchesDeclaredType(byte[] bytes, string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
        ".png" => bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                  && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A,
        ".webp" => bytes.Length >= 12 && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
                   && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P',
        _ => false
    };
}
