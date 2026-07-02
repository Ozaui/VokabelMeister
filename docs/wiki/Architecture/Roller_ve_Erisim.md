# Roller ve Erişim

**Özet:** Sistemde yalnızca iki rol vardır — `User` ve `Admin` — "Instructor/Teacher/öğretmen" kavramı yoktur ve hiçbir public endpoint rol yükseltemez. İçerik sahipliği üç katmanlıdır: sistem içeriği (yalnızca Admin yazar, herkes okur), kişisel içerik (yalnızca sahibi), sınıf içeriği (yalnızca sınıf sahibi yazar, üyeler okur). Kaynak yetkisi her sorguda `UserId` filtresiyle uygulanır; başkasının kaydına erişim 404/403 döner.
**Kütüphaneler:** ASP.NET Core `[Authorize]` / `[Authorize(Roles="Admin")]`
**Bağlantılar:** [[Sistem_Mimarisi]] · [[Guvenlik_Politikalari]] · [[Auth_Domain]] · [[Sosyal_Domain]] · [[Kisisel_Icerik_Domain]]

## İçerik Sahipliği Tablosu

| İçerik | Entity | Oluşturan/düzenleyen | Gören |
|--------|--------|----------------------|-------|
| Sistem içeriği | `Word`, `Category` | Yalnızca **Admin** | Tüm giriş yapmışlar (okuma) |
| Kişisel içerik | `UserCard`, `UserCategory` | Yalnızca **sahibi** (UserId) | Yalnızca sahibi (+ sınıfa atanırsa üyeler) |
| Sınıf kelimesi | `ClassWord` | Yalnızca **sınıf sahibi** | Yalnızca sınıf üyeleri |

## Kurallar

- Herkes `User` olarak kayıt olur; `Admin` yalnızca elle atanır.
- Sistem içeriği CRUD → `[Authorize(Roles="Admin")]`; okuma → `[Authorize]`.
- Kişisel kayda yalnızca yazan kullaıcı erişir; sorgularda `UserId` filtresi zorunlu.
- Sınıflar (`Class`) herhangi bir `User` tarafından oluşturulabilir; sahip `Class.OwnerId`. Sınıf içi
  alt rol yok — yalnızca `Owner`/`Member`.

## İçerik Görünürlük Matrisi

| İçerik | Kim ekler | Varsayılan | Herkese açık | Paylaşım linki | Sınıf |
|--------|-----------|-----------|--------------|----------------|-------|
| Sistem kelimeleri | Admin | ✅ herkes | — | — | — |
| Sınıf kelimeleri | Sınıf sahibi | 🔒 üyeler | ❌ | ❌ | — |
| Kullanıcı kartı/kategorisi | Her user | 🔒 sahibi | ❌ (admin onayı modeli yok) | ✅ linki olan | ✅ üyeler |

> **Not:** "Herkese açık + admin onayı" modeli kaldırıldı; gerçek paylaşım mekanizması
> [[Sosyal_Domain]]'deki `SharedContents` (link tabanlı, admin onayı gerektirmez).
