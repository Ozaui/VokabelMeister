# AuthProfile

**Özet:** Auth feature'ının tek [[AutoMapper]] `Profile` sınıfı — `User → RegisterResponse` ve `User → AuthUserDto` dönüşümlerini tanımlar. [[ApplicationServiceExtensions]]'taki `AddAutoMapper(applicationAssembly)` bu sınıfı otomatik tarar; ek DI kaydı gerekmez. Alan adları (`Id`, `Email`, `FirstName`, `CurrentLevel`) entity ile birebir eşleştiği için `ForMember` konfigürasyonuna ihtiyaç yok.
**Kütüphaneler:** AutoMapper 13.0.1.
**Bağlantılar:** [[ApplicationServiceExtensions]] · [[RegisterCommand]] · [[RefreshCommand]] · [[LoginCompletionService]] · [[Auth_Domain]]

## Konum
`backend/WordLearner.Application/Features/Auth/AuthProfile.cs`

## İçerik
```csharp
public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, RegisterResponse>();
        CreateMap<User, AuthUserDto>();
    }
}
```

## Kullanıldığı Yerler
- `RegisterCommandHandler.Handle` → `_mapper.Map<RegisterResponse>(user)` (eskiden `new RegisterResponse(user.Id, user.Email, user.FirstName, user.CurrentLevel)`).
- `LoginCompletionService.CompleteLoginAsync` → `_mapper.Map<AuthUserDto>(user)` (OTP/Google/Apple girişlerinin ortak son adımı).
- `RefreshCommandHandler.Handle` → `_mapper.Map<AuthUserDto>(user)` (aynı dönüşüm, `LoginCompletionService`'ten bağımsız ikinci bir çağrı noktası — token refresh akışı `CompleteLoginAsync`'i çağırmıyor).

## Neden Bu İki Dönüşüm ve Diğerleri Değil
Auth feature'ındaki 13 Command Handler'ın çoğu `MessageResponse("sabit metin")` döner — entity'den
türemeyen sabit bir string olduğu için AutoMapper'a konu olmaz. `AuthTokenResponse`'un tamamı da
(access/refresh token, expiresIn, accountWasRecovered) tek bir kaynak nesneden gelmez — bunlar
elle inşa edilmeye devam eder, yalnızca içindeki `AuthUserDto` alt-nesnesi mapping'e uygundur.

## Kararın Kökeni
A-02'de "ileride Entity↔DTO dönüşümü için Profile sınıfları eklenecek" varsayımıyla kurulan
AutoMapper paketi, A-03'ün ilk halinde (13 Command Handler MediatR'a taşınırken) hiç kullanılmadan
kaldı — repo genelinde sıfır `Profile`, sıfır `IMapper` kullanımı vardı. Kullanıcı bunu YAGNI ihlali
olarak fark edip iki seçenek sundu: paketi kaldır ya da gerçekten kullan. Seçilen: gerçekten kullan —
bu profil, MediatR'ın daha önce aynı şekilde retrofit edilmesiyle aynı desenin (bkz. wiki Index.md,
"On yedinci INGEST") AutoMapper için tekrarı.
