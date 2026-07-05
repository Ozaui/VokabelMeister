# İçerik Domain (Words, Categories — sistem içeriği)

**Özet:** Yalnızca **Admin**'in yazabildiği, tüm giriş yapmış kullanıcıların okuyabildiği sistem içeriği — çoklu dile açık kelimeler (kavram + dil başına satır), gramer detayları, örnek cümleler ve hiyerarşik kategoriler. Şu an yalnızca Almanca-Türkçe içerik yazılıyor; yeni dil eklemek (örn. İngilizce) yeni migration gerektirmez, yalnızca `Languages`'e satır + ilgili kavramlara `Words`/`CategoryTranslations` satırı eklemektir. `WordDetail.GrammarData`'nın Almanca'ya özel şeması [[Alman_Dili_Ozellikleri]]'ndeki kurallara birebir dayanır. Kod olarak [[Gelistirme_Yol_Haritasi]]'nde **A-05 (Words)** ve **A-06 (Categories)** task'larında yazılacak. Kullanıcının kendi kişisel kartları (`UserCards`) bu domain'in **dışındadır** → [[Kisisel_Icerik_Domain]].
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Alman_Dili_Ozellikleri]] · [[Turkce_Dili_Ozellikleri]] · [[Ingilizce_Dili_Ozellikleri]] · [[Roller_ve_Erisim]] · [[SRS_Domain]] · [[Kisisel_Icerik_Domain]]

## Çoklu Dil Modeli (WordConcept)
Bir kelime, dilden bağımsız bir `WordConcepts` kaydına (kavram: `PartOfSpeech`/`DifficultyLevel`/
`ImageUrl`) ve o kavrama bağlı, her biri bir `Languages` satırına işaret eden N adet `Words` satırına
ayrılır — `WordConcepts (Id) ─┬─ Words(LanguageId=de) ─┬─ Words(LanguageId=tr) ─┬─ [ileride Words(LanguageId=en)]`.
Bir kelime oluşturulurken/düzenlenirken Almanca ve Türkçe karşılığı — kendi gramer/örnek cümleleriyle
birlikte — **aynı işlemde** girilir/güncellenir (sıralı değil). Kategori aynı mantıkla `Categories`
(çekirdek) + `CategoryTranslations` (dil başına ad) olarak ayrılır. Eşleştirme yönsüzdür: DE-TR, DE-EN,
TR-EN hepsi aynı `WordConceptId`/`CategoryId` üzerinden otomatik geçerlidir — ayrı bir çeviri
eşleştirme (M:N) tablosu **yok**, hub-and-spoke (1:N) yeterli.

## Languages
`Code` (ISO 639-1: `de`/`tr`/`en`…), `Name`, `NativeName`, `IsActive`, `DisplayOrder`. Seed: yalnızca
`de`+`tr`.

## WordConcepts (dilden bağımsız kavram)
`PartOfSpeech` (`Noun|Verb|Adjective|Adverb|Conjunction|Preposition|Pronoun|Other`), `DifficultyLevel`
(A1-C2), `ImageUrl` (dilden bağımsız — görsel tüm dillerde aynı), `IsActive`, `CreatedBy`/`UpdatedBy`.

## Words (1:N WordConcepts — bir kavramın tek bir dildeki karşılığı)
`WordConceptId` (FK), `LanguageId` (FK), `Text`, `Definition`, `IsActive`. `UNIQUE(WordConceptId,
LanguageId)` — bir kavramın bir dilde yalnızca bir karşılığı olabilir.

## WordDetails (1:1 Words — dile özel gramer)
Ortak: `Pronunciation` (IPA), `AudioUrl`, `Notes`/`CommonMistakes`. Gramer alanları (Almanca'da
`Gender`, 4 hâlde 8 artikel, `PluralForm`/`PluralFormDative`, `ConjugationData`, `IsSeparableVerb`/
`SeparablePrefix`) artık flat kolon değil, tek bir **`GrammarData` JSON** alanında — şekli
`Words.LanguageId`'ye göre değişir. Almanca şeması → [[Alman_Dili_Ozellikleri]]; Türkçe şeması (öncelikli,
aktif içerik — yön fark etmeksizin, bir Almanın Türkçe öğrenmesi dâhil) → [[Turkce_Dili_Ozellikleri]];
İngilizce şeması tanımlı ama henüz kullanılmıyor → [[Ingilizce_Dili_Ozellikleri]]. **Trade-off:**
`Gender` üzerindeki DB-level `CHECK`/`INDEX` kayboldu, filtreler artık uygulama katmanında
(`JSON_VALUE`).

## WordExamples (1:N Words — dile özel, basitleştirildi)
`SentenceText` (tek dilde, `SentenceDE`/`SentenceTR` ikilisi kaldırıldı — her dilin örnekleri bağımsız),
`Level` (A1-C2 — kullanıcı seviyesine göre filtrelenir), `ExampleType`
(`Normal|Idiom|Formal|Colloquial`), `DisplayOrder`.

## Categories (hiyerarşik, self-ref, dilden bağımsız çekirdek)
`ParentCategoryId` (self-ref), `DisplayOrder`, `Icon`/`Color`, `MinLevel`/`MaxLevel`.

## CategoryTranslations (1:N Categories — dil başına ad)
`CategoryId` (FK), `LanguageId` (FK), `Name`, `Description`. `UNIQUE(CategoryId, LanguageId)`.

## WordCategories (M:N — WordConcept ↔ Category)
Kategori **kavram üzerinden** bir kez etiketlenir (`WordConceptId`↔`CategoryId`), tüm diller otomatik
kapsanır — Türkçe/İngilizce eklenince tekrar etiketlemeye gerek yok.

## Planlanan Kod
- A-05: `Language`/`WordConcept`/`Word`/`WordDetail`/`WordExample` entity + `Language` seed (de/tr) →
  `IWordService`/`WordService` (liste filtre+sayfa, detay, CRUD Admin — bir kelime tüm dilleriyle
  `translations[]` şeklinde tek işlemde, duplikat 409 + `?force=true`) → `WordController`.
- A-06: `Category`/`CategoryTranslation`/`WordCategory` entity → `ICategoryService`/`CategoryService`
  (hiyerarşik liste, silme koruması — alt kategori/aktif kelime varsa 409) → `CategoriesController`.
