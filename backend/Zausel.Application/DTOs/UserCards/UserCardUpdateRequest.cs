namespace Zausel.Application.DTOs.UserCards;

public record UserCardUpdateRequest(
    string FrontText,
    string BackText,
    string? Notes,
    bool IsActive,
    List<int>? CategoryIds,
    List<int>? UserCategoryIds,
    List<UserCardExampleRequest>? Examples);
