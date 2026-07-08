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
```json
{
  "present":    { "ich":"gehe", "du":"gehst", "erSieEs":"geht", "wir":"gehen", "ihr":"geht", "sie":"gehen" },
  "preterite":  { "ich":"ging", "du":"gingst", "erSieEs":"ging", "wir":"gingen", "ihr":"gingt", "sie":"gingen" },
  "perfect":    { "ich":"bin gegangen", "erSieEs":"ist gegangen", "sie":"sind gegangen" },
  "pastParticiple": "gegangen",
  "auxiliaryVerb":  "sein"
}
```

## 7. Bileşik Kelimeler
Cinsiyet **son kelimeden**: der Apfel + der Baum → der Apfelbaum · das Haus + die Tür → die Haustür.

## 8. Kart Tasarımı
- **İsim:** artikel + kelime (cinsiyet rengi) + 4 hâl tablosu + çoğul + Türkçe + örnek (seviyeye göre) + kategoriler + ses/IPA.
- **Fiil:** mastar + ayrılabilir göstergesi + çekim tablosu (present/preterite/perfect) + Türkçe + örnek.

## 9. Sınav Türleri
Artikel quiz (der/die/das) · Çoğul quiz · Hâl quiz ("Ich helfe ___ Mann" → dem) · Ayrılabilir ön ek · Çeviri/dikte.
Unicode: ö ü ä ß Ä Ö Ü. Örnekler `WordExamples.Level`'e göre filtrelenir.
