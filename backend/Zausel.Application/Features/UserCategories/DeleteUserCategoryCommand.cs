using AutoMapper;
using MediatR;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCategories;

public record DeleteUserCategoryCommand(int UserCategoryId, int UserId, string? ActorRole) : IRequest<Unit>;

public class DeleteUserCategoryCommandHandler : IRequestHandler<DeleteUserCategoryCommand, Unit>
{
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public DeleteUserCategoryCommandHandler(IUserCategoryRepository userCategoryRepository, IMapper mapper, IActivityLogger activityLogger)
    {
        _userCategoryRepository = userCategoryRepository;
        _mapper = mapper;
        _activityLogger = activityLogger;
    }

    public async Task<Unit> Handle(DeleteUserCategoryCommand request, CancellationToken cancellationToken)
    {
        var userCategory = await _userCategoryRepository.GetByIdForUserAsync(request.UserCategoryId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCategory not found: Id={request.UserCategoryId}");
        var beforeResponse = _mapper.Map<UserCategoryResponse>(userCategory);

        await _userCategoryRepository.SoftDeleteAsync(userCategory, request.UserId, cancellationToken);
        await _userCategoryRepository.SaveChangesAsync(cancellationToken);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "DELETE_USER_CATEGORY", entityType: "UserCategory", entityId: request.UserCategoryId,
            oldValue: beforeResponse, newValue: null, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
