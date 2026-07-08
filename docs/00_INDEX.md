# PROJE İNDEKSİ

> **Kurallar ve yönlendirme → `CLAUDE.md`** (her oturumda önce onu oku). Bu dosya yalnızca
> teknoloji özeti + dosya haritasıdır.

## Teknoloji

| Katman | Stack |
|--------|-------|
| Backend | .NET 9 Web API + EF Core 9 + MS SQL Server |
| Admin Panel | React + Vite + TS (`/admin`) — yalnızca Admin |
| Web App | React + Vite + TS + React Router v6 (`/web`) |
| Mobil | React Native (Expo) + TS (`/mobile`) |

Kimlik doğrulama: ASP.NET Identity **yok**, JWT + şifre hashleme manuel. Roller: yalnızca `User`/`Admin`.

## Geliştirme Sırası (Faz)

```
A) Admin BE → B) Admin Panel → C) Kullanıcı BE → D) Web → E) Mobil → F) Test & Yayın
```
Admin önce (içerik girilir, kullanıcı tarafı gerçek veriyle test edilir); web mobilden önce (hızlı test, mobile referans).

## Dosya Haritası

| Dosya | İçerik |
|-------|--------|
| `CLAUDE.md` | ⭐ Agent anayasası: değişmez kurallar + yönlendirme + yazım sırası |
| `TASK.md` | Yöntem + ilerleme; faz task listeleri → `TASK/<faz>.md` |
| `DATABASE_SCHEMA.md` | ERD + genel kurallar; tam SQL → `DATABASE_SCHEMA/<domain>.md` |
| `REFERENCE/ARCHITECTURE.md` | Mimari, akışlar, rol/görünürlük matrisi |
| `REFERENCE/API_ENDPOINTS.md` | Endpoint listesi + istek/yanıt örnekleri |
| `REFERENCE/CODING_STANDARDS.md` | Yorum/isim standardı + birim test standardı |
| `REFERENCE/SECURITY.md` | Auth, JWT, OTP, QR, şifreleme, loglama, GDPR |
| `REFERENCE/TECHNICAL_SPECIFICATIONS.md` | NuGet/npm, JWT/SM-2/Repository kod örnekleri |
| `REFERENCE/ENV.md` | Ortam değişkenleri |
| `REFERENCE/DEVELOPMENT_SETUP.md` | Kurulum, çalıştırma, yayınlama |
| `REFERENCE/{GERMAN,TURKISH,ENGLISH}_LANGUAGE_FEATURES.md` | `WordDetail.GrammarData` gramer şemaları (de aktif, tr öncelikli, en hazır ama kullanılmıyor) |
| `API_YOL_HARITASI/`, `ADMIN/WEB/MOBILE_YOL_HARITASI/` | Junior eğitim roadmap'i (HTML); `_TASLAK.html` = şablon |
| `wiki/` | Obsidian mimari hafıza; `wiki/Index.md` ana harita, kurallar `wiki_schema.md` |
