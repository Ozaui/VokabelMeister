# TÜRKÇE DİL ÖZELLİKLERİ

> Türkçe (`tr`) `WordDetail.GrammarData` JSON şeması. Çoklu dil yapısı → `CLAUDE.md §1`.
> **Öncelik:** gerçek içerik şu an tr için yazılıyor (Almanın Türkçe öğrenmesi senaryosu). DB CHECK yok, doğrulama uygulama katmanında.

## 1. Ünlü Uyumu (Vowel Harmony)

Ekler kelimenin **son ünlüsüne** göre şekil değiştirir (agglutinatif).
Büyük uyum: `a,ı,o,u`→kalın; `e,i,ö,ü`→ince. Küçük uyum: `a/e`→`ı/i/u/ü`.

| Son ünlü | Grup | Çoğul `-lAr` |
|----------|------|--------------|
| a, ı | Kalın | masa**lar**, kız**lar** |
| e, i | İnce | ev**ler**, ip**ler** |
| o, u | Kalın (yuvarlak) | kol**lar**, kutu**lar** |
| ö, ü | İnce (yuvarlak) | göz**ler**, süt**ler** |

`GrammarData.vowelHarmony`: `"kalın"` | `"ince"`.

## 2. Çoğul
Tek ek `-lar`/`-ler` (ünlü uyumu). Düzensiz yok; alıntı istisnalar `GrammarData.pluralForm`'da override.

## 3. Hâl Ekleri (6 Hâl)

| Hâl | Ek | Soru | Örnek (masa) |
|-----|-----|------|--------------|
| **Yalın** (Nom) | — | Ne? | masa |
| **Belirtme** (Acc) | -(y)ı/i/u/ü | Neyi? | masa**yı** |
| **Yönelme** (Dat) | -(y)a/e | Nereye? | masa**ya** |
| **Bulunma** (Loc) | -da/de/ta/te | Nerede? | masa**da** |
| **Ayrılma** (Abl) | -dan/den/tan/ten | Nereden? | masa**dan** |
| **Tamlayan** (Gen) | -(n)ın/in/un/ün | Kimin? | masa**nın** |

Kaynaştırma harfleri (y/n/s/ş): `araba`+`-ı` → araba**y**ı. Ünsüz sertleşmesi: sert ünsüz (`p,ç,t,k,f,h,s,ş`) sonrası `-da/de`→`-ta/te` (kitap**ta**).
```json
"cases": { "nominative":"masa", "accusative":"masayı", "dative":"masaya",
           "locative":"masada", "ablative":"masadan", "genitive":"masanın" }
```

## 4. İyelik Ekleri
```json
"possessive": { "ben":"masam", "sen":"masan", "o":"masası",
                "biz":"masamız", "siz":"masanız", "onlar":"masaları" }
```
Ekler: ben -(i)m · sen -(i)n · o -(s)i · biz -(i)mIz · siz -(i)nIz · onlar -(s)i/-lArI.

## 5. Ünsüz Yumuşaması
Sert ünsüz (`p,ç,t,k`) + ünlüyle başlayan ek → `p→b, ç→c, t→d, k→ğ`: kitap→kitab**ı**, ağaç→ağac**ı**, kanat→kanad**ı**. (İstisna: bazı tek heceliler yumuşamaz, at→at**ı**.)
`GrammarData.consonantMutation`: `{ "hasChange": true, "pattern": "p→b", "example": "kitap→kitabı" }`.

## 6. Fiil Çekimi (conjugationData)
5 zaman × 6 kişi = **30 hücrenin tamamı** dolu olmalı (bkz. §9 — gerçek veride hiç boş kalmıyor).
Kökten (`verbRoot`) türetilir, olumsuzu ise ayrı tek bir alanda (`negativeForm`) tutulur — kişi
başına ayrı olumsuz çekim kolonu **yok** (30 kolonun dışında, spreadsheet'te de tek kolon):
```json
{
  "verbRoot": "gel",
  "negativeForm": "gelmemek",
  "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" },
  "aorist":            { "ben":"gelirim", "sen":"gelirsin", "o":"gelir", "biz":"geliriz", "siz":"gelirsiniz", "onlar":"gelirler" },
  "pastDefinite":      { "ben":"geldim", "sen":"geldin", "o":"geldi", "biz":"geldik", "siz":"geldiniz", "onlar":"geldiler" },
  "pastNarrative":     { "ben":"gelmişim", "sen":"gelmişsin", "o":"gelmiş", "biz":"gelmişiz", "siz":"gelmişsiniz", "onlar":"gelmişler" },
  "future":            { "ben":"geleceğim", "sen":"geleceksin", "o":"gelecek", "biz":"geleceğiz", "siz":"geleceksiniz", "onlar":"gelecekler" }
}
```
Olumsuz `-me/-ma` kişi ekinden önce (gel**me**yeceğim) — `negativeForm` yalnızca mastarın olumsuz
biçimini tutar, her zamanın olumsuzu kurala göre türetilir (ayrı 30 kolon daha açılmaz — YAGNI).

## 7. Kart Tasarımı
- **İsim:** kelime + ünlü uyumu grubu + çoğul + 6 hâl tablosu + iyelik + Almanca + örnek + kategoriler + ses/IPA.
- **Fiil:** mastar (`-mek/-mak`) + ünsüz yumuşaması göstergesi + çekim (şimdiki/geniş/geçmiş/gelecek) + Almanca + örnek.

## 8. Sınav Türleri
Hâl eki quiz ("Ben ___ gidiyorum" okul→okul**a**) · Çoğul quiz · İyelik quiz · Ünsüz yumuşaması quiz · Çeviri/dikte.
Unicode: ç ğ ı ö ş ü İ (`ı`≠`i` dikkat). Örnekler `WordExamples.Level`'e göre filtrelenir.

## 9. Tür Bazlı Alan Doldurma Kuralı (GrammarData Doldurma Matrisi)
> `GERMAN_LANGUAGE_FEATURES.md §10`'un birebir `tr` karşılığı — gerçek içerikten ölçülerek çıkarıldı.
> **Kapsam: yalnızca `tr` (Türkçe).** Almanca matrisiyle **karıştırılmaz** — iki dilin grameri
> temelden farklı (Türkçede cinsiyet/artikel yok, 6 hâl var; Almancada gender/artikel var, 4 hâl var).
> `WordGrammarValidator`'ın `tr` dalı — bkz. `GERMAN_LANGUAGE_FEATURES.md §10` "Uygulama noktaları".
>
> **`Definition` ("Anlamı") dili sabit değil** — `GERMAN_LANGUAGE_FEATURES.md §10`'daki aynı not
> burada da geçerli: serbest bir anlam notu (pratikte genelde karşı dilde kısa gloss), kartta
> gösterilen resmi çeviri değil; birincil işlevi eşleştirme önerisi üretmek.
>
> **Doğrulanmadı uyarısı:** `verbRoot`/`negativeForm` alanları henüz gerçek Türkçe içerikle
> ölçülmedi (Almanca matrisi 795 satır üzerinden doğrulandı, Türkçe için elimizde henüz veri yok) —
> gerçek `tr` kelime listesi dolmaya başlayınca bu ikisi gözden geçirilecek. Hâl/çekim
> sayıları (6 hâl, 30 çekim hücresi) Türkçe dilbilgisinin kendisinden geldiği için bu belirsizliğe dahil değil.

**PartOfSpeech = Noun (İsim)**
- **Zorunlu:** `Text`, `Definition` (serbest anlam notu), `plural`, 6 hâl (`cases.nominative/accusative/dative/locative/ablative/genitive`, §3).
- **Koşullu:** `WordDetails.Notes` (bileşik kelime notu) — yalnızca bileşik isimde dolu (ör. "başöğretmen"), değilse boş/NULL.
- **Yasak (NULL kalmalı):** `verbRoot`, `negativeForm`, `conjugation.*` (30 hücre) — Noun'da anlamsız.

**PartOfSpeech = Verb (Fiil)**
- **Zorunlu:** `Text`, `Definition` (serbest anlam notu), `verbRoot`, `negativeForm`, `conjugation.presentContinuous/aorist/pastDefinite/pastNarrative/future` (§6 — 30 hücrenin tamamı).
- **Koşullu:** `WordDetails.Notes` (bileşik kelime notu) — yalnızca bileşik fiilde dolu (ör. "göz atmak"), değilse boş/NULL.
  **Fark (Almancadan):** `de`'de bileşik not yalnızca Noun'da; `tr`'de hem Noun hem Verb'de olabilir —
  validator bu ayrımı dile göre yapar, ortak bir kural değildir.
- **Yasak:** `plural`, `cases.*` — Verb'de anlamsız.

**Diğer türler (Adjective, Adverb, Conjunction, Preposition, Pronoun, Other — Sayı/Ünlem gibi ayrı
enum değeri olmayanlar `PartOfSpeech=Other`'a düşer, `WordConcepts.PartOfSpeech` CHECK listesi sabit)**
- **Zorunlu:** yalnızca `Text`, `Definition`.
- **GrammarData:** tamamen `NULL`.

**Tüm türlerde ortak**
- `WordExamples.SentenceText` her zaman zorunlu (DB'de zaten `NOT NULL`). Almancadaki "Örnek Cümle
  Anlamı" ayrı kolonu `tr` tarafında **yok** — ama bu, karşı dilin örneğinin otomatik çeviri
  sayılacağı anlamına **gelmez**: yalnızca `PairedExampleId` ile elle bağlanmışsa çeviri olarak
  gösterilir, bağlı değilse bağımsız bir örnektir (bkz. `Icerik.md` `WordExamples.PairedExampleId`).
- `WordCategories` (Kategori 1/2) her zaman opsiyonel, 0-2 arası; soyut türlerde çoğu satırda boş.

**Uygulama noktaları:** `A-05` `WordGrammarValidator`'ın `tr` dalı ve `B-03` `WordFormModal`'ın
`tr` seçimi bu matrisi kullanır — ayrıntı ve dil dispatch mekanizması `GERMAN_LANGUAGE_FEATURES.md §10`'da.
