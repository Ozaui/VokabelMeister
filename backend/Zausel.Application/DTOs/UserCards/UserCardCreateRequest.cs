namespace Zausel.Application.DTOs.UserCards;

public record UserCardCreateRequest(
    string FrontText,
    string BackText,
    string? Notes,
    List<int>? CategoryIds,
    List<int>? UserCategoryIds,
    List<UserCardExampleRequest>? Examples);
