# İngilizce Dil Özellikleri (WordDetail.GrammarData referansı — henüz kullanılmıyor)

**Özet:** [[Icerik_Domain]]'deki `WordDetail.GrammarData` JSON alanının **yalnızca İngilizce (`en`)
diline özel** şemasının kaynağı. **Şu an kullanılmıyor** — `Languages` tablosunda `en` satırı yok,
hiçbir `Words` satırı bu dile ait değil; yalnızca şema hazırlığı için yazıldı (öncelik sırası: önce
Türkçe → [[Turkce_Dili_Ozellikleri]]). İngilizce gerçekten eklendiğinde bu şekil `Languages`'e bir
satır + ilgili kavramlara `en` `Words` satırlarıyla devreye girer, migration gerekmez. Kaynak:
`docs/REFERENCE/ENGLISH_LANGUAGE_FEATURES.md`.
**Kütüphaneler:** —
**Bağlantılar:** [[Icerik_Domain]] · [[Turkce_Dili_Ozellikleri]] · [[Alman_Dili_Ozellikleri]]

## Farklar (Almanca/Türkçe'ye kıyasla)
Gramer cinsiyeti yok · artikel basit (`a`/`an`/`the`) · hâl eki/iyelik eki sistemi yok · fiil çekimi
zayıf (yalnızca 3. tekil şahısta `-s/-es`, düzensiz fiillerde ayrı geçmiş/participle formu).

## Çoğul
Düzenli `-s`/`-es` (box→boxes), `-y→-ies` (city→cities), düzensiz (child→children, man→men,
mouse→mice), değişim yok (sheep→sheep). `GrammarData`: `pluralForm`, `isIrregularPlural`.

## Fiil Çekimi (GrammarData.verbForms)
`base`/`thirdPersonSingular`/`pastSimple`/`pastParticiple`/`presentParticiple`/`isIrregular` — düzenli
fiillerde `pastSimple`/`pastParticiple` = `base+"ed"`, yine de tutarlılık için alan doldurulur.

## Sıfat Karşılaştırma
`-er`/`-est` (kısa sıfat) · `more`/`most` (uzun sıfat) · düzensiz (good→better→best).
`GrammarData`: `comparative`, `superlative`.

## İngilizceye Özgü Sınav Türleri
Çoğul quiz · Düzensiz fiil quiz · Karşılaştırma quiz · Artikel quiz (`a`/`an`) · Çeviri/dikte.

## Geliştirme Notları
Düzensiz fiil/çoğul listesi geniş (yüzlerce kelime) — toplu import ihtiyacı İngilizce gerçekten
eklenmeden önce netleştirilecek bir A-05+ detayı, şimdi tasarlanmadı (YAGNI).
