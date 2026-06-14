# GELİŞTİRME ORTAMI KURULUMU

## 1. Teknoloji Stack

### Backend
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| .NET | 9 | Web API framework |
| EF Core | 9 | ORM ve migrations |
| MS SQL Server | 2019+ | Veritabanı |
| IIS | 10+ | Production yayınlama |
| MediatR | 12 | CQRS pattern |
| AutoMapper | 13 | DTO mapping |
| FluentValidation | 11 | Input doğrulama |
| BCrypt.Net-Next | 4 | Şifre hashleme |
| Serilog | 3 | Loglama |

### Admin Panel
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| React + Vite | 18+ / 5+ | UI + build |
| TypeScript | 5+ | Tip güvenliği |
| TailwindCSS | 3+ | Stil |
| RTK Query | 2+ | Server state |
| React Hook Form | 7+ | Form yönetimi |

### Mobile
| Teknoloji | Versiyon | Amaç |
|-----------|----------|------|
| React Native | 0.73+ | iOS & Android |
| Expo | SDK 50+ | Dev araçları |
| TypeScript | 5+ | Tip güvenliği |
| Redux Toolkit + RTK Query | 2+ | State yönetimi |
| Axios | 1+ | HTTP client |
| React Navigation | 6+ | Navigasyon |
| React Hook Form | 7+ | Form yönetimi |
| i18next | 23+ | Çok dil desteği |
| Expo Secure Store | — | Token güvenli saklama |

---

## 2. Geliştirme Ortamı Kurulumu (Bir Kez Yapılır)

### 2.1 Gerekli Araçlar

```
- Node.js 18.17+ LTS    → https://nodejs.org
- .NET 9 SDK            → https://dotnet.microsoft.com/download
- SQL Server 2019+      → SSMS ile birlikte kur
- Visual Studio 2022    → veya VS Code
- Git 2.40+
```

Kurulum sonrası kontrol:
```bash
node --version    # v18.17.0+
dotnet --version  # 9.0.0+
git --version     # 2.40+
```

### 2.2 Projeyi Bilgisayara Al

```bash
# Projeyi klonla (GitHub'dan indir)
git clone https://github.com/kullanici/WordLearner.git
cd WordLearner
```

### 2.3 Backend Bağımlılıklarını Yükle

```bash
# NuGet paketlerini yükle (node_modules'ün .NET karşılığı)
dotnet restore
```

### 2.4 Veritabanını Oluştur

SQL Server kurulu ve çalışıyor olmalı.

`backend/WordLearner.API/appsettings.Development.json` dosyasında bağlantı dizesini ayarla:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WordLearnerDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

Migration çalıştır (tabloları oluşturur):
```bash
cd backend
dotnet ef database update --project WordLearner.Infrastructure --startup-project WordLearner.API
```

### 2.5 Admin Panel Bağımlılıklarını Yükle

```bash
cd admin
npm install
```

### 2.6 Mobile Bağımlılıklarını Yükle

```bash
cd mobile
npm install
```

---

## 3. Backend Proje Kurulumu (TASK-001'de Yapılır)

```bash
mkdir WordLearner
cd WordLearner

# Solution
dotnet new sln -n WordLearner

# 4 proje
dotnet new webapi   -n WordLearner.API            -o backend/WordLearner.API
dotnet new classlib -n WordLearner.Application    -o backend/WordLearner.Application
dotnet new classlib -n WordLearner.Infrastructure -o backend/WordLearner.Infrastructure
dotnet new classlib -n WordLearner.Domain         -o backend/WordLearner.Domain

# Solution'a ekle
dotnet sln add backend/WordLearner.API/WordLearner.API.csproj
dotnet sln add backend/WordLearner.Application/WordLearner.Application.csproj
dotnet sln add backend/WordLearner.Infrastructure/WordLearner.Infrastructure.csproj
dotnet sln add backend/WordLearner.Domain/WordLearner.Domain.csproj

# Proje referansları (Domain ← Infrastructure ← Application ← API)
dotnet add backend/WordLearner.API/WordLearner.API.csproj \
    reference backend/WordLearner.Application/WordLearner.Application.csproj

dotnet add backend/WordLearner.Application/WordLearner.Application.csproj \
    reference backend/WordLearner.Domain/WordLearner.Domain.csproj

dotnet add backend/WordLearner.Infrastructure/WordLearner.Infrastructure.csproj \
    reference backend/WordLearner.Domain/WordLearner.Domain.csproj

dotnet add backend/WordLearner.API/WordLearner.API.csproj \
    reference backend/WordLearner.Infrastructure/WordLearner.Infrastructure.csproj
```

NuGet paket listesi → `docs/TECHNICAL_SPECIFICATIONS.md §1`

---

## 4. Admin Panel Kurulumu (TASK-019'da Yapılır)

```bash
npm create vite@latest admin -- --template react-ts
cd admin
npm install
npm install @reduxjs/toolkit react-redux axios react-hook-form react-router-dom
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
npm run dev
# → http://localhost:5173
```

Admin panelde Google/Apple girişi **yoktur**. Admin panele sadece e-posta + şifre ile giriş yapılır. Kullanıcı rolü `Admin` veya `Instructor` olmalıdır.

---

## 5. Mobile Kurulumu (TASK-025'te Yapılır)

```bash
npx create-expo-app mobile --template expo-template-blank-typescript
cd mobile

npx expo install expo-secure-store expo-av expo-image-picker expo-apple-authentication
npm install @reduxjs/toolkit react-redux axios react-hook-form i18next react-i18next
npx expo install @react-navigation/native @react-navigation/bottom-tabs @react-navigation/stack
npx expo install react-native-screens react-native-safe-area-context react-native-gesture-handler
npm install @react-native-google-signin/google-signin

# .env dosyaları
echo "EXPO_PUBLIC_API_URL=http://localhost:5001/api/v1" > .env.development
echo "EXPO_PUBLIC_API_URL=https://api.wordlearner.com/api/v1" > .env.production

npx expo start
```

---

## 6. Google ve Apple ile Giriş (Sadece Mobile)

### 6.1 Google Giriş

**Google Cloud Console adımları:**
1. `console.cloud.google.com` → Yeni proje
2. APIs & Services → Credentials → OAuth 2.0 Client ID
3. Android: Package name + SHA-1 fingerprint
4. iOS: Bundle ID
5. `google-services.json` (Android) ve `GoogleService-Info.plist` (iOS) indir

**Backend — Google token doğrulama:**
```bash
# NuGet paketi
dotnet add package Google.Apis.Auth --version 1.67.0
```

```csharp
// Kullanıcı mobil uygulamadan Google token alır → backend'e gönderir
// Backend Google'a sorar: "Bu token geçerli mi?"
// Geçerliyse kendi JWT'mizi üretiriz — Google token'ı saklamayız

public async Task<AuthResponseDto> GoogleLoginAsync(string googleIdToken)
{
    var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken);

    var user = await _userRepository.GetByEmailAsync(payload.Email)
            ?? await CreateUserFromGoogleAsync(payload);

    return await GenerateAuthResponseAsync(user);
}
```

### 6.2 Apple Giriş

**Apple Developer Console adımları:**
1. `developer.apple.com` → Identifiers → App ID
2. Sign In with Apple capability ekle
3. Services → Sign In with Apple → Configure

```csharp
// Apple identity token doğrulama
// Apple, Google'dan farklı — JWT formatında token gönderir
// Apple'ın public key'leri ile doğrulama yapılır

public async Task<AuthResponseDto> AppleLoginAsync(string identityToken, string? firstName, string? lastName)
{
    var claims = await ValidateAppleTokenAsync(identityToken);
    var appleUserId = claims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    var email = claims.FindFirst(ClaimTypes.Email)?.Value;

    // Not: Apple, email ve ismi sadece ilk girişte gönderir
    var user = await _userRepository.GetByAppleIdAsync(appleUserId)
            ?? await CreateUserFromAppleAsync(appleUserId, email, firstName, lastName);

    return await GenerateAuthResponseAsync(user);
}
```

---

## 7. IIS ile Production Yayınlama

### 7.1 Backend (.NET 9 → IIS)

```bash
dotnet publish backend/WordLearner.API \
    -c Release \
    -r win-x64 \
    --self-contained false \
    -o ./publish/api
```

**IIS Adımları:**
1. IIS Manager → Sites → Add Website
2. Physical path: `publish/api` klasörü
3. Application Pool → .NET CLR Version: **No Managed Code**
   (.NET 9 Kestrel kullanır, IIS sadece reverse proxy yapar)
4. URL Rewrite Module kurulu olmalı

**web.config** (publish klasöründe otomatik oluşur):
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*"
             modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\WordLearner.API.dll"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

### 7.2 Admin Panel (React → IIS)

```bash
cd admin
npm run build
# dist/ klasörü oluşur
```

**IIS Adımları:**
1. Physical path: `dist/` klasörü
2. Application Pool → No Managed Code

**web.config** (`dist/` içine koy — React Router için):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="React Router" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## 8. Git Kullanımı

### 8.1 Branch Stratejisi

```
main     → production (IIS'te çalışan, dokunma)
develop  → aktif geliştirme
feature/ → yeni özellik branch'i
bugfix/  → hata düzeltme branch'i
```

### 8.2 Günlük Çalışma Akışı

```bash
# 1. Sabah — develop'u güncelle
git checkout develop
git pull origin develop

# 2. Yeni özellik için branch aç
git checkout -b feature/kelime-ekleme

# 3. Kodunu yaz...

# 4. Değişiklikleri kaydet
git add .
git commit -m "feat(word): kelime oluşturma servisi eklendi"

# 5. GitHub'a gönder
git push origin feature/kelime-ekleme

# 6. Bitince develop'a merge et
git checkout develop
git merge feature/kelime-ekleme
git push origin develop
```

### 8.3 Commit Mesajı Kuralları

```bash
feat:     → yeni özellik       → "feat(auth): Google girişi eklendi"
fix:      → hata düzeltme      → "fix(word): çoğul form kaydetme hatası düzeltildi"
docs:     → dokümantasyon      → "docs: API endpoint açıklamaları güncellendi"
refactor: → kod düzenleme      → "refactor(repo): generic repository sadeleştirildi"
test:     → test ekleme        → "test(srs): SM-2 algoritması unit testi eklendi"
```

### 8.4 Merge Conflict Nedir?

İki farklı branch'te aynı dosyanın aynı satırı değiştirilirse Git hangisini alacağını bilemez. Buna merge conflict denir.

```
Örnek:
  Sen:      feature/login → AuthController.cs 10. satırı değiştirdin
  Arkadaşın: feature/register → aynı dosyanın aynı satırını değiştirdi
  İkisi develop'a merge edilmek isteniyor
  Git: "Hangisini alayım?" → Conflict!
```

Tek başına çalışıyorsan conflict neredeyse hiç çıkmaz.

Çıkarsa VS Code veya Visual Studio conflict editörü ile kolayca çözülür:
```
<<<<<<< HEAD (senin değişikliğin)
public async Task Login() { ... }
=======
public async Task Login(LoginDto dto) { ... }
>>>>>>> feature/register (gelen değişiklik)
```
İkisinden birini seç, kaydet, commit at.

---

## 9. Klasör Yapısı

```
WordLearner/
├── CLAUDE.md
├── WordLearner.sln
├── docs/
│   ├── TASK.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE_SCHEMA.md
│   ├── API_ENDPOINTS.md
│   ├── TECHNICAL_SPECIFICATIONS.md
│   ├── CODING_STANDARDS.md
│   ├── SECURITY.md
│   ├── GERMAN_LANGUAGE_FEATURES.md
│   └── DEVELOPMENT_SETUP.md
├── backend/
│   ├── WordLearner.API/
│   ├── WordLearner.Application/
│   ├── WordLearner.Infrastructure/
│   └── WordLearner.Domain/
├── admin/          ← React + Vite admin panel
└── mobile/         ← React Native Expo
```
