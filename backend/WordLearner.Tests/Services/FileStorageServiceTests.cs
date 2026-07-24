using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.Services;

namespace WordLearner.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    // LocalFileStorageService dosyanın ilk baytlarına (magic bytes) da bakıyor — "geçerli"
    // senaryolar gerçek bir imzayla başlamalı, yoksa başarılı bir test istemeden başarısız olur.
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private readonly string _tempUploadPath;

    public FileStorageServiceTests()
    {
        _tempUploadPath = Path.Combine(Path.GetTempPath(), $"wordlearner-tests-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempUploadPath))
            Directory.Delete(_tempUploadPath, recursive: true);
    }

    private LocalFileStorageService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["FileStorage:UploadPath"] = _tempUploadPath,
                    ["FileStorage:BaseUrl"] = "https://localhost:7001/uploads",
                }
            )
            .Build();

        return new LocalFileStorageService(configuration);
    }

    // AMAÇ: Gerçek bir imzayla BAŞLAYAN, geri kalanı dolgu (padding) olan bir görsel akışı üretir.
    private static MemoryStream CreateImageStream(byte[] signature, int totalSize = 16)
    {
        var bytes = new byte[Math.Max(totalSize, signature.Length)];
        signature.CopyTo(bytes, 0);
        return new MemoryStream(bytes);
    }

    [Fact]
    public async Task SaveImageAsync_ValidPngFile_ReturnsUrlWithBaseUrlPrefix()
    {
        // ARRANGE
        var service = CreateService();
        using var stream = CreateImageStream(PngSignature);

        // ACT
        var url = await service.SaveImageAsync(stream, "photo.png", stream.Length);

        // ASSERT
        url.Should().StartWith("https://localhost:7001/uploads/");
        url.Should().EndWith(".png");
    }

    [Fact]
    public async Task SaveImageAsync_ValidFile_WritesFileToUploadPath()
    {
        // ARRANGE
        var service = CreateService();
        var content = JpegSignature.Concat(new byte[] { 1, 2, 3, 4, 5 }).ToArray();

        // ACT
        var url = await service.SaveImageAsync(new MemoryStream(content), "photo.jpg", content.Length);

        // ASSERT
        var fileName = url.Split('/').Last();
        var filePath = Path.Combine(_tempUploadPath, fileName);
        File.Exists(filePath).Should().BeTrue();
        (await File.ReadAllBytesAsync(filePath)).Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task SaveImageAsync_CalledTwiceWithSameOriginalName_GeneratesUniqueFileNames()
    {
        // ARRANGE
        var service = CreateService();

        // ACT
        var firstUrl = await service.SaveImageAsync(CreateImageStream(PngSignature), "logo.png", 16);
        var secondUrl = await service.SaveImageAsync(CreateImageStream(PngSignature), "logo.png", 16);

        // ASSERT
        firstUrl.Should().NotBe(secondUrl);
    }

    [Fact]
    public async Task SaveImageAsync_UnsupportedExtension_ThrowsUnsupportedFileTypeException()
    {
        // ARRANGE
        var service = CreateService();

        // ACT
        var act = () => service.SaveImageAsync(new MemoryStream(new byte[10]), "malware.exe", 10);

        // ASSERT
        await act.Should().ThrowAsync<UnsupportedFileTypeException>();
        Directory.Exists(_tempUploadPath).Should().BeFalse();
    }

    [Fact]
    public async Task SaveImageAsync_FileSizeExceedsLimit_ThrowsFileTooLargeException()
    {
        // ARRANGE
        var service = CreateService();
        const long tooLarge = 5 * 1024 * 1024 + 1;

        // ACT
        var act = () => service.SaveImageAsync(new MemoryStream(new byte[10]), "big.png", tooLarge);

        // ASSERT
        await act.Should().ThrowAsync<FileTooLargeException>();
        Directory.Exists(_tempUploadPath).Should().BeFalse();
    }

    [Fact]
    public async Task SaveImageAsync_ExactlyAtSizeLimit_Succeeds()
    {
        // ARRANGE
        var service = CreateService();
        const int exactlyMaxSize = 5 * 1024 * 1024;
        using var stream = CreateImageStream(PngSignature, exactlyMaxSize);

        // ACT
        var act = () => service.SaveImageAsync(stream, "big.png", exactlyMaxSize);

        // ASSERT
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveImageAsync_UppercaseExtension_IsAcceptedCaseInsensitively()
    {
        // ARRANGE
        var service = CreateService();
        using var stream = CreateImageStream(JpegSignature);

        // ACT
        var act = () => service.SaveImageAsync(stream, "PHOTO.JPG", stream.Length);

        // ASSERT
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveImageAsync_ExtensionDoesNotMatchActualContent_ThrowsUnsupportedFileTypeException()
    {
        // ARRANGE
        var service = CreateService();
        using var stream = CreateImageStream(JpegSignature); // gerçek içerik JPEG, ad .png

        // ACT
        var act = () => service.SaveImageAsync(stream, "spoofed.png", stream.Length);

        // ASSERT
        await act.Should().ThrowAsync<UnsupportedFileTypeException>();
        Directory.Exists(_tempUploadPath).Should().BeFalse();
    }
}
