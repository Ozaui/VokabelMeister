namespace Zausel.Application.DTOs.UserCategories;

public record UserCategoryCreateRequest(string Name, string? Description, string? Color, string? Icon);
