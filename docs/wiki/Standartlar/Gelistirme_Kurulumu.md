# Geliştirme Ortamı Kurulumu

**Özet:** `docs/REFERENCE/DEVELOPMENT_SETUP.md`'nin wiki karşılığı — gerekli araçlar, proje iskeletini oluşturan `dotnet` komutları (A-01'de zaten çalıştırıldı), migration/çalıştırma komutları ve IIS yayınlama adımları. [[Gelistirme_Yol_Haritasi]]'ndeki her fazın "nasıl kurulur" kısmı burada.
**Kütüphaneler:** .NET 9 SDK, Node.js 18.17+ LTS, SQL Server 2019+, Git 2.40+
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[Teknik_Ozellikler]] · [[Ortam_Degiskenleri]] · [[WordLearner_API]]

## Gerekli Araçlar
`Node.js 18.17+ LTS` · `.NET 9 SDK` · `SQL Server 2019+ (SSMS)` · `VS 2022 / VS Code` · `Git 2.40+`

## Backend Proje İskeleti (A-01 — zaten yapıldı, referans için)
```bash
dotnet new sln -n WordLearner
dotnet new webapi   -n WordLearner.API            -o backend/WordLearner.API
dotnet new classlib -n WordLearner.Application    -o backend/WordLearner.Application
dotnet new classlib -n WordLearner.Infrastructure -o backend/WordLearner.Infrastructure
dotnet new classlib -n WordLearner.Domain         -o backend/WordLearner.Domain
dotnet new xunit    -n WordLearner.Tests          -o backend/WordLearner.Tests
dotnet sln add backend/**/*.csproj

# Referanslar: Domain ← Infrastructure ← Application ← API (bkz. Backend_Katmanli_Mimari)
dotnet add backend/WordLearner.API reference backend/WordLearner.Application backend/WordLearner.Infrastructure
dotnet add backend/WordLearner.Application reference backend/WordLearner.Domain
dotnet add backend/WordLearner.Infrastructure reference backend/WordLearner.Domain
dotnet add backend/WordLearner.Tests reference backend/WordLearner.Application backend/WordLearner.Infrastructure
```

## Veritabanı + Migration (henüz hiç migration oluşturulmadı)
```bash
cd backend
dotnet ef migrations add InitialCreate --project WordLearner.Infrastructure --startup-project WordLearner.API
dotnet ef database update --project WordLearner.Infrastructure --startup-project WordLearner.API
```
> **Dikey dilim notu:** Her yeni API'da o API'ın entity'si için ayrı migration eklenir
> (`dotnet ef migrations add AddXxx`) — tüm tablolar tek seferde değil, API API büyür.

## Backend Çalıştırma
```bash
dotnet run --project WordLearner.API
# Swagger: http://localhost:5001/swagger
```

## Frontend Kurulumları (Faz B/D/E — henüz `admin/`, `web/`, `mobile/` klasörleri yok)
Komutlar → [[Teknik_Ozellikler]] §2. Admin: 5173, Web: 5174, Mobil/Expo: 8081 (CORS ile eşleşir,
bkz. [[Ortam_Degiskenleri]]).

## Google / Apple Giriş Doğrulama (backend, planlanan)
```csharp
// Google: token doğrula → kendi JWT'mizi üret (Google token saklanmaz)
var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken);
// Apple: identity token Apple JWKS ile doğrulanır; email/isim yalnızca ilk girişte gelir
```

## IIS Yayınlama (Faz F)
```bash
dotnet publish backend/WordLearner.API -c Release -r win-x64 --self-contained false -o ./publish/api
```
App Pool → **No Managed Code** (.NET Kestrel; IIS reverse proxy, URL Rewrite Module gerekli).

## Git Akışı
`main` → production · `develop` → aktif geliştirme · `feature/` · `bugfix/`.
Commit formatı: `feat(auth): Google girişi eklendi` · `fix(word): çoğul kaydetme hatası` · `docs: ...`.
