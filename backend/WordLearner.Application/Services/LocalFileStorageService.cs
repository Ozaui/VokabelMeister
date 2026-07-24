using Microsoft.Extensions.Configuration;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    // WEBP imzası (RIFF....WEBP) 12 bayt gerektirir.
    private const int SignatureBufferSize = 12;
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _uploadPath = Path.GetFullPath(configuration["FileStorage:UploadPath"]!);
        _baseUrl = configuration["FileStorage:BaseUrl"]!.TrimEnd('/');
    }

    public async Task<string> SaveImageAsync(
        Stream fileStream,
        string originalFileName,
        long fileSizeBytes,
        CancellationToken ct = default
    )
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new UnsupportedFileTypeException();

        if (fileSizeBytes > MaxFileSizeBytes)
            throw new FileTooLargeException();

        // Yalnızca uzantı kontrolü bir .exe'nin adını foto.png yapıp yüklenmesini engellemez —
        // uzantı istemcinin beyanıdır, dosyanın gerçek içeriği değil. İlk baytlara (magic bytes) bakılır.
        var header = new byte[SignatureBufferSize];
        var headerBytesRead = await ReadHeaderAsync(fileStream, header, ct);
        if (!HasValidImageSignature(header.AsSpan(0, headerBytesRead), extension))
            throw new UnsupportedFileTypeException();

        Directory.CreateDirectory(_uploadPath);

        // Orijinal dosya adı KORUNMAZ (çakışma + path traversal riski), yalnızca doğrulanmış uzantı korunur.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using (var output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            // header'daki baytlar imza kontrolü için zaten okundu; stream seekable olmayabileceği
            // için geri sarılamaz — bu yüzden elle yazılıp akışın kalanı ayrıca kopyalanır.
            await output.WriteAsync(header.AsMemory(0, headerBytesRead), ct);
            await fileStream.CopyToAsync(output, ct);
        }

        return $"{_baseUrl}/{fileName}";
    }

    // Stream.ReadAsync tek çağrıda istenen tüm baytları döndürmeyi garanti etmez
    // (kısmi okuma ağ/form akışlarında normaldir) — döngü olmadan imza kontrolü yanlışlıkla başarısız olabilirdi.
    private static async Task<int> ReadHeaderAsync(Stream stream, byte[] buffer, CancellationToken ct)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead), ct);
            if (read == 0)
                break;
            totalRead += read;
        }
        return totalRead;
    }

    private static bool HasValidImageSignature(ReadOnlySpan<byte> header, string extension) =>
        extension switch
        {
            ".png" => header.StartsWith(PngSignature),
            ".jpg" or ".jpeg" => header.StartsWith(JpegSignature),
            ".webp" => header.Length >= SignatureBufferSize
                && header[..4].SequenceEqual("RIFF"u8)
                && header[8..12].SequenceEqual("WEBP"u8),
            _ => false,
        };
}
