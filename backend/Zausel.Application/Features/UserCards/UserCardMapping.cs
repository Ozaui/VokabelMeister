using Zausel.Application.DTOs.UserCards;
using Zausel.Application.Interfaces.Repositories.PersonalContent;
using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Features.UserCards;

// AutoMapper Profile YAZILMADI — UserCardAggregate iki farklı entity türünü (UserCard/UserCardExample)
// ve iki ayrı ara tablodan türetilmiş id listesini TEK bir iç içe DTO'ya birleştiriyor, WordMapping ile
// AYNI gerekçe: aggregate/çoklu-kaynaklı bir dönüşüm AutoMapper'ın düz property eşlemesinden daha
// OKUNAKLI değil. Create/Update/GetById/GetUserCards Handler'larının HEPSİ bu TEK metodu paylaşır.
public static class UserCardMapping
{
    public static UserCardResponse ToResponse(UserCardAggregate aggregate, int? suggestedSystemWordId = null)
    {
        var card = aggregate.Card;
        return new UserCardResponse(
            card.Id,
            card.FrontText,
            card.BackText,
            card.Notes,
            card.ImageUrl,
            card.IsActive,
            aggregate.CategoryIds,
            aggregate.UserCategoryIds,
            aggregate.Examples.Select(ToExampleResponse).ToList(),
            suggestedSystemWordId);
    }

    private static UserCardExampleResponse ToExampleResponse(UserCardExample example) =>
        new(example.Id, example.SentenceFront, example.SentenceBack, example.DisplayOrder);
}
