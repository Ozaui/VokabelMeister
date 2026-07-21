# İçerik Domain — Sistem Kelimeleri ve Kategoriler

> Genel + çoklu dil kuralları → `../CLAUDE.md §1`. Bu dosyadaki tüm tablolar **yalnızca Admin yazar**,
> giriş yapmış herkes salt-okur. Kullanıcının kendi kartları ayrı → `Kisisel_Icerik.md`.
> Bir kelime `WordConcept` (kavram) + her dile bir `Words` satırıdır; `de`+`tr` **aynı işlemde** girilir.

### Languages
```sql
CREATE TABLE Languages (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(5) NOT NULL,           -- ISO 639-1: 'de','tr','en'...
    Name NVARCHAR(50) NOT NULL,          -- 'German','Turkish'
    NativeName NVARCHAR(50) NOT NULL,    -- 'Deutsch','Türkçe'
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Languages_Code UNIQUE (Code)
);
-- Seed: yalnızca de+tr. Üçüncü dil = tek satırlık INSERT, migration gerekmez.
INSERT INTO Languages (Code, Name, NativeName, DisplayOrder) VALUES
('de','German','Deutsch',1), ('tr','Turkish','Türkçe',2);
```

### WordConcepts (dilden bağımsız kavram — kategori/seviye burada)
```sql
CREATE TABLE WordConcepts (
    Id INT PRIMARY KEY IDENTITY,
    PartOfSpeech NVARCHAR(20) NOT NULL,   -- Noun|Verb|Adjective|Adverb|Conjunction|Preposition|Pronoun|Other
    DifficultyLevel NVARCHAR(2) NOT NULL, -- A1..C2
    ImageUrl NVARCHAR(500) NULL,          -- görsel dilden bağımsız
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NULL, UpdatedBy INT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_WordConcepts_Level CHECK (DifficultyLevel IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordConcepts_PartOfSpeech CHECK (PartOfSpeech IN ('Noun','Verb','Adjective','Adverb','Conjunction','Preposition','Pronoun','Other')),
    INDEX IX_WordConcepts_DifficultyLevel (DifficultyLevel)
);
```

### Words (bir kavramın tek dildeki karşılığı)
```sql
CREATE TABLE Words (
    Id INT PRIMARY KEY IDENTITY,
    WordConceptId INT NOT NULL,
    LanguageId INT NOT NULL,
    Text NVARCHAR(255) NOT NULL,          -- 'Tisch' (de) / 'masa' (tr)
    Definition NVARCHAR(MAX) NULL,        -- serbest "anlam notu" — dili SABİT DEĞİL (pratikte çoğunlukla
                                           -- karşı dilde kısa gloss, ör. 'aber'→"ama, fakat, ancak").
                                           -- Kartta "resmi çeviri" DEĞİLDİR (o eşleşen Words.Text'ten gelir) —
                                           -- birincil işlevi ayrı-girilen içerikte eşleştirme ipucu olmak
                                           -- (bkz. "Eşleştirme" bölümü, suggestedMatchConceptId).
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT UQ_Words_Concept_Language UNIQUE (WordConceptId, LanguageId),
    INDEX IX_Words_LanguageId_Text (LanguageId, Text)
);
```

### WordDetails (dile özel gramer — 1:1 Words)
```sql
CREATE TABLE WordDetails (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL UNIQUE,
    Pronunciation NVARCHAR(500) NULL,        -- IPA
    AudioUrl NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL, CommonMistakes NVARCHAR(MAX) NULL,
    GrammarData NVARCHAR(MAX) NULL,          -- JSON — şekli Words.LanguageId'ye göre değişir
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE
);
```
> **GrammarData JSON şeması dil bazında:** de → `REFERENCE/GERMAN_LANGUAGE_FEATURES.md`, tr → `REFERENCE/TURKISH_LANGUAGE_FEATURES.md`, en → `REFERENCE/ENGLISH_LANGUAGE_FEATURES.md` (tanımlı ama henüz kullanılmıyor).
> **Trade-off:** `Gender` üzerinde DB `CHECK`/`INDEX` yok; "tüm maskülinleri getir" gibi filtreler uygulama katmanında (`JSON_VALUE`). Bu ölçekte sorun değil.

### WordExamples (seviyeli örnek cümleler — 1:N Words)
```sql
CREATE TABLE WordExamples (
    Id INT PRIMARY KEY IDENTITY,
    WordId INT NOT NULL,
    SentenceText NVARCHAR(MAX) NOT NULL,   -- bu Word'ün dilinde tek örnek cümle
    Level NVARCHAR(2) NOT NULL DEFAULT 'A1',
    ExampleType NVARCHAR(20) NOT NULL DEFAULT 'Normal',  -- Normal|Idiom|Formal|Colloquial
    PairedExampleId INT NULL,              -- karşı dildeki WordExamples satırı — YALNIZCA gerçekten
                                            -- birlikte girildiyse (translations[] tek işlem) veya admin
                                            -- eşleştirme sırasında elle bağladıysa dolar. NULL ise bu
                                            -- örnek BAĞIMSIZDIR — "çeviri" değil, o kelimeyi kullanan
                                            -- ayrı bir cümle; UI bunu "Örnek Cümle Anlamı" gibi SUNMAZ.
    DisplayOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordId) REFERENCES Words(Id) ON DELETE CASCADE,
    FOREIGN KEY (PairedExampleId) REFERENCES WordExamples(Id),
    CONSTRAINT CK_WordExamples_Level CHECK (Level IN ('A1','A2','B1','B2','C1','C2')),
    CONSTRAINT CK_WordExamples_ExampleType CHECK (ExampleType IN ('Normal','Idiom','Formal','Colloquial')),
    INDEX IX_WordExamples_WordId_Level (WordId, Level)
);
```
> **`PairedExampleId` neden gerekli:** İki dil ayrı import edildiğinde, aynı kavramın Almanca ve
> Türkçe örnek cümleleri **birbirinin çevirisi olacağı garanti edilemez** — ikisi de o kelimeyi
> kullanan bağımsız cümlelerdir. Yalnızca (a) `translations[]` ile tek işlemde birlikte girilen veya
> (b) eşleştirme ekranında admin'in elle "bu ikisi çeviri" diye işaretlediği örnekler bu alanla
> bağlanır ve kartta "Örnek Cümle Anlamı" olarak gösterilir. Bağlanmamış örnekler her dilde
> **ayrı ayrı** gösterilir, çeviri iddia edilmez.

### Categories (hiyerarşik, dilden bağımsız çekirdek)
```sql
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY,
    ParentCategoryId INT NULL,               -- self-ref
    DisplayOrder INT NOT NULL DEFAULT 0,
    Icon NVARCHAR(100) NULL, Color NVARCHAR(10) NULL,
    MinLevel NVARCHAR(2) NULL, MaxLevel NVARCHAR(2) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0, DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(), UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id),
    INDEX IX_Categories_ParentCategoryId (ParentCategoryId)
);
```

### CategoryTranslations (dil başına 1 satır)
```sql
CREATE TABLE CategoryTranslations (
    Id INT PRIMARY KEY IDENTITY,
    CategoryId INT NOT NULL, LanguageId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL, Description NVARCHAR(MAX) NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    FOREIGN KEY (LanguageId) REFERENCES Languages(Id),
    CONSTRAINT UQ_CategoryTranslations_Category_Language UNIQUE (CategoryId, LanguageId)
);
```

### WordCategories (WordConcept ↔ Category M:N)
```sql
CREATE TABLE WordCategories (
    Id INT PRIMARY KEY IDENTITY, WordConceptId INT NOT NULL, CategoryId INT NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0, CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WordConceptId) REFERENCES WordConcepts(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WordCategories UNIQUE (WordConceptId, CategoryId)
);
```

## Eşleştirme (Pairing) — `de` ve `tr` İçeriğinin Ayrı Girilip Sonra Birleştirilmesi

> `DATABASE_SCHEMA.md`'deki eski not ("Karşılaştırılacak LanguageId... C-fazında netleşir") burada
> çözüldü. Karar: **`de` ve `tr` içeriği ayrı ayrı, kendi toplu import akışlarıyla girilir**
> (`GERMAN_LANGUAGE_FEATURES.md §10` / `TURKISH_LANGUAGE_FEATURES.md §9` kendi dilinin matrisine göre
> doğrulanır) — tek işlemde `translations[]` ile birlikte girme **artık zorunlu değil**, ek bir yol.

**Eşleşmemiş (unmatched) kavram:** Bir `WordConcept`, altında yalnızca **tek dilde** `Words` satırı
varken "eşleşmemiş" sayılır — ayrı bir `IsMatched` kolonu **açılmaz** (YAGNI); durum
`COUNT(DISTINCT Words.LanguageId) = 1` ile türetilir. Tek dilli import (`de`-only veya `tr`-only)
her satır için kendi `WordConcept`'ini oluşturur (`PartOfSpeech`/`DifficultyLevel`/kategoriler o an
girilir) + o dilin `Words`/`WordDetail`/`WordExample`'ı. **Eşleşene kadar bu kelime hiçbir öğrenme
oturumuna girmez** — `LearningSessions` sorgusu yalnızca 2 dilli (eşleşmiş) `WordConcept`'leri kaynak alır.

**Eşleştirme işlemi (Admin panel, B-03'ün bir parçası — bkz. A-05):**
- `GET /words/unmatched?languageId={id}` — o dilde eşleşmemiş kavramların listesi (arama+sayfa),
  her satırda **`suggestedMatchConceptId`** (varsa) — karşı dilin eşleşmemiş havuzunda bu kavramın
  `Definition`'ı (serbest anlam notu, bkz. `Words.Definition` yorumu) ile karşı adayın `Text`'i
  (veya tersi) örtüşen en iyi aday. **`Definition` virgülle (`,`) ayrılmış birden fazla karşılık
  içerebilir** (ör. "ama, fakat, ancak") — algoritma bunu TEK bir string olarak değil, virgülle
  **token'lara bölüp her birini ayrı ayrı** karşı `Text` havuzuna karşı dener (yoksa bütün string
  hiçbir tekil kelimeye tam eşleşmez, öneri hiç bulunmaz). **Amaç:** 795+ satırı admin'in elle tek
  tek taraması yerine, halihazırda satırlarda duran karşı-dil glossu bir öneri olarak kullanmak —
  admin yalnızca **onaylar**, sıfırdan aramaz. Öneri yoksa liste manuel taranır.
- `POST /words/pair` `{ "primaryId": Y, "otherConceptId": X }` — `otherConceptId`'nin tek
  `Words` satırını `primaryId`'ye taşır (`WordConceptId` güncellenir), `otherConceptId`'yi (artık
  boş kalan kavram) siler. Sonuç: tek `WordConcept`, 2 dilde `Words`. **Bloklayıcı hata yok** —
  eşleştirme her zaman başarılı olur.
- **"Birincil taraf" (`primaryId`) nasıl seçilir:** varsayılan olarak admin'in **"Eşleştir"
  işlemini başlattığı** taraf — `WordPairingPage`'de hangi satırın üzerinde "Eşleştir" butonuna
  basılırsa `primaryId` odur (URL zaten bunu ifade eder). Onaylamadan önce arayüzde açık bir
  **"birincil tarafı değiştir"** kontrolü bulunur, admin isterse karşı tarafı birincil seçebilir —
  davranış rastgele/click-order'a bağlı kalmaz, her zaman görünür ve değiştirilebilir bir seçimdir.
- **`PartOfSpeech` uyuşmazlığı ARTIK 409 DEĞİL.** İki dil arasında çeviri edilen bir kavramın gramer
  türü doğal olarak kayabilir (ör. Almanca ayrılabilir bir fiil Türkçede bir deyim/isim tamlaması
  olabilir) — bu bir veri hatası değil, dillerin doğası. `primaryId`'nin `PartOfSpeech`'i sessizce
  kazanır, arayüzde yalnızca bilgilendirme amaçlı gösterilir ("Not: türler farklı — bu normal
  olabilir"), onay istenmez.
- **`WordCategories` (Kategori/tema) çakışması** da bloklamaz ama **bilgilendirme değeri daha
  yüksektir** (ör. "aile" temalı bir kelime yanlışlıkla "iş" kategorisiyle eşleşiyorsa bu gerçekten
  fark edilmeye değer) — `primaryId`'ninki sessizce korunur, admin ekranında iki tarafın kategorileri
  yan yana gösterilir ki admin isterse eşleştirmeden önce vazgeçebilir.
- **`DifficultyLevel` (Seviye, A1-C2)** — `WordConcepts.DifficultyLevel`, tür-matrisinden bağımsız
  concept-seviyesi bir alan (§2/§3 matrislerinde yer almaz, çünkü türe göre değişmez). Çakışmada
  `primaryId`'ninki korunur, kritik sayılmaz.
- **Bilinçli sınırlama — çoklu eşanlamlı kaybı:** `UQ_Words_Concept_Language` bir `WordConcept`'in
  bir dilde yalnızca **tek** `Words` satırına sahip olmasını zorunlu kılar. `Definition`="ama,
  fakat, ancak" gibi çok-adaylı durumlarda eşleştirme yalnızca **birini** (öneri onaylanan) resmi
  çift yapar — diğerleri ayrı, sonsuza dek eşleşmemiş Türkçe kavramlar olarak kalabilir ve o
  Almanca kelimeyle birlikte kullanıcıya hiç gösterilmez. Bu **kabul edilen bir sadeleştirme**
  (çoğu flashcard uygulaması zaten tek bir birincil çeviri gösterir) — şema büyütülmez, çoklu
  eşanlamlı birinci sınıf desteklenmez (YAGNI). İstenirse admin, eşleşmeyen ek eşanlamlıları
  mevcut `WordDetails.Notes` alanına serbest metin olarak ekleyebilir (yeni kolon açılmaz).

**Yön farkındalığı (hangi dilden hangi dile):** Bir `WordConcept` eşleşince kullanıcı onu **iki
yönde de** öğrenebilir — `de→tr` (Almanca gramer test edilir, Türkçe yalnızca anlam) veya `tr→de`
(tersi). Hangi yönün aktif olduğu **kullanıcı profilinde sabit bir alan değil**, her öğrenme
oturumunun (`LearningSessions.TargetLanguageId`) parametresi — bkz. `SRS.md` ve `API_ENDPOINTS.md §9`.
`UserProgress`/`UserCardProgress` zaten `WordId` (dile özel satır) üzerinden anahtarlandığı için bu
**şema değişikliği gerektirmez**: aynı kullanıcı aynı kavram için hem Almanca `Words.Id`'sinde hem
Türkçe `Words.Id`'sinde **ayrı ayrı** `UserProgress` satırına sahip olabilir — iki yön birbirinden
bağımsız ilerler.
