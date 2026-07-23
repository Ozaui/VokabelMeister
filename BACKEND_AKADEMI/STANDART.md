# BACKEND_AKADEMI — Yeni Görev Ekleme Standardı

> Bu dosya, yeni bir API/görev eklerken **kod taramadan, sıfırdan tasarlamadan** doğrudan
> kopyalayıp doldurabileceğin bir şablon + kısa bir kural seti sağlar. Yeni bir konuşmada/
> oturumda bu dosyayı okumak, tüm `engine/` mimarisini yeniden keşfetmekten daha hızlı olmalı.

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
  `docs/TASK.md`/`docs/TASK/<faz>.md`'deki koduyla **birebir aynı** olmalı — okuyucu iki
  dosya arasında geçiş yaptığında kafası karışmasın.
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
| `postman` | Bir endpoint'e Postman'dan (curl'e de uyar) gerçekte nasıl istek atılacağı | `baslik`, `yontem`, `url`, `yanit` (+ opsiyonel `aciklama`, `kimlikDogrulama`, `headers[]`, `govde`, `notlar[]`) |
| `kod-degisiklik` | Daha önce bir `kod` slaytında öğretilmiş bir dosyanın SONRADAN (başka bir görevde) değişmesi | `baslik`, `dosyaYolu`, `diff`, `neden` (+ opsiyonel `satirlar[]`) |

`postman` türü **her endpoint'i controller'a bağlayan `kod` slaytından hemen sonra** eklenir —
CLAUDE.md §3 adım 13 notuna bak (Backend Akademi'ye işlerken zorunlu adım). Alanlar:
- `yontem`: `GET`/`POST`/`PUT`/`DELETE` (büyük harf).
- `url`: kopyala-yapıştır yapılabilir TAM adres (`http://localhost:5001/api/v1/...`) — göreli yol değil.
- `kimlikDogrulama`: `[Authorize]` varsa `'Bearer {{accessToken}}'` gibi Authorization header değeri; yoksa alan hiç yazılmaz.
- `headers[]`: `Content-Type` dışında özel bir header gerekiyorsa (`{ anahtar, deger }`); `Content-Type: application/json` gövdesi olan her istekte örtük kabul edilir, ayrıca yazılmaz.
- `govde`: request body'nin gerçek DTO/Command alan adlarıyla birebir örnek JSON'u (uydurma alan adı yazılmaz); body yoksa alan hiç yazılmaz.
- `yanit`: `{ durum, govde }` — gerçek response DTO şekliyle birebir örnek başarı yanıtı.
- `notlar[]`: ön koşul/bağımlılık (ör. "bu isteği atmadan önce X adımını tamamla", "OTP kodu için backend konsolundaki `[DEV EMAIL]` logunu oku") — sık yapılan bir Postman hatası varsa da buraya yazılır.

`kod` türünde `satirlar[]` her öğesi `{ satir, aciklama, neden, olmasaydi }` — `satir` alanı,
`kod` metnindeki satırla **karakter karakter (trim edilmiş) eşleşmeli**, yoksa motor o satırı
tıklanabilir işaretlemez (sessizce atlar, hata vermez).

### 3.1 `kod-degisiklik` — daha önce öğretilmiş bir kod SONRADAN değiştiğinde

Yazılım bir süreçtir: bir görev bitip akademiye işlendikten SONRA, BAŞKA bir görev o dosyaya geri
dönüp **var olan bir satırı/imzayı/metodu değiştirebilir** (ör. A-04, A-03'te yazılmış bir Handler'a
`ISecurityLogger` parametresi ekledi). Bu durumda:

- **Eski bölüm SESSİZCE bırakılmaz, güncellenmez de** (bkz. §3.2 "yalnızca ekleme" kuralı) — onun
  yerine bu değişikliğin işlendiği bölüme (genelde değişikliği yapan görevin kendi klasörüne) bir
  `kod-degisiklik` slaytı eklenir; o slayt hem ESKİ hem YENİ hâli git-diff tarzı TEK blokta gösterir.
- `diff` alanı gerçek bir unified diff gibi yazılır — HER satırın İLK karakteri `+` (eklendi),
  `-` (silindi) veya ` ` (boşluk — değişmedi/bağlam) olmalı, ikinci karakterden itibaren kodun
  kendisi gelir (girinti dahil). Motor kırmızı/üstü-çizili (silinen) ve yeşil (eklenen) olarak
  render eder.
- `neden` **zorunlu** — bir kod değişikliği asla sebepsiz olmaz (yeni bir görev, yeni bir
  gereksinim); bu alan o olayı adıyla anar (ör. "A-04'te SecurityLog entegrasyonu gerektiği için").
- `satirlar[]` opsiyonel ama önerilir — özellikle YENİ eklenen satırlar için `{ satir, aciklama,
  neden, olmasaydi }` üçlüsü, `kod` slaytındaki AYNI kuralla eklenir.
- Bu slayt, DEĞİŞEN dosyanın SONRAKİ görevine (değişikliği yapan göreve) ait bölümde yer alır —
  eski görevin kendi `kod` slaytına dokunulmaz, yalnızca yeni görev "bak, bu dosyaya önceki bir
  görevde yazdığımız X metodu değişti" diye bu slaytla işaret eder.

### 3.2 Var olan bir dosyaya SAF EKLEME yapıldığında (değişiklik değil) — ne kadarı gösterilir

Bir görev, var olan bir dosyaya davranışı DEĞİŞTİRMEDEN yeni, bağımsız bir şey ekliyorsa (yeni bir
DbSet property'si, yeni bir DI kaydı satırı, yeni bir standalone metot/sınıf) bu bir `kod-degisiklik`
DEĞİLDİR — normal bir `kod` slaytıdır, ama TÜM dosyayı göstermeye gerek YOKTUR:

- **Tek başına çalışabilen ekleme** (yeni bir metot, yeni bir DI satırı, yeni bir property) →
  yalnızca eklenen kod + öncesindeki 3-4 satır + sonrasındaki 3-4 satır gösterilir. Amaç, okuyucunun
  "bu YENİ satır dosyanın neresine, hangi bağlama eklendi" sorusuna cevap bulması — dosyanın tamamı
  gerekmez.
- **Var olan bir FONKSİYONUN/metodun İÇİNE ekleme** (var olan bir metodun gövdesine yeni satır(lar)
  eklendi) → bu durumda o metodun **TAMAMI** gösterilir, yalnızca eklenen kısım değil — kısmi bir
  metot gövdesi okuyucuyu metodun geri kalanını tahmin etmeye zorlar, bu kabul edilemez. Dosyanın
  GERİ KALANI (metodun dışındaki diğer metotlar) yine de gösterilmeyebilir.

## 4. Değişmez yazım kuralları (CLAUDE.md ile tutarlı)

- Her `kod` slaytı gerçek backend dosyasından **birebir** kopyalanır — kısaltılmaz, uydurulmaz.
  Satır numarası/dosya yolu doğru olmalı.
- Her `kod`/`kavram` slaytında zorunlu üçlü: **ne** (aciklama) → **neden** (mühendislik
  gerekçesi, "kural böyle" değil) → **olmasaydı ne olurdu** (somut senaryo).
- Metinler Türkçe, kod/tanımlayıcı isimler İngilizce (CLAUDE.md §1 ile aynı disiplin).
- Hedef okuyucu junior'dan daha acemi — jargon kullanılıyorsa aynı slaytta veya `sozluk`
  türünde tanımlanmalı.
- **Eski bölümler, sonradan yapılan SAF EKLEMELER için güncellenmez.** Yazılım bir süreçtir;
  bir görev bitip akademiye işlendikten sonra başka bir görev o alana yeni, ilgisiz bir şey
  eklerse (yeni tablo/DbSet, yeni DI kaydı, yeni policy) eski bölümün bunu göstermemesi HATA
  DEĞİLDİR — güncellenmeye çalışılmaz. Bunun tek istisnası: eski bölüm o alan için açıkça bir
  TAMLIK iddiasında bulunuyorsa (ör. "TAM DOSYA", "tüm alanlar") — böyle bir iddia varsa ve artık
  doğru değilse, iddianın kendisi (ör. "A-0X dönemindeki hâli" gibi zamana bağlı bir ifadeye)
  düzeltilir, kodun içeriği güncellenmez.
- **Var olan bir kodun SATIRI/İMZASI DEĞİŞTİĞİNDE** (ekleme değil, değişiklik — bkz. §3.1) eski
  bölüm dokunulmadan bırakılır, değişikliği yapan görevin bölümüne bir `kod-degisiklik` slaytı
  eklenir.

## 5. Kök `index.html`'e kart ekleme

Görev tamamlanınca `BACKEND_AKADEMI/index.html` içindeki `.landing-grid` bloğuna, mevcut
kartların birebir aynı yapısında yeni bir `.landing-card` eklenir (bkz. dosyanın içindeki
yorum). Kart eklemeden önceki kartlara **dokunulmaz**.

## 6. Motoru genişletmek istersen (yeni slayt türü)

Yeni bir `tur` gerekiyorsa: `engine/slides-engine.js` içinde `renderXxx(s)` fonksiyonu yazılır,
`RENDERERS` objesine `xxx: renderXxx` eklenir, `engine/slides.css`'e `.slide-xxx` sınıfı
eklenir. Var olan render fonksiyonlarına dokunulmaz (geriye dönük uyumluluk — eski bölümler
bozulmasın).
