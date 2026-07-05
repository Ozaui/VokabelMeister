# İNGİLİZCE DİL ÖZELLİKLERİ

> **Şu an kullanılmıyor.** `Languages` tablosunda henüz `en` satırı yok, hiçbir `Words` satırı bu dile
> ait değil. Bu dosya yalnızca **şema hazırlığı** için yazıldı — İngilizce gerçekten eklendiğinde
> (`Languages`'e `INSERT` + ilgili kavramlara `en` `Words` satırı) `WordDetail.GrammarData` bu şekli
> kullanacak. Öncelik sırası: önce Türkçe (`TURKISH_LANGUAGE_FEATURES.md`), İngilizce sonra.

Kelime kartı ve sınav türleri yazılırken referans. Bu dosya, `WordDetail.GrammarData` JSON alanının
**yalnızca İngilizce (`en`) diline özel** şemasını tanımlar — şema çoklu dile açık tasarlandı
(`DATABASE_SCHEMA/Icerik.md` → `Languages`/`WordConcept`).

## 1. Cinsiyet Yok, Artikel Basit

Almanca/Türkçe'nin aksine gramer cinsiyeti yok. Artikel: `a`/`an` (belirsiz — bir sonraki kelimenin
sesi ünlüyse `an`: *an apple*, ünsüzse `a`: *a table*) ve `the` (belirli, tek biçim).
`WordDetail.GrammarData.article`: `"a"` | `"an"` | `"the"` | `null` (sayılamayan isimlerde artikel yok).

## 2. Çoğul (Plural)

| Kalıp | Örnek |
|-------|-------|
| -s (düzenli, çoğunluk) | table → tables · book → books |
| -es (s/x/z/ch/sh ile bitenler) | box → boxes · watch → watches |
| -y → -ies (ünsüz+y ile bitenler) | city → cities · baby → babies |
| Düzensiz | child → children · man → men · mouse → mice · foot → feet |
| Değişim yok | sheep → sheep · fish → fish |

`WordDetail.GrammarData`: `"pluralForm": "children"`, `"isIrregularPlural": true`.

## 3. Fiil Çekimi (GrammarData.verbForms)

Almanca/Türkçe'deki gibi kişiye göre zengin çekim yok — yalnızca 3. tekil şahısta `-s/-es` eki ve
düzensiz fiillerde ayrı geçmiş/participle formu var.

```json
{
  "base": "go", "thirdPersonSingular": "goes",
  "pastSimple": "went", "pastParticiple": "gone", "presentParticiple": "going",
  "isIrregular": true
}
```
Düzenli fiillerde `pastSimple`/`pastParticiple` = `base + "ed"` (örn. `walk → walked → walked`),
üretilmez, yine de alan doldurulur (tutarlı sorgu için).

## 4. Sıfatlarda Karşılaştırma (Comparative/Superlative)

| Kalıp | Örnek |
|-------|-------|
| -er / -est (kısa sıfat) | big → bigger → biggest |
| more / most (uzun sıfat) | beautiful → more beautiful → most beautiful |
| Düzensiz | good → better → best · bad → worse → worst |

`WordDetail.GrammarData`: `"comparative": "bigger", "superlative": "biggest"`.

## 5. Kelime Kartı Tasarımı

**İsim kartı:** artikel (`a`/`an`/`the`/yok) + kelime + çoğul + Almanca/Türkçe + örnek cümle + seviye
+ kategoriler + ses/IPA.

**Fiil kartı:** mastar + düzensiz göstergesi + `base/pastSimple/pastParticiple` tablosu + örnek.

## 6. İngilizceye Özgü Sınav Türleri

1. **Çoğul quiz:** child → ___ (children) ✓
2. **Düzensiz fiil quiz:** go → ___ (past simple: went) ✓
3. **Karşılaştırma quiz:** good → ___ (better) ✓
4. **Artikel quiz:** "___ apple" → an ✓
5. **Çeviri / dikte:** İngilizce cümle → Almanca/Türkçe veya yazım kontrolü.

## 7. Geliştirme Notları

- `WordDetail.GrammarData` (JSON) uygulama katmanında validasyondan geçer (DB-seviyesi `CHECK` yok,
  bkz. `DATABASE_SCHEMA/Icerik.md` trade-off notu).
- Düzensiz fiil/çoğul listesi genişçe olduğundan (yüzlerce kelime), içerik girişinde bir referans
  tablo/CSV'den toplu import düşünülebilir — bu, İngilizce gerçekten eklenmeden önce netleştirilecek
  bir A-05+ detayı, şimdi tasarlanmıyor (YAGNI).
