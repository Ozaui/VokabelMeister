# EntityNotFoundException

**Özet:** [[WordLearner_Application]] içinde tanımlı, `Exception`'dan türeyen özel exception tipi — istenilen entity DB'de bulunamadığında fırlatılır. Genel `KeyNotFoundException`/`Exception` yerine özel tip kullanılması, ileride eklenecek global exception middleware'inin bu durumu yakalayıp otomatik 404 döndürmesini sağlayacak.
**Kütüphaneler:** Saf C# — dış bağımlılık yok
**Bağlantılar:** [[WordLearner_Application]] · [[Repository]] · [[IRepository]] · [[Kodlama_Standartlari]]

## Konum
`backend/WordLearner.Application/Common/Exceptions/EntityNotFoundException.cs`

## Kullanım
[[Repository]].`SoftDeleteAsync` içinde fırlatılıyor:
```csharp
var entity = await GetByIdAsync(id, ct)
    ?? throw new EntityNotFoundException($"{typeof(T).Name} bulunamadı: Id={id}");
```

## Planlanan Genişleme
Global exception middleware (A-02'nin kalan adımı, henüz yok) bu tipi yakalayıp `404` + standart
hata formatına çevirecek — bkz. [[API_Sozlesmesi]]. `EntityNotFoundException` yanına ileride
`DuplicateEntityException` (409 için, `?force=true` akışlarında kullanılıyor — bkz. `API_ENDPOINTS.md`)
gibi kardeş exception tipleri eklenmesi bekleniyor.
