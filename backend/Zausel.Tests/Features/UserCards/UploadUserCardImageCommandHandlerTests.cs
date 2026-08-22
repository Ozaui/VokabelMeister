using FluentAssertions;
using Moq;
using Zausel.Application.Features.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Features.UserCards;

public class UploadUserCardImageCommandHandlerTests
{
    private readonly Mock<IUserCardRepository> _userCardRepository = new();
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly Mock<IActivityLogger> _activityLogger = new();

    private UploadUserCardImageCommandHandler CreateHandler() =>
        new(_userCardRepository.Object, _fileStorageService.Object, _activityLogger.Object);

    private static UploadUserCardImageCommand Command(int userCardId, int userId) =>
        new(userCardId, new MemoryStream(), "photo.png", 1024, userId, "User");

    [Fact]
    public async Task Handle_NotFoundOrNotOwned_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(99, 1, It.IsAny<CancellationToken>())).ReturnsAsync((UserCardAggregate?)null);
        var handler = CreateHandler();

        // ACT
        var act = () => handler.Handle(Command(99, 1), default);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _fileStorageService.Verify(f => f.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoExistingImage_SavesNewImageWithoutDeletingAnything()
    {
        // ARRANGE — kartın ÖNCEDEN görseli yok, silinecek eski dosya YOK.
        var card = new UserCard { Id = 1, UserId = 5, FrontText = "laufen", BackText = "koşmak", ImageUrl = null };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCardAggregate(card, [], [], []));
        _fileStorageService
            .Setup(f => f.SaveImageAsync(It.IsAny<Stream>(), "photo.png", 1024, "user-cards", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost/uploads/user-cards/2026/08/new.png");
        var handler = CreateHandler();

        // ACT
        var result = await handler.Handle(Command(1, 5), default);

        // ASSERT
        result.ImageUrl.Should().Be("http://localhost/uploads/user-cards/2026/08/new.png");
        _fileStorageService.Verify(f => f.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _userCardRepository.Verify(r => r.UpdateAsync(It.Is<UserCard>(c => c.ImageUrl == "http://localhost/uploads/user-cards/2026/08/new.png"), 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingImage_DeletesOldImageAfterSavingNew()
    {
        // ARRANGE — kartın ZATEN bir görseli var, YENİSİ kaydedildikten sonra ESKİSİ silinmeli.
        var card = new UserCard { Id = 1, UserId = 5, FrontText = "laufen", BackText = "koşmak", ImageUrl = "http://localhost/uploads/user-cards/2026/07/old.png" };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCardAggregate(card, [], [], []));
        _fileStorageService
            .Setup(f => f.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(), "user-cards", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost/uploads/user-cards/2026/08/new.png");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(Command(1, 5), default);

        // ASSERT
        _fileStorageService.Verify(f => f.DeleteImageAsync("http://localhost/uploads/user-cards/2026/07/old.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Success_LogsUpdateUserCardActivity()
    {
        // ARRANGE
        var card = new UserCard { Id = 1, UserId = 5, FrontText = "laufen", BackText = "koşmak", ImageUrl = null };
        _userCardRepository.Setup(r => r.GetByIdForUserAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(new UserCardAggregate(card, [], [], []));
        _fileStorageService
            .Setup(f => f.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://localhost/uploads/user-cards/2026/08/new.png");
        var handler = CreateHandler();

        // ACT
        await handler.Handle(Command(1, 5), default);

        // ASSERT — UPDATE_USER_CARD ile AYNI aksiyon kodu (görsel değişimi de bir kart güncellemesidir).
        _activityLogger.Verify(l => l.LogAsync(
            5, "User", "UPDATE_USER_CARD", "UserCard", 1, It.IsAny<object>(), It.IsAny<object>(), null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
