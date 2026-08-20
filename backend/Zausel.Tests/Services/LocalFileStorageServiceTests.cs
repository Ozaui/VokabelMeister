using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Zausel.Application.Common.Exceptions;
using Zausel.Application.Services;

namespace Zausel.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _uploadRoot = Path.Combine(Path.GetTempPath(), "zausel-tests-" + Guid.NewGuid().ToString("N"));

    private LocalFileStorageService CreateService()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:UploadPath"] = _uploadRoot,
            ["FileStorage:BaseUrl"] = "http://localhost:5001/uploads",
        }).Build();
        return new LocalFileStorageService(config);
    }

    // Gerçek dosya format imzaları (magic bytes) — spoofing testlerinde de kullanılıyor.
    private static readonly byte[] ValidPngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    [Fact]
    public async Task SaveImageAsync_UnsupportedExtension_ThrowsUnsupportedFileTypeException()
    {
        var service = CreateService();
        using var stream = new MemoryStream(ValidPngBytes);

        var act = () => service.SaveImageAsync(stream, "resim.gif", ValidPngBytes.Length, "word-images", default);

        await act.Should().ThrowAsync<UnsupportedFileTypeException>();
    }

    [Fact]
    public async Task SaveImageAsync_ContentLengthOverLimit_ThrowsFileTooLargeException()
    {
        var service = CreateService();
        using var stream = new MemoryStream(ValidPngBytes);
        const long overLimit = 5 * 1024 * 1024 + 1;

        var act = () => service.SaveImageAsync(stream, "resim.png", overLimit, "word-images", default);

        await act.Should().ThrowAsync<FileTooLargeException>();
    }

    [Fact]
    public async Task SaveImageAsync_ContentDoesNotMatchDeclaredExtension_ThrowsUnsupportedFileTypeException()
    {
        // Spoofing regresyonu: ".png" adıyla ama gerçek içeriği düz metin olan bir "dosya" — yalnızca
        // uzantı kontrolü GEÇER ama magic bytes doğrulaması reddetmeli.
        var service = CreateService();
        var fakeBytes = "bu bir png degil"u8.ToArray();
        using var stream = new MemoryStream(fakeBytes);

        var act = () => service.SaveImageAsync(stream, "sahte.png", fakeBytes.Length, "word-images", default);

        await act.Should().ThrowAsync<UnsupportedFileTypeException>();
    }

    [Fact]
    public async Task SaveImageAsync_ValidPng_SavesFileAndReturnsPurposeAndDateScopedUrl()
    {
        var service = CreateService();
        using var stream = new MemoryStream(ValidPngBytes);
        var now = DateTime.UtcNow;

        var url = await service.SaveImageAsync(stream, "resim.png", ValidPngBytes.Length, "word-images", default);

        url.Should().StartWith($"http://localhost:5001/uploads/word-images/{now.Year}/{now.Month:00}/");
        url.Should().EndWith(".png");
        var savedFilePath = Path.Combine(_uploadRoot, "word-images", now.Year.ToString(), now.Month.ToString("00"),
            url.Split('/')[^1]);
        File.Exists(savedFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveImageAsync_CalledTwiceWithSameOriginalName_ProducesTwoDistinctFileNames()
    {
        var service = CreateService();
        using var stream1 = new MemoryStream(ValidPngBytes);
        using var stream2 = new MemoryStream(ValidPngBytes);

        var url1 = await service.SaveImageAsync(stream1, "ayni-isim.png", ValidPngBytes.Length, "word-images", default);
        var url2 = await service.SaveImageAsync(stream2, "ayni-isim.png", ValidPngBytes.Length, "word-images", default);

        url1.Should().NotBe(url2);
    }

    public void Dispose()
    {
        if (Directory.Exists(_uploadRoot))
            Directory.Delete(_uploadRoot, recursive: true);
    }
}
