namespace Zausel.Application.DTOs.UserCategories;

public record UserCategoryResponse(int Id, string Name, string? Description, string? Color, string? Icon, int CardCount);
