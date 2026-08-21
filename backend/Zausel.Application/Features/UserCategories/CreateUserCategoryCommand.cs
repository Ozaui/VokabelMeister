using AutoMapper;
using MediatR;
using Zausel.Application.DTOs.UserCategories;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Application.Interfaces.Services;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Features.UserCategories;

// UserId BİLEREK non-nullable (int) — Category'nin Admin-command'larındaki `int? UserId`nin AKSİNE:
// kişisel içerikte sahip her zaman zorunlu (CLAUDE.md §1), [Authorize] olmayan bir çağrı senaryosu yok.
public record CreateUserCategoryCommand(
    string Name, string? Description, string? Color, string? Icon, int UserId, string? ActorRole) : IRequest<UserCategoryResponse>;

public class CreateUserCategoryCommandHandler : IRequestHandler<CreateUserCategoryCommand, UserCategoryResponse>
{
    private readonly IUserCategoryRepository _userCategoryRepository;
    private readonly IMapper _mapper;
    private readonly IActivityLogger _activityLogger;

    public CreateUserCategoryCommandHandler(IUserCategoryRepository userCategoryRepository, IMapper mapper, IActivityLogger activityLogger)
    {
        _userCategoryRepository = userCategoryRepository;
        _mapper = mapper;
        _activityLogger = activityLogger;
    }

    public async Task<UserCategoryResponse> Handle(CreateUserCategoryCommand request, CancellationToken cancellationToken)
    {
        var userCategory = new UserCategory
        {
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon
        };
        await _userCategoryRepository.AddAsync(userCategory, request.UserId, cancellationToken);
        await _userCategoryRepository.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<UserCategoryResponse>(userCategory);

        await _activityLogger.LogAsync(
            request.UserId, request.ActorRole, "CREATE_USER_CATEGORY", entityType: "UserCategory", entityId: userCategory.Id,
            oldValue: null, newValue: response, cancellationToken: cancellationToken);

        return response;
    }
}
