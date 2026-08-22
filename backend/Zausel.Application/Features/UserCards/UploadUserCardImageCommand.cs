using MediatR;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCards;

public record UploadUserCardImageCommand(
    int UserCardId, Stream Content, string FileName, long ContentLength, int UserId, string? ActorRole)
    : IRequest<UserCardResponse>;

public class UploadUserCardImageCommandHandler : IRequestHandler<UploadUserCardImageCommand, UserCardResponse>
{
    private readonly IUserCardRepository _userCardRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IActivityLogger _activityLogger;

    public UploadUserCardImageCommandHandler(
        IUserCardRepository userCardRepository, IFileStorageService fileStorageService, IActivityLogger activityLogger)
    {
        _userCardRepository = userCardRepository;
        _fileStorageService = fileStorageService;
        _activityLogger = activityLogger;
    }

    public async Task<UserCardResponse> Handle(UploadUserCardImageCommand request, CancellationToken cancellationToken)
    {
        // Sahiplik filtresi burada gömülü — başkasının kartına görsel yüklenemez.
        var aggregate = await _userCardRepository.GetByIdForUserAsync(request.UserCardId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found: Id={request.UserCardId}");
        var oldImageUrl = aggregate.Card.ImageUrl;

        var newImageUrl = await _fileStorageService.SaveImageAsync(
            request.Content, request.FileName, request.ContentLength, "user-cards", cancellationToken);

        if (oldImageUrl is not null)
            await _fileStorageService.DeleteImageAsync(oldImageUrl, cancellationToken);

        aggregate.Card.ImageUrl = newImageUrl;
        await _userCardRepository.UpdateAsync(aggregate.Card, request.UserId, cancellationToken);
        await _userCardRepository.SaveChangesAsync(cancellationToken);

        var response = UserCardMapping.ToResponse(aggregate);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPDATE_USER_CARD", entityType: "UserCard", entityId: aggregate.Card.Id,
            oldValue: new { ImageUrl = oldImageUrl }, newValue: new { ImageUrl = newImageUrl }, cancellationToken: cancellationToken);

        return response;
    }
}
