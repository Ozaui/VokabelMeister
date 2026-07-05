# Almanca Dil Özellikleri (WordDetail referansı)

**Özet:** [[Icerik_Domain]]'deki `WordDetail.GrammarData` JSON alanının **yalnızca Almanca (`de`)
diline özel** şemasının kaynağı — cinsiyet/renk sistemi, 4 hâl artikelleri, çoğul kalıpları, ayrılabilir
fiiller ve fiil çekim JSON'u burada tanımlanır. Şema çoklu dile açık tasarlandı (`Languages`/
`WordConcept`); şu an yalnızca Almanca-Türkçe içerik yazılıyor. İleride eklenecek her dilin (örn.
İngilizce) kendi `GrammarData` şekli olacak — buradaki alanlar yalnızca Almanca `Words` satırlarının
`GrammarData`'sında bulunur, ayrı bir flat kolon/entity alanı değil.
**Kütüphaneler:** —
**Bağlantılar:** [[Icerik_Domain]] · [[Turkce_Dili_Ozellikleri]] · [[Ingilizce_Dili_Ozellikleri]] · [[Kodlama_Standartlari]]

## Cinsiyet ve Renk Sistemi
| Cinsiyet | Belirli | Belirsiz | Çoğul | Kart Rengi |
|----------|---------|----------|-------|-----------|
| Maskulin (der) | der | ein | die | 🔵 Mavi |
| Feminin (die) | die | eine | die | 🔴 Kırmızı |
| Neutrum (das) | das | ein | die | 🟢 Yeşil |

## Artikeller (4 Hâl)
```
Belirli:    NOM  AKK  DAT  GEN         Belirsiz:  NOM   AKK    DAT    GEN
MASKULIN    der  den  dem  des         MASKULIN   ein   einen  einem  eines
FEMININ     die  die  der  der         FEMININ    eine  eine   einer  einer
NEUTRUM     das  das  dem  des         NEUTRUM    ein   ein    einem  eines
ÇOĞUL       die  die  den  der
```
Bu 8+4 alan → `WordDetail.GrammarData.articleDefinite{Nom,Acc,Dat,Gen}` /
`articleIndefinite{Nom,Acc,Dat,Gen}` (JSON alanları, flat kolon değil — bkz. [[Icerik_Domain]]).

## Dört Hâl (Cases)
Nominativ (Wer?/Özne) · Akkusativ (Wen?/Düz nesne) · Dativ (Wem?/Dolaylı nesne, çoğulda ekstra `-n`)
· Genitiv (Wessen?/İyelik).

## Çoğul Kalıpları
`-e` (der Tisch→die Tische) · `-n/-en` (die Frau→die Frauen) · `-er +umlaut` (das Kind→die Kinder)
· `-s` (yabancı: das Auto→die Autos) · değişim yok (das Fenster→die Fenster) · umlaut (der Apfel→die Äpfel).

## Ayrılabilir Fiiller
Ön ek cümle sonuna gider: `anrufen → Ich rufe dich an.` (Perfekt: `Ich habe dich angerufen.`)
`WordDetail.GrammarData.isSeparableVerb=true`, `separablePrefix="an"`.

## ConjugationData JSON Şeması
```json
{
  "present": { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
  "preterite": { ... },
  "perfect": { "ich":"bin gegangen", ... },
  "pastParticiple": "gegangen",
  "auxiliaryVerb": "sein"
}
```

## Bileşik Kelimeler
Cinsiyet **son kelimeden** gelir: der Apfel + der Baum → der Apfelbaum.

## Almancaya Özgü Sınav Türleri (→ [[SRS_Domain]] `SessionType`)
Artikel quiz · Çoğul quiz · Hâl quiz · Ayrılabilir ön ek quiz · Çeviri/dikte.

## Geliştirme Notları
Artikeller cinsiyetle tutarlı olmalı (validasyon) · cinsiyet renkle gösterilir, artikel başlıkta
belirgin · Unicode tam destek (ö, ü, ä, ß) · örnek cümleler kullanıcı seviyesine göre filtrelenir.
