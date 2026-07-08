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
```json
{
  "presentContinuous": { "ben":"geliyorum", "sen":"geliyorsun", "o":"geliyor", "biz":"geliyoruz", "siz":"geliyorsunuz", "onlar":"geliyorlar" },
  "aorist":            { "ben":"gelirim", "sen":"gelirsin", "o":"gelir", "biz":"geliriz", "siz":"gelirsiniz", "onlar":"gelirler" },
  "pastDefinite":      { "ben":"geldim", "sen":"geldin", "o":"geldi", "biz":"geldik", "siz":"geldiniz", "onlar":"geldiler" },
  "future":            { "ben":"geleceğim", "sen":"geleceksin", "o":"gelecek", "biz":"geleceğiz", "siz":"geleceksiniz", "onlar":"gelecekler" }
}
```
Olumsuz `-me/-ma` kişi ekinden önce (gel**me**yeceğim).

## 7. Kart Tasarımı
- **İsim:** kelime + ünlü uyumu grubu + çoğul + 6 hâl tablosu + iyelik + Almanca + örnek + kategoriler + ses/IPA.
- **Fiil:** mastar (`-mek/-mak`) + ünsüz yumuşaması göstergesi + çekim (şimdiki/geniş/geçmiş/gelecek) + Almanca + örnek.

## 8. Sınav Türleri
Hâl eki quiz ("Ben ___ gidiyorum" okul→okul**a**) · Çoğul quiz · İyelik quiz · Ünsüz yumuşaması quiz · Çeviri/dikte.
Unicode: ç ğ ı ö ş ü İ (`ı`≠`i` dikkat). Örnekler `WordExamples.Level`'e göre filtrelenir.
