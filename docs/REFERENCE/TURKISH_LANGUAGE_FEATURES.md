# TÜRKÇE DİL ÖZELLİKLERİ

Kelime kartı ve sınav türleri yazılırken referans. Bu dosya, `WordDetail.GrammarData` JSON alanının
**yalnızca Türkçe (`tr`) diline özel** şemasını tanımlar — şema çoklu dile açık tasarlandı
(`DATABASE_SCHEMA/Icerik.md` → `Languages`/`WordConcept`). Almanca'nın karşılığı →
`GERMAN_LANGUAGE_FEATURES.md`. **Öncelik:** şu an gerçek içerik Türkçe için yazılıyor (bir Almanın
Türkçe öğrenmesi senaryosu için) — İngilizce şeması da tanımlı (`ENGLISH_LANGUAGE_FEATURES.md`) ama
henüz kullanılmıyor (`Languages` tablosunda `en` satırı yok).

## 1. Ünlü Uyumu (Vowel Harmony)

Türkçe eklerin hangi ünlüyü alacağını belirleyen kural — ekler kelimenin **son ünlüsüne** göre şekil
değiştirir (agglutinatif/eklemeli yapı).

**Büyük ünlü uyumu** (kalın/ince): `a, ı, o, u` → kalın; `e, i, ö, ü` → ince.
**Küçük ünlü uyumu** (düz/yuvarlak): ek seçimi 4 varyanta çıkar (`a/e` → `ı/i/u/ü`).

| Kök son ünlüsü | Grup | Örnek ek varyantı (çoğul `-lAr`) |
|----------------|------|-----------------------------------|
| a, ı | Kalın | masa**lar**, kız**lar** |
| e, i | İnce | ev**ler**, ip**ler** |
| o, u | Kalın (yuvarlak) | kol**lar**, kutu**lar** |
| ö, ü | İnce (yuvarlak) | göz**ler**, süt**ler** |

`WordDetail.GrammarData.vowelHarmony`: `"kalın"` veya `"ince"` — ek üretiminde/doğrulamasında kullanılır.

## 2. Çoğul (Plural)

Tek ek: `-lar` (kalın) / `-ler` (ince) — ünlü uyumuna göre otomatik seçilir.
```
masa → masalar    ev → evler    kitap → kitaplar    göz → gözler
```
Düzensiz çoğul yoktur (Almanca'nın aksine); tek istisna bazı alıntı kelimelerde (örn. "hayvanat" gibi
Arapça kökenli çoğullar) — bunlar `GrammarData.pluralForm` alanında elle override edilir.

## 3. Hâl Ekleri (6 Hâl)

| Hâl | Ek (kalın/ince) | Soru | Örnek (masa) |
|-----|-----------------|------|--------------|
| **Yalın** (Nominative) | — | Kim? Ne? | masa |
| **Belirtme** (Accusative) | -(y)ı / -(y)i / -(y)u / -(y)ü | Kimi? Neyi? | masa**yı** |
| **Yönelme** (Dative) | -(y)a / -(y)e | Kime? Nereye? | masa**ya** |
| **Bulunma** (Locative) | -da / -de / -ta / -te | Kimde? Nerede? | masa**da** |
| **Ayrılma** (Ablative) | -dan / -den / -tan / -ten | Kimden? Nereden? | masa**dan** |
| **Tamlayan** (Genitive) | -(n)ın / -(n)in / -(n)un / -(n)ün | Kimin? | masa**nın** |

**Kaynaştırma harfleri** (buffer): kök ünlüyle bitiyorsa ek başına `y`/`n`/`s`/`ş` araya girer —
`araba` + `-ı` → araba**y**ı (Belirtme), `masa` + `-ın` → masa**n**ın (Tamlayan).
**Ünsüz sertleşmesi** (-da/-ta): kök sert ünsüzle (`p,ç,t,k,f,h,s,ş`) bitiyorsa `-da/-de` → `-ta/-te`
olur — `kitap` + `-da` → kitap**ta**.

```json
"cases": { "nominative": "masa", "accusative": "masayı", "dative": "masaya",
           "locative": "masada", "ablative": "masadan", "genitive": "masanın" }
```

## 4. İyelik Ekleri (Possessive Suffixes)

| Kişi | Ek | Örnek (masa) |
|------|-----|--------------|
| ben | -(i)m | masa**m** |
| sen | -(i)n | masa**n** |
| o | -(s)i | masa**sı** |
| biz | -(i)mız/-(i)miz | masa**mız** |
| siz | -(i)nız/-(i)niz | masa**nız** |
| onlar | -(s)i / -ları | masa**sı** (tekil sahiplik) |

```json
"possessive": { "ben":"masam", "sen":"masan", "o":"masası",
                "biz":"masamız", "siz":"masanız", "onlar":"masaları" }
```

## 5. Ünsüz Yumuşaması (Consonant Mutation)

Sert ünsüzle (`p, ç, t, k`) biten kökler, ünlüyle başlayan ek aldığında yumuşar: `p→b, ç→c, t→d, k→ğ`.
```
kitap → kitabı (Belirtme)    ağaç → ağacı    kanat → kanadı    kitap+lık → kitaplık (istisna: bazı tek heceli kelimeler yumuşamaz, örn. "at→atı")
```
`WordDetail.GrammarData.consonantMutation`: `{ "hasChange": true, "pattern": "p→b", "example": "kitap→kitabı" }`.

## 6. Fiil Çekimi (GrammarData.conjugationData)

Türkçe fiil çekimi de eklemeli: kip/zaman eki + kişi eki. En sık kullanılan 4 zaman:

```json
{
  "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" },
  "aorist":            { "ben":"gelirim", "sen":"gelirsin", "o":"gelir", "biz":"geliriz", "siz":"gelirsiniz", "onlar":"gelirler" },
  "pastDefinite":      { "ben":"geldim", "sen":"geldin", "o":"geldi", "biz":"geldik", "siz":"geldiniz", "onlar":"geldiler" },
  "future":            { "ben":"geleceğim", "sen":"geleceksin", "o":"gelecek", "biz":"geleceğiz", "siz":"geleceksiniz", "onlar":"gelecekler" }
}
```
Olumsuz ek `-me/-ma` her zamanda kişi ekinden önce gelir (örn. gel**me**yeceğim).

## 7. Kelime Kartı Tasarımı

**İsim kartı:** kelime + ünlü uyumu grubu + çoğul + 6 hâl tablosu + iyelik ekleri + Almanca + örnek
cümle (kullanıcı seviyesine göre) + seviye + kategoriler + ses/IPA.

**Fiil kartı:** mastar (`-mek/-mak`) + ünsüz yumuşaması göstergesi + çekim tablosu
(şimdiki/geniş/geçmiş/gelecek) + Almanca + örnek.

## 8. Türkçeye Özgü Sınav Türleri

1. **Hâl eki quiz:** "Ben ___ gidiyorum." (okul) → okul**a** ✓
2. **Çoğul quiz:** kitap → kitap**lar** ✓ (ünlü uyumu doğru mu)
3. **İyelik quiz:** "benim ___" (araba) → araba**m** ✓
4. **Ünsüz yumuşaması quiz:** kitap + Belirtme → kitab**ı** ✓
5. **Çeviri / dikte:** Türkçe cümle → Almanca veya yazım kontrolü.

## 9. Geliştirme Notları

- `WordDetail.GrammarData` (JSON) uygulama katmanında validasyondan geçer; ek seçimi
  `vowelHarmony` alanıyla tutarlı olmalı (DB-seviyesi `CHECK` yok, bkz. `DATABASE_SCHEMA/Icerik.md`
  trade-off notu).
- Unicode tam destek: ç, ğ, ı, ö, ş, ü, İ (noktalı/noktasız I ayrımına dikkat — `ı` ≠ `i`).
- Örnek cümleler kullanıcı seviyesine göre filtrelenir (`WordExamples.Level`).
- Bu şema önceliği: TR-DE çiftinde bir Almanın Türkçe öğrenmesi senaryosu — önceden yalnızca
  Türkçe→çeviri (`GrammarData=NULL`) varsayılmıştı, artık yön fark etmeksizin her iki taraf da
  gramer içeriğine sahip.
