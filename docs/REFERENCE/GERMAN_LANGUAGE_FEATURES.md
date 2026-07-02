# ALMANCA DİL ÖZELLİKLERİ

Kelime kartı (`WordDetail`) ve sınav türleri yazılırken referans. Uygulama yalnızca Türkçe-Almanca destekler.

## 1. Cinsiyet (Gender) ve Renk Sistemi

| Cinsiyet | Belirli | Belirsiz | Çoğul | Kart Rengi |
|----------|---------|----------|-------|-----------|
| **Maskulin** (der) | der | ein | die | 🔵 Mavi |
| **Feminin** (die) | die | eine | die | 🔴 Kırmızı |
| **Neutrum** (das) | das | ein | die | 🟢 Yeşil |

Örnek: der Mann (erkek), die Frau (kadın), das Kind (çocuk), das Buch (kitap), der Hund (köpek).

## 2. Artikeller (4 Hâl)

**Belirli:**
```
         NOM   AKK   DAT   GEN
MASKULIN der   den   dem   des
FEMININ  die   die   der   der
NEUTRUM  das   das   dem   des
ÇOĞUL    die   die   den   der
```
**Belirsiz:**
```
         NOM   AKK    DAT    GEN
MASKULIN ein   einen  einem  eines
FEMININ  eine  eine   einer  einer
NEUTRUM  ein   ein    einem  eines
```
Olumsuz: kein/keine/kein/keine.

## 3. Dört Hâl (Cases)

| Hâl | Soru | Fonksiyon | Örnek |
|-----|------|-----------|-------|
| **Nominativ** | Wer? Was? | Özne | **Der Mann** geht. |
| **Akkusativ** | Wen? Was? | Düz nesne | Ich sehe **den Mann**. |
| **Dativ** | Wem? | Dolaylı nesne | Ich helfe **dem Mann**. |
| **Genitiv** | Wessen? | İyelik | Das Auto **des Mannes**. |

```
der Mann →  Nom: der Mann · Akk: den Mann · Dat: dem Mann · Gen: des Mannes
die Frau →  Nom: die Frau · Akk: die Frau · Dat: der Frau · Gen: der Frau
das Kind →  Nom: das Kind · Akk: das Kind · Dat: dem Kind · Gen: des Kindes
ÇOĞUL    →  Dativ'de ekstra -n: den Männern
```

## 4. Çoğul Kalıpları

| Kalıp | Örnek |
|-------|-------|
| -e (çoğu maskulin) | der Tisch → die Tische · der Hund → die Hunde |
| -n / -en (çoğu feminin) | die Frau → die Frauen · die Blume → die Blumen |
| -er (+umlaut) | das Kind → die Kinder · der Mann → die Männer |
| -s (yabancı kelimeler) | das Auto → die Autos · das Hotel → die Hotels |
| Değişim yok | das Fenster → die Fenster · das Mädchen → die Mädchen |
| Umlaut (a→ä, o→ö, u→ü) | der Apfel → die Äpfel · der Sohn → die Söhne |

## 5. Ayrılabilir Fiiller (Separable Verbs)

Ön ek ana fiilden ayrılır, cümle sonuna gider:
```
anrufen (aramak)    → Ich rufe dich an.       · Perfekt: Ich habe dich angerufen.
aufstehen (kalkmak) → Ich stehe um 6 Uhr auf. · Perfekt: Ich bin früh aufgestanden.
fernsehen (TV izle) → Ich sehe jeden Abend fern.
```
Yaygın ön ekler: ab-, an-, auf-, aus-, ein-, mit-, vor-, weg-, zu-, zurück-, zusammen-.
`WordDetail`: `IsSeparableVerb = true`, `SeparablePrefix = "an"`.

## 6. Fiil Çekimi (ConjugationData JSON)

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

Cinsiyet **son kelimeden** gelir: der Apfel + der Baum → der Apfelbaum · die Küche + der Tisch → der
Küchentisch · das Haus + die Tür → die Haustür.

## 8. Kelime Kartı Tasarımı

**İsim kartı:** artikel + kelime (cinsiyet rengi) + 4 hâl tablosu + çoğul + Türkçe + örnek cümle (kullanıcı
seviyesine göre) + seviye + kategoriler + ses/IPA.

**Fiil kartı:** mastar + ayrılabilir göstergesi + çekim tablosu (present/preterite/perfect) + Türkçe + örnek.

## 9. Almancaya Özgü Sınav Türleri

1. **Artikel quiz:** "der/die/das ___?" → der Mann ✓
2. **Çoğul quiz:** der Apfel → die Äpfel ✓
3. **Hâl quiz:** "Ich helfe ___ Mann" → dem ✓
4. **Ayrılabilir ön ek:** "Sie ___ um 6 Uhr ___" (aufstehen) → steht ... auf ✓
5. **Çeviri / dikte:** Almanca cümle → Türkçe veya yazım kontrolü.

## 10. Geliştirme Notları

- WordDetail alanları validasyondan geçer; artikeller cinsiyetle tutarlı olmalı.
- Cinsiyet renkle gösterilir (mavi/kırmızı/yeşil), artikel başlıkta belirgin.
- Unicode tam destek: ö, ü, ä, ß, Ä, Ö, Ü. Çoğullarda umlaut'a dikkat.
- Örnek cümleler kullanıcı seviyesine göre filtrelenir (`WordExamples.Level`).
