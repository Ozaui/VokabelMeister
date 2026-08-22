using Zausel.Domain.Entities.PersonalContent;

namespace Zausel.Application.Interfaces.Repositories.PersonalContent;

// IUserCardRepository'nin okuma metotlarının döndürdüğü şekil — WordConceptAggregate ile AYNI
// gerekçe: bir kart tek başına anlamsız, örnekleri + hangi kategorilere bağlı olduğu olmadan
// gösterilecek bir şeyi yok.
public record UserCardAggregate(UserCard Card, List<UserCardExample> Examples, List<int> CategoryIds, List<int> UserCategoryIds);
