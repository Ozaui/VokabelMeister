# İçerik Domain (Words, Categories — sistem içeriği)

**Özet:** Yalnızca **Admin**'in yazabildiği, tüm giriş yapmış kullanıcıların okuyabildiği sistem içeriği — çoklu dile açık kelimeler (kavram + dil başına satır), gramer detayları, örnek cümleler ve hiyerarşik kategoriler. Şu an yalnızca Almanca-Türkçe içerik yazılıyor; yeni dil eklemek (örn. İngilizce) yeni migration gerektirmez, yalnızca `Languages`'e satır + ilgili kavramlara `Words`/`CategoryTranslations` satırı eklemektir. `WordDetail.GrammarData`'nın Almanca'ya özel şeması [[Alman_Dili_Ozellikleri]]'ndeki kurallara birebir dayanır. Kod olarak [[Gelistirme_Yol_Haritasi]]'nde **A-05 (Words)** ve **A-06 (Categories)** task'larında yazılacak. Kullanıcının kendi kişisel kartları (`UserCards`) bu domain'in **dışındadır** → [[Kisisel_Icerik_Domain]].
**Kütüphaneler:** —
**Bağlantılar:** [[Veritabani_Semasi]] · [[Alman_Dili_Ozellikleri]] · [[Turkce_Dili_Ozellikleri]] · [[Ingilizce_Dili_Ozellikleri]] · [[Roller_ve_Erisim]] · [[SRS_Domain]] · [[Kisisel_Icerik_Domain]]

## Çoklu Dil Modeli (WordConcept)
Bir kelime, dilden bağımsız bir `WordConcepts` kaydına (kavram: `PartOfSpeech`/`DifficultyLevel`/
`ImageUrl`) ve o kavrama bağlı, her biri bir `Languages` satırına işaret eden N adet `Words` satırına
ayrılır — `WordConcepts (Id) ─┬─ Words(LanguageId=de) ─┬─ Words(LanguageId=tr) ─┬─ [ileride Words(LanguageId=en)]`.
Bir kelime **iki yoldan** girilebilir: (a) Almanca+Türkçe karşılığı — kendi gramer/örnek
cümleleriyle birlikte — **aynı işlemde** girilir/güncellenir, ya da (b) her dil **ayrı** toplu
import'la kendi başına girilir (795 Almanca / N Türkçe satır gibi büyük hacimli içerik girişinde
gerçekçi olan yol) — bu durumda `WordConcept` geçici olarak **"eşleşmemiş"** kalır (tek dilde
`Words` satırı), Admin panelden ayrı bir **Eşleştirme** ekranıyla (bkz. Yirmi dokuzuncu INGEST)
sonradan birleştirilir; eşleşene kadar öğrenme oturumuna girmez. Kategori aynı mantıkla `Categories`
(çekirdek) + `CategoryTranslations` (dil başına ad) olarak ayrılır. Kavram bazında eşleşme yönsüzdür:
DE-TR, DE-EN, TR-EN hepsi aynı `WordConceptId`/`CategoryId` üzerinden otomatik geçerlidir — ayrı bir
çeviri eşleştirme (M:N) tablosu **yok**, hub-and-spoke (1:N) yeterli. **Öğrenme yönü** (`de→tr` mi
`tr→de` mi) kullanıcı profilinde sabit değil, her `LearningSession`'ın `TargetLanguageId`'si — aynı
hesap iki yönü de bağımsız ilerletebilir (bkz. [[SRS_Domain]]).

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

## Tür Bazlı GrammarData Doldurma Kuralı ve Eşleştirme
`WordGrammarValidator`, `Words.LanguageId`'ye göre dile dispatch eder, sonra dilin kendi
`PartOfSpeech` matrisini uygular — `de` → [[Alman_Dili_Ozellikleri]] §10, `tr` →
[[Turkce_Dili_Ozellikleri]] §9 (795 kelime/eşdeğer gerçek içerikten ölçülmüş desenler; iki dilin
matrisi birbirinden bağımsız, kopyalanamaz — ör. "bileşik kelime notu" `de`'de yalnızca Noun'da,
`tr`'de Noun+Verb'de). Eşleştirme uç noktaları: `GET /word-concepts/unmatched?languageId=`
(+ `suggestedMatchConceptId` — `Definition`↔`Text` örtüşmesiyle önerilen aday, elle taramayı azaltır),
`POST /word-concepts/{primaryId}/pair` — **bloklayıcı hata yok**: `PartOfSpeech`/kategori/seviye
çakışsa bile `primaryId`'ninki sessizce kazanır (diller arası tür kayması dilin doğası, veri hatası
değil — bkz. Otuzuncu INGEST).

## Planlanan Kod
- A-05: `Language`/`WordConcept`/`Word`/`WordDetail`/`WordExample` entity + `Language` seed (de/tr) →
  `WordGrammarValidator` (dil+tür bazlı koşullu kural) → `IWordService`/`WordService` (liste
  filtre+sayfa, detay, CRUD Admin — `translations[]` 1 veya 2 dil, duplikat 409 + `?force=true`) →
  eşleştirme (`GetUnmatchedWordConceptsQuery`/`PairWordConceptsCommand`) → `WordController`.
- A-06: `Category`/`CategoryTranslation`/`WordCategory` entity → `ICategoryService`/`CategoryService`
  (hiyerarşik liste, silme koruması — alt kategori/aktif kelime varsa 409) → `CategoriesController`.
