# Türkçe Dil Özellikleri (WordDetail.GrammarData referansı)

**Özet:** [[Icerik_Domain]]'deki `WordDetail.GrammarData` JSON alanının **yalnızca Türkçe (`tr`)
diline özel** şemasının kaynağı — ünlü uyumu, 6 hâl eki, çoğul, iyelik ekleri, ünsüz yumuşaması ve fiil
çekimi burada tanımlanır. Almanca'nın karşılığı → [[Alman_Dili_Ozellikleri]]. **Öncelik:** şu an gerçek
içerik Türkçe için yazılıyor (bir Almanın Türkçe öğrenmesi senaryosu dâhil, yön fark etmeksizin);
İngilizce şeması da tanımlı ama henüz kullanılmıyor → [[Ingilizce_Dili_Ozellikleri]]. Kaynak:
`docs/REFERENCE/TURKISH_LANGUAGE_FEATURES.md`.
**Kütüphaneler:** —
**Bağlantılar:** [[Icerik_Domain]] · [[Alman_Dili_Ozellikleri]] · [[Kodlama_Standartlari]]

## Ünlü Uyumu (Vowel Harmony)
Ekler kökün son ünlüsüne göre şekil değiştirir (agglutinatif yapı): `a,ı,o,u` → kalın, `e,i,ö,ü` →
ince. `GrammarData.vowelHarmony`: `"kalın"`/`"ince"` — çoğul/hâl eki seçiminde kullanılır.

## Çoğul
Tek ek `-lar`/`-ler` (ünlü uyumuna göre): masa→masalar, ev→evler. Düzensiz çoğul yok (Almanca'nın
aksine); istisnalar `GrammarData.pluralForm`'da elle override edilir.

## Hâl Ekleri (6 Hâl)
Yalın (—) · Belirtme (-(y)ı/i/u/ü) · Yönelme (-(y)a/e) · Bulunma (-da/de/ta/te) · Ayrılma
(-dan/den/tan/ten) · Tamlayan (-(n)ın/in/un/ün). Kaynaştırma harfleri (y/n/s/ş) ünlüyle biten köklerde
araya girer; ünsüz sertleşmesi (`p,ç,t,k,f,h,s,ş` ile biten köklerde -da/-de → -ta/-te).
`GrammarData.cases`: `{nominative, accusative, dative, locative, ablative, genitive}`.

## İyelik Ekleri
ben(-im)/sen(-in)/o(-si)/biz(-imiz)/siz(-iniz)/onlar(-si) → `GrammarData.possessive`.

## Ünsüz Yumuşaması
Sert ünsüzle (`p,ç,t,k`) biten kökler ünlü ekle yumuşar: p→b, ç→c, t→d, k→ğ (kitap→kitabı).
`GrammarData.consonantMutation`: `{hasChange, pattern, example}`.

## Fiil Çekimi (GrammarData.conjugationData)
4 temel zaman: `presentContinuous` (-yor), `aorist` (geniş zaman, -r/-ar/-er), `pastDefinite` (-di),
`future` (-ecek/-acak) — her biri 6 kişi formu (ben/sen/o/biz/siz/onlar).

## Türkçeye Özgü Sınav Türleri (→ [[SRS_Domain]] `SessionType`)
Hâl eki quiz · Çoğul quiz (ünlü uyumu doğru mu) · İyelik quiz · Ünsüz yumuşaması quiz · Çeviri/dikte.

## Geliştirme Notları
Ek seçimi `vowelHarmony` alanıyla tutarlı olmalı (DB-seviyesi `CHECK` yok) · Unicode tam destek
(ç, ğ, ı, ö, ş, ü, noktalı/noktasız İ/I ayrımı) · örnek cümleler kullanıcı seviyesine göre filtrelenir.
