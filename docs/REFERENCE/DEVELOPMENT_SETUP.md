# GELİŞTİRME ORTAMI KURULUMU

> npm paket listeleri → `TECHNICAL_SPECIFICATIONS.md §2`. ENV değerleri → `ENV.md`.

## 1. Araçlar
```
Node.js 18.17+ LTS · .NET 9 SDK · SQL Server 2019+ (SSMS) · VS 2022 / VS Code · Git 2.40+
```

## 2. Projeyi Al
```bash
git clone https://github.com/kullanici/WordLearner.git && cd WordLearner && dotnet restore
```

## 3. Backend İskeleti (A-01)
```bash
dotnet new sln -n WordLearner
dotnet new webapi   -n WordLearner.API            -o backend/WordLearner.API
dotnet new classlib -n WordLearner.Application    -o backend/WordLearner.Application
dotnet new classlib -n WordLearner.Infrastructure -o backend/WordLearner.Infrastructure
dotnet new classlib -n WordLearner.Domain         -o backend/WordLearner.Domain
dotnet new xunit    -n WordLearner.Tests          -o backend/WordLearner.Tests
dotnet sln add backend/**/*.csproj

# Referanslar: Domain ← Application ← Infrastructure ← API
dotnet add backend/WordLearner.API reference backend/WordLearner.Application backend/WordLearner.Infrastructure
dotnet add backend/WordLearner.Application reference backend/WordLearner.Domain
dotnet add backend/WordLearner.Infrastructure reference backend/WordLearner.Domain
dotnet add backend/WordLearner.Tests reference backend/WordLearner.Application backend/WordLearner.Infrastructure
```
Tests klasörü: `{Services, Features, Helpers, Repositories}/` (→ `CODING_STANDARDS.md §7`). NuGet → `TECHNICAL_SPECIFICATIONS.md §1`.

## 4. Migration (dikey dilim — her API kendi migration'ını ekler)
```bash
cd backend
dotnet ef migrations add InitialCreate --project WordLearner.Infrastructure --startup-project WordLearner.API
dotnet ef database update --project WordLearner.Infrastructure --startup-project WordLearner.API
# Yeni API: dotnet ef migrations add AddXxx (tüm tablolar tek seferde değil, API API büyür)
```

## 5. Çalıştırma
```bash
dotnet run --project WordLearner.API      # Swagger: http://localhost:5001/swagger
cd admin && npm run dev                    # http://localhost:5173  (yalnızca e-posta+şifre, Admin rolü)
cd web && npm run dev                      # http://localhost:5174  (token localStorage; Apple yok)
cd mobile && npx expo start                # token Expo Secure Store
```
Frontend `.env.development`: `VITE_API_URL` / `EXPO_PUBLIC_API_URL = http://localhost:5001/api/v1` (+ web `VITE_GOOGLE_CLIENT_ID`).

## 6. Google / Apple Giriş (backend doğrulama)
```csharp
// Google: token doğrula → kendi JWT'mizi üret (Google token saklanmaz)
var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken);
var user = await _userRepo.GetByEmailAsync(payload.Email) ?? await CreateFromGoogleAsync(payload);
// Apple: identity token (JWT) Apple JWKS ile doğrulanır; email/isim yalnızca ilk girişte gelir
var claims = await ValidateAppleTokenAsync(identityToken);
```

## 7. IIS Yayınlama
```bash
dotnet publish backend/WordLearner.API -c Release -r win-x64 --self-contained false -o ./publish/api
```
App Pool → **No Managed Code** (Kestrel + IIS reverse proxy, URL Rewrite gerekli). Frontend: `npm run build` → `dist/`; React Router için `web.config`'e SPA rewrite.

## 8. Git Akışı
```
main → production · develop → aktif · feature/ · bugfix/
```
Commit: `feat(auth): Google girişi eklendi` · `fix(word): çoğul kaydetme hatası` · `docs: ...`.

## 9. Klasör Yapısı
```
WordLearner/
├── CLAUDE.md · docs/
├── WordLearner.sln
├── backend/{WordLearner.API, .Application, .Infrastructure, .Domain, .Tests}
├── admin/ · web/ · mobile/
```
