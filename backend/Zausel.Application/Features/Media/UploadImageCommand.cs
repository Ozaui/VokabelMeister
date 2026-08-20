using MediatR;
using Zausel.Application.DTOs.Media;
using Zausel.Application.Interfaces.Services;

namespace Zausel.Application.Features.Media;

public record UploadImageCommand(Stream Content, string FileName, long ContentLength, int? UserId, string? ActorRole)
    : IRequest<MediaUploadResponse>;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, MediaUploadResponse>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IActivityLogger _activityLogger;

    public UploadImageCommandHandler(IFileStorageService fileStorageService, IActivityLogger activityLogger)
    {
        _fileStorageService = fileStorageService;
        _activityLogger = activityLogger;
    }

    public async Task<MediaUploadResponse> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        // Bu uç yalnızca kelime kavramı görseli içindir (API_ENDPOINTS.md §5.1) — A-10/A-13'teki
        // kart görseli/avatar yüklemeleri kendi Command'larından IFileStorageService'i FARKLI bir
        // purpose ile ("user-cards"/"avatars") çağırır, bu Command'ı yeniden kullanmaz.
        var url = await _fileStorageService.SaveImageAsync(request.Content, request.FileName, request.ContentLength, "word-images", cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPLOAD_MEDIA", entityType: "Word", entityId: null,
            oldValue: null, newValue: new { url }, cancellationToken: cancellationToken);

        return new MediaUploadResponse(url);
    }
}
