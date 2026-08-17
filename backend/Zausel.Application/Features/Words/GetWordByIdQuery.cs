using MediatR;
using Zausel.Application.DTOs.Words;
using Zausel.Application.Interfaces.Repositories.Content;
using Zausel.Domain.Exceptions;

namespace Zausel.Application.Features.Words;

public record GetWordByIdQuery(int WordConceptId) : IRequest<WordResponse>;

public class GetWordByIdQueryHandler : IRequestHandler<GetWordByIdQuery, WordResponse>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetWordByIdQueryHandler(IWordConceptRepository wordConceptRepository) => _wordConceptRepository = wordConceptRepository;

    public async Task<WordResponse> Handle(GetWordByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _wordConceptRepository.GetAggregateAsync(request.WordConceptId, cancellationToken)
            ?? throw new EntityNotFoundException($"WordConcept not found: Id={request.WordConceptId}");
        return WordMapping.ToResponse(aggregate);
    }
}
