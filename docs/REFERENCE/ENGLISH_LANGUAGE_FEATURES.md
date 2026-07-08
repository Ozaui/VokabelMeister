# İNGİLİZCE DİL ÖZELLİKLERİ

> **Şu an KULLANILMIYOR** — `Languages`'te `en` satırı yok, hiçbir `Words` bu dile ait değil. Bu dosya yalnızca
> şema hazırlığı; İngilizce gerçekten eklenince (`Languages`'e INSERT + kavramlara `en` `Words`) bu şekil kullanılacak.
> Öncelik: önce Türkçe. `WordDetail.GrammarData` (`en`) şeması:

## 1. Artikel (cinsiyet yok)
`a`/`an` (sonraki kelime ünlü sesle başlıyorsa `an`: an apple) · `the` (tek biçim). Sayılamayanlarda artikel yok.
`GrammarData.article`: `"a"` | `"an"` | `"the"` | `null`.

## 2. Çoğul
| Kalıp | Örnek |
|-------|-------|
| -s (düzenli) | table → tables |
| -es (s/x/z/ch/sh) | box → boxes |
| ünsüz+y → -ies | city → cities |
| Düzensiz | child → children · man → men · foot → feet |
| Değişim yok | sheep → sheep · fish → fish |

`GrammarData`: `"pluralForm": "children"`, `"isIrregularPlural": true`.

## 3. Fiil Çekimi (verbForms)
```json
{ "base":"go", "thirdPersonSingular":"goes",
  "pastSimple":"went", "pastParticiple":"gone", "presentParticiple":"going", "isIrregular":true }
```
Düzenlilerde past = `base+"ed"` (walk→walked), yine de tutarlı sorgu için alan doldurulur.

## 4. Karşılaştırma
| Kalıp | Örnek |
|-------|-------|
| -er/-est (kısa) | big → bigger → biggest |
| more/most (uzun) | beautiful → more/most beautiful |
| Düzensiz | good → better → best · bad → worse → worst |

`GrammarData`: `"comparative":"bigger", "superlative":"biggest"`.

## 5. Kart & Sınav
İsim: artikel + kelime + çoğul + çeviri + örnek. Fiil: mastar + düzensiz göstergesi + base/pastSimple/pastParticiple.
Sınavlar: çoğul, düzensiz fiil, karşılaştırma, artikel, çeviri/dikte.
> Düzensiz liste geniş → içerik girişinde toplu import (CSV) düşünülebilir; A-05+ detayı, şimdi tasarlanmıyor (YAGNI).
