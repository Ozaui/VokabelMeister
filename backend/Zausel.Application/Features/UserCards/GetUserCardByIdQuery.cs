using MediatR;
using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.UserCards;

public record GetUserCardByIdQuery(int UserCardId, int UserId) : IRequest<UserCardResponse>;

public class GetUserCardByIdQueryHandler : IRequestHandler<GetUserCardByIdQuery, UserCardResponse>
{
    private readonly IUserCardRepository _userCardRepository;

    public GetUserCardByIdQueryHandler(IUserCardRepository userCardRepository) => _userCardRepository = userCardRepository;

    public async Task<UserCardResponse> Handle(GetUserCardByIdQuery request, CancellationToken cancellationToken)
    {
        // Sahiplik filtresi burada gömülü — başkasının kartı da AYNI 404'ü döner.
        var aggregate = await _userCardRepository.GetByIdForUserAsync(request.UserCardId, request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException($"UserCard not found: Id={request.UserCardId}");

        return UserCardMapping.ToResponse(aggregate);
    }
}
