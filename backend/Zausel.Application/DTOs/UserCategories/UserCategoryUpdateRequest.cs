namespace Zausel.Application.DTOs.UserCategories;

public record UserCategoryUpdateRequest(string Name, string? Description, string? Color, string? Icon);
