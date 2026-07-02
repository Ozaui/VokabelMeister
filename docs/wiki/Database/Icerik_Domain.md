# İçerik Domain (Words, Categories — sistem içeriği)

**Özet:** Yalnızca **Admin**'in yazabildiği, tüm giriş yapmış kullanıcıların okuyabildiği sistem içeriği — Almanca kelimeler, gramer detayları, örnek cümleler ve hiyerarşik kategoriler. `WordDetail`'in gramer alanları [[Alman_Dili_Ozellikleri]]'ndeki kurallara birebir dayanır. Kod olarak [[Gelistirme_Yol_Haritasi]]'nde **A-05 (Words)** ve **A-06 (Categories)** task'larında yazılacak.
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Alman_Dili_Ozellikleri]] · [[Roller_ve_Erisim]] · [[SRS_Domain]] · [[Kisisel_Icerik_Domain]]

## Words
`GermanWord`, `TurkishTranslation`, `PartOfSpeech` (`Noun|Verb|Adjective|Adverb|Conjunction|
Preposition|Pronoun|Other`), `DifficultyLevel` (A1-C2), `Definition`, `ImageUrl`, `IsActive`,
`CreatedBy`/`UpdatedBy`.

## WordDetails (1:1 Words — Almanca gramer)
`Gender` (Masculine/Feminine/Neuter — fiilde NULL), 4 hâlde belirli+belirsiz artikeller (8 alan),
`PluralForm`/`PluralFormDative`, `ConjugationData` (JSON: present/preterite/perfect/pastParticiple/
auxiliaryVerb), `IsSeparableVerb`/`SeparablePrefix`, `Pronunciation` (IPA), `AudioUrl`/`ImageUrl`,
`Notes`/`CommonMistakes`. Alan kaynağı ve kurallar → [[Alman_Dili_Ozellikleri]] §1-6.

## WordExamples (1:N Words)
`SentenceDE`/`SentenceTR`, `Level` (A1-C2 — kullanıcı seviyesine göre filtrelenir), `ExampleType`
(`Normal|Idiom|Formal|Colloquial`), `DisplayOrder`.

## Categories (hiyerarşik, self-ref)
`NameDE`/`NameTR`, `ParentCategoryId` (self-ref), `DisplayOrder`, `Icon`/`Color`, `MinLevel`/`MaxLevel`.
`WordCategories` ara tablosu (M:N) `Words`↔`Categories` bağlar.

## Planlanan Kod
- A-05: `Word`/`WordDetail`/`WordExample` entity → `IWordService`/`WordService` (liste filtre+sayfa,
  detay, CRUD Admin, duplikat 409 + `?force=true`) → `WordController`.
- A-06: `Category`/`WordCategory` entity → `ICategoryService`/`CategoryService` (hiyerarşik liste,
  silme koruması — alt kategori/aktif kelime varsa 409) → `CategoriesController`.
