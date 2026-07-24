using MediatR;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Application.DTOs.Words;
using WordLearner.Application.Interfaces.Repositories;
using WordLearner.Domain.Entities.Words;

namespace WordLearner.Application.Features.Words;

public record GetWordByIdQuery(int Id) : IRequest<WordConceptDetailDto>;

public class GetWordByIdQueryHandler : IRequestHandler<GetWordByIdQuery, WordConceptDetailDto>
{
    private readonly IWordConceptRepository _wordConceptRepository;

    public GetWordByIdQueryHandler(IWordConceptRepository wordConceptRepository) =>
        _wordConceptRepository = wordConceptRepository;

    public async Task<WordConceptDetailDto> Handle(GetWordByIdQuery request, CancellationToken ct)
    {
        var concept =
            await _wordConceptRepository.GetWithTranslationsAsync(request.Id, ct)
            ?? throw new EntityNotFoundException(typeof(WordConcept), request.Id);

        return WordConceptDtoBuilder.BuildDetail(concept);
    }
}
