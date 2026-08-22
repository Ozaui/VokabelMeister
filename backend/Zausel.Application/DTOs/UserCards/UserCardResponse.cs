namespace Zausel.Application.DTOs.UserCards;

// SuggestedSystemWordId yalnızca POST /user-cards'ın 201 yanıtında dolu olabilir (sistem
// eşleşmesi bulunduysa) — liste/detay/güncelleme yanıtlarında her zaman null, ayrı bir
// "CreateUserCardResponse" zarfı AÇILMADI (WordResponse ile AYNI tek-şekil ilkesi).
public record UserCardResponse(
    int Id,
    string FrontText,
    string BackText,
    string? Notes,
    string? ImageUrl,
    bool IsActive,
    List<int> CategoryIds,
    List<int> UserCategoryIds,
    List<UserCardExampleResponse> Examples,
    int? SuggestedSystemWordId);
