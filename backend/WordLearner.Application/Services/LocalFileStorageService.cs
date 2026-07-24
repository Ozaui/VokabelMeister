// ─────────────────────────────────────────────────────────────────────────────
// LocalFileStorageService.cs
//
// AMAÇ: IFileStorageService'in yerel disk implementasyonu — görseli
//       FileStorage:UploadPath altına yazar, FileStorage:BaseUrl + üretilen
//       dosya adıyla herkese açık URL döner (REFERENCE/ENV.md §7).
// NEDEN IConfiguration doğrudan enjekte edilir (IOptions<T> DEĞİL): Projede henüz
//       hiçbir yerde IOptions<T> deseni kurulmamış — JwtTokenService de aynı
//       şekilde `_configuration["Jwt:SecretKey"]` okur (bkz. JwtTokenService.cs).
//       Yeni bir soyutlama katmanı eklemek yerine mevcut desen tekrarlanır.
// NEDEN Path.GetFullPath: UploadPath appsettings'te göreli (`wwwroot/uploads`,
//       Development) veya mutlak (`/var/app/uploads`, prod) olabilir.
//       Path.GetFullPath ikisini de doğru çözer (göreliyi mevcut çalışma
//       dizinine göre) — ayrıca IWebHostEnvironment enjekte etmek Application
//       katmanına bir ASP.NET Core Hosting bağımlılığı ekletirdi (proje bunu
//       hiçbir yerde yapmıyor).
// BAĞIMLILIKLAR: IConfiguration, UnsupportedFileTypeException, FileTooLargeException.
// ─────────────────────────────────────────────────────────────────────────────

using Microsoft.Extensions.Configuration;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Interfaces.Services;

namespace WordLearner.Application.Services;

public class LocalFileStorageService : IFileStorageService
{
    // AMAÇ: Kabul edilen görsel uzantıları — küçük harfe çevrilmiş hâliyle karşılaştırılır.
    // NEDEN bu liste: Kelime kartı görseli için yeterli, yaygın 4 format; .gif/.svg/.bmp
    //       gibi ek formatlar gerçek bir ihtiyaç doğmadan spekülatif olarak eklenmez (YAGNI).
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    // AMAÇ: Üst dosya boyutu sınırı — 5 MB.
    // NEDEN: Kelime kartı görseli küçük bir ikon/fotoğraf, 5 MB bu amaç için zaten
    //        cömert bir üst sınır; disk/bant genişliği tükenmesine karşı bir koruma.
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    // AMAÇ: Her formatın gerçek dosya imzası (magic bytes) — WEBP'nin imzası (RIFF....WEBP)
    //       12 bayt gerektirdiği için tampon bu büyüklükte tutulur.
    // NEDEN: Yalnızca UZANTI kontrolü (kod denetiminde bulundu) bir `.exe`nin adını
    //        `foto.png` yapıp yüklenmesini ENGELLEMEZ — uzantı yalnızca istemcinin
    //        BEYANIdır, dosyanın gerçek içeriği değildir. İlk baytlara bakmak, harici bir
    //        kütüphane (görsel decode) gerektirmeden ucuz bir içerik doğrulaması sağlar.
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
        // ADIM 1: Uzantı doğrulaması — izin verilen listede değilse diske hiç dokunmadan reddet.
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new UnsupportedFileTypeException();

        // ADIM 2: Boyut doğrulaması — üst sınırı aşıyorsa diske hiç dokunmadan reddet.
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new FileTooLargeException();

        // ADIM 3: İçerik (magic bytes) doğrulaması — uzantı doğru olsa bile dosyanın İLK
        //         baytları o formatın gerçek imzasıyla eşleşmiyorsa diske hiç dokunmadan reddet.
        var header = new byte[SignatureBufferSize];
        var headerBytesRead = await ReadHeaderAsync(fileStream, header, ct);
        if (!HasValidImageSignature(header.AsSpan(0, headerBytesRead), extension))
            throw new UnsupportedFileTypeException();

        // ADIM 4: Hedef klasör yoksa oluştur (CreateDirectory zaten varsa no-op'tur).
        Directory.CreateDirectory(_uploadPath);

        // ADIM 5: Benzersiz ad üret — orijinal dosya adı KORUNMAZ (çakışma + path
        //         traversal riski), yalnızca doğrulanmış uzantı korunur.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using (var output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            // NEDEN header'ı ayrıca yazıyoruz: ADIM 3'te `fileStream`'den zaten OKUNMUŞ
            //       baytlar `CopyToAsync`'e görünmez — stream seekable olmayabileceği için
            //       geri sarılamaz (Rewind/Seek yok), bu yüzden okunan baytlar elle yazılıp
            //       ardından akışın KALANI kopyalanır.
            await output.WriteAsync(header.AsMemory(0, headerBytesRead), ct);
            await fileStream.CopyToAsync(output, ct);
        }

        return $"{_baseUrl}/{fileName}";
    }

    // AMAÇ: Akıştan tam olarak `buffer.Length` bayt (veya akış daha kısaysa mevcut tümü) okur.
    // NEDEN: `Stream.ReadAsync` TEK çağrıda istenen tüm baytları DÖNDÜRMEYİ garanti ETMEZ
    //        (özellikle ağ/form akışlarında kısmi okuma normaldir) — döngü olmadan yalnızca
    //        ilk parçayı okuyup imza kontrolü YANLIŞLIKLA başarısız olabilirdi.
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

    // AMAÇ: Okunan ilk baytların, `extension`in gerçek formatının imzasıyla eşleşip
    //       eşleşmediğini kontrol eder.
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
