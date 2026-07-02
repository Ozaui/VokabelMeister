# GELİŞTİRME ORTAMI KURULUMU

## 1. Gerekli Araçlar
```
Node.js 18.17+ LTS · .NET 9 SDK · SQL Server 2019+ (SSMS) · VS 2022 / VS Code · Git 2.40+
```
Kontrol: `node --version` · `dotnet --version` · `git --version`

## 2. Projeyi Al
```bash
git clone https://github.com/kullanici/WordLearner.git && cd WordLearner
dotnet restore
```

## 3. Backend Proje İskeleti (A-01)
```bash
dotnet new sln -n WordLearner
dotnet new webapi   -n WordLearner.API            -o backend/WordLearner.API
dotnet new classlib -n WordLearner.Application    -o backend/WordLearner.Application
dotnet new classlib -n WordLearner.Infrastructure -o backend/WordLearner.Infrastructure
dotnet new classlib -n WordLearner.Domain         -o backend/WordLearner.Domain
dotnet new xunit    -n WordLearner.Tests          -o backend/WordLearner.Tests
dotnet sln add backend/**/*.csproj

# Referanslar: Domain ← Infrastructure ← Application ← API
dotnet add backend/WordLearner.API reference backend/WordLearner.Application backend/WordLearner.Infrastructure
dotnet add backend/WordLearner.Application reference backend/WordLearner.Domain
dotnet add backend/WordLearner.Infrastructure reference backend/WordLearner.Domain
# Test projesi: Application + Infrastructure'a referans verir (servisleri ve Repository<T>'yi test eder)
dotnet add backend/WordLearner.Tests reference backend/WordLearner.Application backend/WordLearner.Infrastructure
```
> **Klasör yapısı (Tests):** `WordLearner.Tests/{Services, Helpers, Repositories}/XxxServiceTests.cs` —
> her API task'ının "Birim testleri" adımı bu klasörlere yazılır. Detay → `REFERENCE/CODING_STANDARDS.md §8`.
NuGet listesi → `REFERENCE/TECHNICAL_SPECIFICATIONS.md §1`.

## 4. Veritabanı + Migration
`appsettings.Development.json`'da bağlantı dizesini ayarla (→ `REFERENCE/ENV.md`), sonra:
```bash
cd backend
dotnet ef migrations add InitialCreate --project WordLearner.Infrastructure --startup-project WordLearner.API
dotnet ef database update --project WordLearner.Infrastructure --startup-project WordLearner.API
```
> **Dikey dilim notu:** Her yeni API'da o API'ın entity'si için ayrı migration eklenir
> (`dotnet ef migrations add AddXxx`). Tüm tablolar tek seferde değil, API API büyür.

## 5. Backend Çalıştır
```bash
dotnet run --project WordLearner.API
# Swagger: http://localhost:5001/swagger  (yazdığın endpoint'ler otomatik görünür)
```

## 6. Admin Panel (B-01)
```bash
npm create vite@latest admin -- --template react-ts && cd admin
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom
npm i -D tailwindcss postcss autoprefixer && npx tailwindcss init -p
npm run dev   # http://localhost:5173
```
Admin'e yalnızca e-posta + şifre ile, `Admin` rolüyle giriş yapılır (Google/Apple yok).

## 7. Web App (D-01)
```bash
npm create vite@latest web -- --template react-ts && cd web
npm i @reduxjs/toolkit react-redux axios react-hook-form react-router-dom @react-oauth/google
npm i -D tailwindcss postcss autoprefixer && npx tailwindcss init -p
echo "VITE_API_URL=http://localhost:5001/api/v1" > .env.development
echo "VITE_GOOGLE_CLIENT_ID=..." >> .env.development
npm run dev   # http://localhost:5174
```
Token: `localStorage` (mobil'deki Expo Secure Store yerine). Apple web yok.

## 8. Mobil (E-01)
```bash
npx create-expo-app mobile --template expo-template-blank-typescript && cd mobile
npx expo install expo-secure-store expo-av expo-image-picker expo-apple-authentication
npm i @reduxjs/toolkit react-redux axios react-hook-form i18next react-i18next @react-native-google-signin/google-signin
npx expo install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/stack
echo "EXPO_PUBLIC_API_URL=http://localhost:5001/api/v1" > .env.development
npx expo start
```

## 9. Google / Apple Giriş (backend doğrulama)
```csharp
// Google: token doğrula → kendi JWT'mizi üret (Google token saklanmaz)
var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken);
var user = await _userRepo.GetByEmailAsync(payload.Email) ?? await CreateFromGoogleAsync(payload);

// Apple: identity token (JWT) Apple JWKS ile doğrulanır; email/isim yalnızca ilk girişte gelir
var claims = await ValidateAppleTokenAsync(identityToken);
```

## 10. IIS Yayınlama (Production)
```bash
dotnet publish backend/WordLearner.API -c Release -r win-x64 --self-contained false -o ./publish/api
```
- App Pool → **No Managed Code** (.NET Kestrel; IIS reverse proxy). URL Rewrite Module gerekli.
- Frontend: `npm run build` → `dist/`; React Router için `web.config`'e SPA rewrite kuralı eklenir.

## 11. Git Akışı
```
main → production · develop → aktif geliştirme · feature/ · bugfix/
```
Commit: `feat(auth): Google girişi eklendi` · `fix(word): çoğul kaydetme hatası` · `docs: ...`.

## 12. Klasör Yapısı
```
WordLearner/
├── CLAUDE.md · docs/ · "new md"/
├── WordLearner.sln
├── backend/{WordLearner.API, .Application, .Infrastructure, .Domain}
├── admin/ · web/ · mobile/
```
