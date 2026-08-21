namespace Zausel.Application.DTOs.UserCategories;

// CardCount (API_ENDPOINTS.md §8) BİLEREK yok — kaynağı olan UserCardUserCategories ara tablosu
// henüz yok (UserCard entity'si A-10'da gelir, dikey dilim sırası bu yüzden UserCategory'yi önce ister).
// A-10'da GetUserCategoriesQueryHandler'a eklenecek.
public record UserCategoryResponse(int Id, string Name, string? Description, string? Color, string? Icon);
