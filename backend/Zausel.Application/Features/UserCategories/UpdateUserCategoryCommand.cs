using AutoMapper;
using MediatR;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCategories;

public record UpdateUserCategoryCommand(
    int UserCategoryId, string Name, string? Description, string? Color, string? Icon, int UserId, string? ActorRole)
    : IRequest<UserCategoryResponse>;

public class UpdateUserCategoryCommandHandler : IRequestHandler<UpdateUserCategoryCommand, UserCategoryResponse>
{
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public UpdateUserCategoryCommandHandler(IUserCategoryRepository userCategoryRepository, IMapper mapper, IActivityLogger activityLogger)
    {
        _userCategoryRepository = userCategoryRepository;
        _mapper = mapper;
        _activityLogger = activityLogger;
    }

    public async Task<UserCategoryResponse> Handle(UpdateUserCategoryCommand request, CancellationToken cancellationToken)
    {
        // Sahiplik filtresi repository'de gömülü — başkasının kategorisi de aynı 404'ü döner (CLAUDE.md §1).
        var userCategory = await _userCategoryRepository.GetByIdForUserAsync(request.UserCategoryId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCategory not found: Id={request.UserCategoryId}");
        var beforeResponse = _mapper.Map<UserCategoryResponse>(userCategory);

        userCategory.Name = request.Name;
        userCategory.Description = request.Description;
        userCategory.Color = request.Color;
        userCategory.Icon = request.Icon;
        await _userCategoryRepository.UpdateAsync(userCategory, request.UserId, cancellationToken);
        await _userCategoryRepository.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<UserCategoryResponse>(userCategory);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "UPDATE_USER_CATEGORY", entityType: "UserCategory", entityId: userCategory.Id,
            oldValue: beforeResponse, newValue: response, cancellationToken: cancellationToken);

        return response;
    }
}
