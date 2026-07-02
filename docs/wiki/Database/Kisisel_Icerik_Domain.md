# Kişisel İçerik Domain (UserCards, UserCategories)

**Özet:** Her kullanıcının kendi oluşturduğu kartlar ve kategoriler — yalnızca sahibi görür/düzenler, her sorguda `UserId` filtresi zorunludur. `UserCard.FrontText` sistem `Words.GermanWord` ile eşleşirse kullanıcıya "öğrenme listene ekleyelim mi?" sorulur → Evet ise [[SRS_Domain]]'deki `UserProgress` açılır (UserCard oluşturulmaz). Kod olarak **C-02 (UserCategory)** ve **C-04 (UserCard)** task'larında yazılacak.
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Roller_ve_Erisim]] · [[SRS_Domain]] · [[Icerik_Domain]] · [[Sosyal_Domain]]

## UserCards + UserCardExamples
`UserId` (sahip, zorunlu filtre), `FrontText`/`BackText`, `Notes`, `ImageUrl`/`AudioUrl`, `IsActive`.
`UserCardExamples` (1:N): `SentenceFront`/`SentenceBack`, `DisplayOrder`.

## UserCategories + Ara Tablolar
`UserCategories`: `UserId`, `Name`, `Description`, `Color`/`Icon`.
- `UserCardCategories` (M:N) — `UserCard` ↔ sistem `Category`
- `UserCardUserCategories` (M:N) — `UserCard` ↔ kişisel `UserCategory`

## Önemli İş Kuralı
`UserCard.FrontText` bir sistem `Words.GermanWord` ile eşleşir ve kullanıcının o kelime için
`UserProgress` kaydı varsa, Mixed sınav oturumunda o `UserCard` **atlanır** — `UserProgress`
önceliklidir (çift öğrenme kaydı önlenir).

## Planlanan Kod
- C-02: `UserCategory` entity → `IUserCategoryService`/`UserCategoryController` (yalnızca sahibi).
  Sıra bilinçli değişti: C-04'ün ihtiyacı olan ara tablo FK'sı önce hazır olmalı.
- C-04: `UserCard`/`UserCardExample` + ara tablolar → `IUserCardService`/`UserCardService`
  (liste/detay/CRUD sahiplik filtresi, duplikat 409 + `?force=true`, sistem eşleşme uyarısı
  `suggestedSystemWordId`) → `POST /user-cards/learn-system-word` (UserProgress açar) →
  `UserCardController`. `UserCategory` ve `UserProgress` (C-03) önce hazır olmalı.
