# EntityNotFoundException

**Özet:** [[WordLearner_Application]] içinde tanımlı, `Exception`'dan türeyen özel exception tipi — istenilen entity DB'de bulunamadığında fırlatılır. Genel `KeyNotFoundException`/`Exception` yerine özel tip kullanılması, ileride eklenecek global exception middleware'inin bu durumu yakalayıp otomatik 404 döndürmesini sağlayacak. `Type`+key alan bir overload'u da var — çağıranın mesajı elle interpolate etmesini önler.
**Kütüphaneler:** Saf C# — dış bağımlılık yok
**Bağlantılar:** [[WordLearner_Application]] · [[Repository]] · [[IRepository]] · [[Kodlama_Standartlari]]

## Konum
`backend/WordLearner.Application/Common/Exceptions/EntityNotFoundException.cs`

## Constructor'lar
- `EntityNotFoundException(string message)` — serbest metin (Id dışı anahtarla arama gibi durumlar için).
- `EntityNotFoundException(Type entityType, object key)` — `"{entityType.Name} bulunamadı: Id={key}"`
  formatını otomatik üretir. **Tercih edilen yol** — çağıranın string interpolasyonunu elle yazmasını
  önler, format tek yerden yönetilir.

## Kullanım
[[Repository]].`SoftDeleteAsync` içinde fırlatılıyor:
```csharp
var entity = await GetByIdAsync(id, ct)
    ?? throw new EntityNotFoundException(typeof(T), id);
```

## Test Kapsamı
`EntityNotFoundExceptionTests` (`WordLearner.Tests/Common/Exceptions/`) — `Type`+key overload'ının
standart mesaj formatını ürettiğini doğrulayan 1 test, bkz. [[WordLearner_Tests]].

## Planlanan Genişleme
Global exception middleware (A-02'nin kalan adımı, henüz yok) bu tipi yakalayıp `404` + standart
hata formatına çevirecek — bkz. [[API_Sozlesmesi]]. `EntityNotFoundException` yanına ileride
`DuplicateEntityException` (409 için, `?force=true` akışlarında kullanılıyor — bkz. `docs/REFERENCE/API_ENDPOINTS.md`)
gibi kardeş exception tipleri eklenmesi bekleniyor.
