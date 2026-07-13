# BACKEND_AKADEMI — Yeni Görev Ekleme Standardı

> Bu dosya, `docs/API_YOL_HARITASI/_TASLAK.html`'ın bu akademideki karşılığıdır: yeni bir
> API/görev eklerken **kod taramadan, sıfırdan tasarlamadan** doğrudan kopyalayıp
> doldurabileceğin bir şablon + kısa bir kural seti. Yeni bir konuşmada/oturumda bu dosyayı
> okumak, tüm `engine/` mimarisini yeniden keşfetmekten daha hızlı olmalı.

## 1. Klasör kuralı

Her görev (mevcut task numarasıyla, ör. `A-04`) kendi klasöründe yaşar:

```
BACKEND_AKADEMI/
├── A-0X_kisa-konu-adi/
│   ├── index.html          -- bu görevin bölüm listesi (landing sayfası)
│   ├── 01_ilk-bolum.html
│   ├── 02_ikinci-bolum.html
│   └── ...
```

- Klasör adı: `<TaskKodu>_<kebab-case-konu>` (ör. `A-04_kelime-crud`). Task kodu
  `docs/TASK.md`/`docs/API_YOL_HARITASI/`'daki koduyla **birebir aynı** olmalı — okuyucu iki
  sistem arasında geçiş yaptığında kafası karışmasın.
- Bölüm dosyaları klasör içinde `01_`, `02_`... öneki ile numaralanır (iki haneli, sıralama
  bozulmasın diye).
- Yeni görev eklerken **mevcut hiçbir klasöre dokunulmaz** — yalnızca yeni klasör açılır +
  kök `index.html`'e bir kart eklenir (bkz. §5).

## 2. Nasıl başlanır — `_TASLAK/` klasörünü kopyala

`BACKEND_AKADEMI/_TASLAK/` gerçek, çalışan bir örnek görev klasörüdür (tarayıcıda açılabilir).
Yeni görev eklerken:

1. `_TASLAK/` klasörünü `A-0X_konu-adi/` olarak kopyala.
2. `index.html` içindeki başlık/açıklama/bölüm listesini doldur.
3. `01_ornek-bolum.html` içindeki `window.MODULE` objesini gerçek içerikle değiştir — dosya
   zaten **her slayt türünün** (`kapak, kavram, kod, karsilastirma, sozluk, ozet`) bir örneğini
   içeriyor, hangi alanın ne işe yaradığını yorumlarla anlatıyor.
4. Gerekirse dosyayı çoğalt (`02_...html`, `03_...html`), her birinde `oncekiBolum`/
   `sonrakiBolum` alanlarını bir sonraki/önceki dosyaya göre güncelle.
5. Yeni görevin ilk bölümünün `oncekiBolum`'unu, önceki görevin SON bölümüne (göreli yol,
   ör. `../A-03_auth-register/12_ozet-sozluk.html`) bağla — akademi baştan sona tek bir
   doğrusal akış olarak da gezilebilsin.

## 3. `window.MODULE` şeması (zorunlu/opsiyonel alanlar)

```js
window.MODULE = {
  id: 'kebab-case-id',                 // zorunlu, benzersiz
  bolumBaslik: 'A-0X — Görev Adı',     // zorunlu, topbar breadcrumb'ında görünür
  oncekiBolum: '../onceki/dosya.html', // yoksa null — ilk slaytta "Önceki Bölüm" butonu kapanır
  sonrakiBolum: 'sonraki-dosya.html',  // yoksa null — son slaytta "Sonraki Bölüm" butonu kapanır
  slaytlar: [ /* aşağıdaki türlerden en az 1 */ ]
};
```

Slayt türleri (`tur` alanı motorun hangi şablonu çizeceğini belirler):

| tur | Ne zaman kullanılır | Zorunlu alanlar |
|---|---|---|
| `kapak` | Bölümün ilk slaytı, başlık ekranı | `baslik` (+ opsiyonel `ustBaslik`, `altBaslik`) |
| `kavram` | Kod göstermeden bir kavramı anlatmak | `baslik`, `aciklama`, `neden`, `olmasaydi` |
| `kod` | Gerçek kaynak dosyadan bir kod bloğu, satır satır | `baslik`, `dosyaYolu`, `kod`, `satirlar[]` |
| `karsilastirma` | "Doğru yapılırsa / yanlış yapılırsa" iki sütun | `baslik`, `iyi`, `kotu` |
| `sozluk` | Terim tanımları grid'i | `baslik`, `terimler[]` |
| `ozet` | Bölüm kapanışı, madde listesi | `baslik`, `maddeler[]` |

`kod` türünde `satirlar[]` her öğesi `{ satir, aciklama, neden, olmasaydi }` — `satir` alanı,
`kod` metnindeki satırla **karakter karakter (trim edilmiş) eşleşmeli**, yoksa motor o satırı
tıklanabilir işaretlemez (sessizce atlar, hata vermez).

## 4. Değişmez yazım kuralları (CLAUDE.md ile tutarlı)

- Her `kod` slaytı gerçek backend dosyasından **birebir** kopyalanır — kısaltılmaz, uydurulmaz.
  Satır numarası/dosya yolu doğru olmalı.
- Her `kod`/`kavram` slaytında zorunlu üçlü: **ne** (aciklama) → **neden** (mühendislik
  gerekçesi, "kural böyle" değil) → **olmasaydı ne olurdu** (somut senaryo).
- Metinler Türkçe, kod/tanımlayıcı isimler İngilizce (CLAUDE.md §1 ile aynı disiplin).
- Hedef okuyucu junior'dan daha acemi — jargon kullanılıyorsa aynı slaytta veya `sozluk`
  türünde tanımlanmalı.

## 5. Kök `index.html`'e kart ekleme

Görev tamamlanınca `BACKEND_AKADEMI/index.html` içindeki `.landing-grid` bloğuna, mevcut
kartların birebir aynı yapısında yeni bir `.landing-card` eklenir (bkz. dosyanın içindeki
yorum). Kart eklemeden önceki kartlara **dokunulmaz**.

## 6. Motoru genişletmek istersen (yeni slayt türü)

Yeni bir `tur` gerekiyorsa: `engine/slides-engine.js` içinde `renderXxx(s)` fonksiyonu yazılır,
`RENDERERS` objesine `xxx: renderXxx` eklenir, `engine/slides.css`'e `.slide-xxx` sınıfı
eklenir. Var olan render fonksiyonlarına dokunulmaz (geriye dönük uyumluluk — eski bölümler
bozulmasın).
