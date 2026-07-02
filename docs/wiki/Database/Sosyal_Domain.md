# Sosyal Domain (Classes, Friendships, SharedContents)

**Özet:** Kullanıcılar arası sosyal özellikler — sınıflar (davet kodlu içerik paylaşımı), arkadaşlık istekleri ve UUID token'lı içerik paylaşım linkleri. Sınıf içi alt rol yoktur, yalnızca `Owner`/`Member` ayrımı vardır ([[Roller_ve_Erisim]]). Kod olarak **C-06/C-07/C-08**'de yazılacak.
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Roller_ve_Erisim]] · [[Kisisel_Icerik_Domain]] · [[Icerik_Domain]]

## Classes / ClassWords / ClassMemberships
`Classes`: `OwnerId`, `Name`/`Description`, `InviteCode` (UNIQUE). Sahip: ad/bilgi güncelle,
kelime/kategori ata, sınıfı sil. Üyeler davet koduyla katılır, ayrılabilir.
`ClassWords`: sınıfa özel kelimeler — yalnızca sınıf sahibi ekler/düzenler/siler, **yalnızca
üyeler görür** (`WHERE ClassId IN (kullanıcının üye olduğu sınıflar)`). Gramer alanları
(Gender/ArticleDefiniteNom/PluralForm) sistem `Words`'e benzer ama basitleştirilmiş.
`ClassCategories`/`ClassUserCategories` (M:N) — sınıfa sistem kategorisi / kişisel kategori atama.

## Friendships
`RequesterId`/`ReceiverId`, `Status` (`Pending|Accepted|Rejected|Blocked`). Self-friendship
`CHECK` kısıtıyla engellenir.

## SharedContents / SharedContentImports
`ShareToken` (UUID, UNIQUE), `ContentType` (`UserCard|UserCategory|Class`), `ContentId`,
`ExpiresAt`, `ViewCount`. Akış: "Paylaş" → link oluştur → arkadaş **anonim önizleme** ile açar →
giriş yapıp "listeme ekle" (`SharedContentImports` benzersiz `(SharedContentId, ImportedByUserId)`).

## Planlanan Kod
- C-06 (Paylaşım): `SharedContent`/`SharedContentImport` → `IShareService`/`ShareService` (UUID
  link, anonim önizleme, listene kopyala) → `SharedContentController`.
- C-07 (Sınıf): `Class`/`ClassMembership`/`ClassWord`/`ClassCategory`/`ClassUserCategory` →
  `IClassService`, `IClassWordService` → `ClassController`.
- C-08 (Arkadaş): `Friendship` → `IFriendshipService` → `FriendshipController`.
