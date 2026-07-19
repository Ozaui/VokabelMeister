# ALMANCA DİL ÖZELLİKLERİ

> Almanca (`de`) `WordDetail.GrammarData` JSON şeması — kelime kartı/sınav referansı. Çoklu dil yapısı → `CLAUDE.md §1`.
> DB'de `Gender` üzerinde CHECK/INDEX yok, doğrulama uygulama katmanında (`DATABASE_SCHEMA/Icerik.md` trade-off).

## 1. Cinsiyet (Gender) ve Kart Rengi

| Cinsiyet | Belirli | Belirsiz | Çoğul | Renk |
|----------|---------|----------|-------|------|
| **Maskulin** (der) | der | ein | die | 🔵 Mavi |
| **Feminin** (die) | die | eine | die | 🔴 Kırmızı |
| **Neutrum** (das) | das | ein | die | 🟢 Yeşil |

Örnek: der Mann, die Frau, das Kind, das Buch, der Hund.

## 2. Artikeller (4 Hâl)
```
Belirli:            NOM   AKK   DAT   GEN        Belirsiz:  NOM   AKK    DAT    GEN
MASKULIN            der   den   dem   des        MASKULIN   ein   einen  einem  eines
FEMININ             die   die   der   der        FEMININ    eine  eine   einer  einer
NEUTRUM             das   das   dem   des        NEUTRUM    ein   ein    einem  eines
ÇOĞUL               die   die   den   der
```
Olumsuz: kein/keine/kein/keine.

## 3. Dört Hâl

| Hâl | Soru | Fonksiyon | Örnek |
|-----|------|-----------|-------|
| **Nominativ** | Wer? Was? | Özne | **Der Mann** geht. |
| **Akkusativ** | Wen? Was? | Düz nesne | Ich sehe **den Mann**. |
| **Dativ** | Wem? | Dolaylı nesne | Ich helfe **dem Mann**. |
| **Genitiv** | Wessen? | İyelik | Das Auto **des Mannes**. |

```
der Mann → Nom: der Mann · Akk: den Mann · Dat: dem Mann · Gen: des Mannes
die Frau → Nom/Akk: die Frau · Dat/Gen: der Frau
das Kind → Nom/Akk: das Kind · Dat: dem Kind · Gen: des Kindes
ÇOĞUL    → Dativ'de ekstra -n: den Männern
```

## 4. Çoğul Kalıpları

| Kalıp | Örnek |
|-------|-------|
| -e (çoğu maskulin) | der Tisch → die Tische |
| -n / -en (çoğu feminin) | die Frau → die Frauen |
| -er (+umlaut) | das Kind → die Kinder · der Mann → die Männer |
| -s (yabancı) | das Auto → die Autos |
| Değişim yok | das Fenster → die Fenster |
| Umlaut (a→ä, o→ö, u→ü) | der Apfel → die Äpfel |

## 5. Ayrılabilir Fiiller

Ön ek ayrılır, cümle sonuna gider: `anrufen` → Ich rufe dich **an**. (Perfekt: angerufen) · `aufstehen` → Ich stehe um 6 Uhr **auf**.
Yaygın ön ekler: ab-, an-, auf-, aus-, ein-, mit-, vor-, weg-, zu-, zurück-, zusammen-.
`GrammarData`: `isSeparableVerb: true`, `separablePrefix: "an"`.

## 6. Fiil Çekimi (conjugationData)
6 kişi × 3 zaman = **18 hücrenin tamamı** dolu olmalı (bkz. §10 — gerçek veride hiç boş kalmıyor):
```json
{
  "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
  "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
  "perfect":    { "ich":"bin gegangen", "du":"bist gegangen", "erSieEs":"ist gegangen",
                  "wir":"sind gegangen", "ihr":"seid gegangen", "sie":"sind gegangen" },
  "pastParticiple": "gegangen",
  "auxiliaryVerb":  "sein"
}
```

## 7. Bileşik Kelimeler
Cinsiyet **son kelimeden**: der Apfel + der Baum → der Apfelbaum · das Haus + die Tür → die Haustür.
Bileşik olduğunu belirten ayrı bir alan yok; açıklama **mevcut `WordDetails.Notes`** alanına yazılır
(ör. "Anruf + Beantworter" — der Anrufbeantworter), yeni bir GrammarData anahtarı **açılmaz** (YAGNI).

## 8. Kart Tasarımı
- **İsim:** artikel + kelime (cinsiyet rengi) + 4 hâl tablosu + çoğul + Türkçe + örnek (seviyeye göre) + kategoriler + ses/IPA.
- **Fiil:** mastar + ayrılabilir göstergesi + çekim tablosu (present/preterite/perfect) + Türkçe + örnek.

## 9. Sınav Türleri
Artikel quiz (der/die/das) · Çoğul quiz · Hâl quiz ("Ich helfe ___ Mann" → dem) · Ayrılabilir ön ek · Çeviri/dikte.
Unicode: ö ü ä ß Ä Ö Ü. Örnekler `WordExamples.Level`'e göre filtrelenir.

## 10. Tür Bazlı Alan Doldurma Kuralı (GrammarData Doldurma Matrisi)
> 795 kelimelik gerçek içerik ölçümünden çıkarılan desen — hem tekil kelime formunda (B-03
> `WordFormModal`) hem toplu içe aktarımda (A-07) **aynı koşullu kural** uygulanır: kaynak
> `WordConcepts.PartOfSpeech`. Bu bölüm `WordValidator`/`WordFormModal`'in tek doğruluk kaynağıdır —
> kod bu kuralları tekrar tanımlamaz, buraya referans verir.
>
> **Kapsam: yalnızca `de` (Almanca).** Bu matris Almancaya özgüdür (gender/artikel/hâl/ayrılabilir
> fiil Almancaya has kavramlar) — başka dile **kopyalanamaz**. `tr` için eşdeğer matris
> `TURKISH_LANGUAGE_FEATURES.md §9`'da tanımlı (kendi grameri: ünlü uyumu, 6 hâl, iyelik eki, ünsüz
> yumuşaması — bileşik kelime notu kuralı bile farklı, orada ayrıca not edildi). `en` şu an içerik
> girilmiyor (`Icerik.md`: "tanımlı ama henüz kullanılmıyor"), matrisi gerektiğinde yazılır.
> Validator/form dile göre dispatch eder — bkz. "Uygulama noktaları".
>
> **`Definition` ("Anlamı") dili sabit değil.** Gerçek 795 satırlık veride bu kolon **Türkçe** kısa
> gloss içeriyor (ör. `aber` → "ama, fakat, ancak") — yani "kelimenin kendi dilinde tanımı" değil,
> serbest bir anlam notu. Kartta kullanıcıya gösterilen "resmi çeviri" buradan **gelmez** — eşleşen
> `tr` `Word.Text`'inden gelir. Bu alanın asıl işlevi: ayrı girilen içerikte eşleştirme önerisi
> üretmek (`Icerik.md` "Eşleştirme" → `suggestedMatchConceptId`).

**PartOfSpeech = Noun (İsim)**
- **Zorunlu:** `Text` (kelime), `Definition` (serbest anlam notu — bkz. yukarı), `plural`,
  `gender` (Maskulin|Feminin|Neutrum — tek değer; kaynak Excel'deki "3 kolondan biri dolu" görünümü
  tek `gender` alanına karşılık gelir), 4 hâl (`cases.nominative/accusative/dative/genitive`).
- **Koşullu:** `WordDetails.Notes` (bileşik kelime notu) — yalnızca bileşik isimde dolu, değilse boş/NULL.
- **Yasak (NULL kalmalı):** fiil alanları (`isSeparableVerb`, `separablePrefix`, `auxiliaryVerb`,
  `pastParticiple`, `conjugation.*`) — Noun'da GrammarData'da bulunmamalı.

**PartOfSpeech = Verb (Fiil)**
- **Zorunlu:** `Text`, `Definition`, `isSeparableVerb`, `auxiliaryVerb`, `pastParticiple`,
  `conjugation.present/preterite/perfect` (§6 — 18 hücrenin tamamı).
- **Koşullu:** `separablePrefix` — yalnızca `isSeparableVerb=true` iken zorunlu/dolu, `false` iken
  boş/NULL olmalı (validator: `isSeparableVerb=true` ⇒ `separablePrefix` NOT NULL, tersi de geçerli).
- **Yasak:** `gender`, `plural`, `cases.*` — Verb'de anlamsız, GrammarData'da bulunmamalı.

**Diğer türler (Adjective, Adverb, Conjunction, Preposition, Pronoun, Other)**
- **Zorunlu:** yalnızca `Text`, `Definition`.
- **GrammarData:** tamamen `NULL` — ne isim alanları ne fiil alanları yazılır.

**Tüm türlerde ortak**
- `WordExamples.SentenceText` **her zaman zorunlu**, tür fark etmez (DB'de zaten `NOT NULL`).
  **"Örnek Cümle Anlamı" otomatik bir çeviri değildir** — yalnızca `PairedExampleId` ile karşı dilin
  bir örneğine **elle** bağlanmışsa (birlikte girildiyse veya eşleştirmede admin bağladıysa) kartta
  çeviri olarak gösterilir; bağlı değilse örnek **kendi başına** (o kelimeyi kullanan bağımsız bir
  cümle olarak) gösterilir (bkz. `Icerik.md` `WordExamples.PairedExampleId`).
- `WordCategories` (Kategori 1/2) her zaman **opsiyonel**, 0-2 arası; soyut türlerde (Conjunction,
  Pronoun vb.) çoğu zaman boş kalması beklenen/normal bir durumdur, validator bunu zorunlu kılmaz.

**Uygulama noktaları**
- **Backend (A-05):** `WordGrammarValidator` (FluentValidation) önce `Words.LanguageId`/`Languages.Code`'a
  göre dile dispatch eder, sonra o dilin kendi `PartOfSpeech` matrisini uygular — `de` dalı bu
  bölümdeki kural, `tr` dalı `TURKISH_LANGUAGE_FEATURES.md §9`. `en` henüz içerik girilmediği için
  dal yok (gerektiğinde eklenir). Hem tekil `CreateWordCommand`/`UpdateWordCommand` hem A-07 toplu
  import **aynı validator'ı** kullanır.
- **Frontend (B-03):** `WordFormModal` — önce dil (`de`/`tr`), sonra `Tür` seçilir; gramer bölümü bu
  ikisine göre koşullu render edilir. `de` + Noun/Verb/Diğer için bu bölümdeki gruplar, `tr` + aynı
  türler için `TURKISH_LANGUAGE_FEATURES.md §9` (RHF conditional schema, backend kuralının TS
  karşılığı — iki taraf ayrı ayrı ama aynı mantıkla yazılır, kod paylaşımı yok).
