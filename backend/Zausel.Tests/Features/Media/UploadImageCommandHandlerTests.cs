using FluentAssertions;
using Moq;
using Zausel.Application.Features.Media;
using Zausel.Application.Interfaces.Services;

namespace Zausel.Tests.Features.Media;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UploadImageCommandHandler CreateHandler() => new(_fileStorageService.Object, _activityLogger.Object);

    [Fact]
    public async Task Handle_ValidFile_SavesViaFileStorageServiceWithWordImagesPurpose()
    {
        // ARRANGE
        using var stream = new MemoryStream();
        _fileStorageService
            .Setup(s => s.SaveImageAsync(stream, "kelime.png", 1234, "word-images", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost:5001/uploads/word-images/2026/08/abc.png");
        var command = new UploadImageCommand(stream, "kelime.png", 1234, UserId: 1, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(command, default);

        // ASSERT
        result.Url.Should().Be("http://localhost:5001/uploads/word-images/2026/08/abc.png");
        _fileStorageService.Verify(s => s.SaveImageAsync(stream, "kelime.png", 1234, "word-images", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidFile_LogsUploadMediaActivity()
    {
        // ARRANGE
        using var stream = new MemoryStream();
        _fileStorageService
            .Setup(s => s.SaveImageAsync(stream, "kelime.png", 1234, "word-images", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost:5001/uploads/word-images/2026/08/abc.png");
        var command = new UploadImageCommand(stream, "kelime.png", 1234, UserId: 7, ActorRole: "Admin");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(command, default);

        // ASSERT
        _activityLogger.Verify(l => l.LogAsync(
            7, "Admin", "UPLOAD_MEDIA", "Word", null, null, It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
