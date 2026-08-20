using Zausel.Domain.Entities.Content;

namespace Zausel.Application.Interfaces.Repositories.Content;

// ICategoryRepository'nin okuma metotlarının döndürdüğü şekil — Category tek başına anlamsız
// (adı olmadan gösterilecek bir şeyi yok), WordConceptAggregate'in Translations deseniyle AYNI.
public record CategoryAggregate(Category Category, List<CategoryTranslationAggregate> Translations);

// Her çeviri kendi CategoryTranslation'ı + hangi dile ait olduğu.
public record CategoryTranslationAggregate(CategoryTranslation Translation, Language Language);
