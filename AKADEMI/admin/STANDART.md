# Admin Akademi (`AKADEMI/admin/`) — Yeni Görev Ekleme Standardı

> Bu dosya, `AKADEMI/backend/STANDART.md`'nin frontend (admin panel) karşılığıdır — motor
> (`engine/`) ve klasör/slayt mimarisi BİREBİR aynı, tek somut fark backend'in `postman` slaytının
> yerini frontend'de `onizleme` slaytının alması (bkz. §3). İki dosya arasında çelişki olursa
> ortak kurallarda `AKADEMI/backend/STANDART.md` otorite kaynağıdır, bu dosya yalnızca frontend'e
> özel farkları taşır.

## 1. Klasör kuralı

Her görev (mevcut task numarasıyla, ör. `B-03`) kendi klasöründe yaşar:

```
AKADEMI/admin/
├── B-0X_kisa-konu-adi/
│   ├── index.html          -- bu görevin bölüm listesi (landing sayfası)
│   ├── 01_ilk-bolum.html
│   ├── 02_ikinci-bolum.html
│   └── ...
```

- Klasör adı: `<TaskKodu>_<kebab-case-konu>` (ör. `B-03_kelime-yonetimi`). Task kodu
  `docs/TASK/B_admin_panel.md`'deki koduyla **birebir aynı** olmalı.
- Bölüm dosyaları klasör içinde `01_`, `02_`... öneki ile numaralanır (iki haneli, sıralama
  bozulmasın diye).
- Yeni görev eklerken **mevcut hiçbir klasöre dokunulmaz** — yalnızca yeni klasör açılır +
  kök `index.html`'e bir kart eklenir (bkz. §5).

## 2. Nasıl başlanır — `_TASLAK/` klasörünü kopyala

`AKADEMI/admin/_TASLAK/` gerçek, çalışan bir örnek görev klasörüdür (tarayıcıda açılabilir).
Yeni görev eklerken:

1. `_TASLAK/` klasörünü `B-0X_konu-adi/` olarak kopyala.
2. `index.html` içindeki başlık/açıklama/bölüm listesini doldur.
3. `01_ornek-bolum.html` içindeki `window.MODULE` objesini gerçek içerikle değiştir — dosya
   zaten **her slayt türünün** (`kapak, kavram, kod, karsilastirma, sozluk, onizleme, ozet`) bir
   örneğini içeriyor, hangi alanın ne işe yaradığını yorumlarla anlatıyor.
4. Gerekirse dosyayı çoğalt (`02_...html`, `03_...html`), her birinde `oncekiBolum`/
   `sonrakiBolum` alanlarını bir sonraki/önceki dosyaya göre güncelle.
5. Yeni görevin ilk bölümünün `oncekiBolum`'unu, önceki görevin SON bölümüne (göreli yol,
   ör. `../B-02_auth-sayfalari/06_ozet-sozluk.html`) bağla — akademi baştan sona tek bir
   doğrusal akış olarak da gezilebilsin. Faz A'dan Faz B'ye geçişte ilk B görevinin
   `oncekiBolum`'u `../../backend/A-10_.../.../son-bolum.html` gibi **başka bir kardeş
   akademiye** (`AKADEMI/backend/`) de işaret edebilir (iki akademi `AKADEMI/` altında ayrı
   klasör ama tek doğrusal akışın parçası).

## 3. `window.MODULE` şeması (zorunlu/opsiyonel alanlar)

Şema `AKADEMI/backend/STANDART.md` §3 ile **birebir aynı** (`kapak/kavram/kod/karsilastirma/
sozluk/ozet/kod-degisiklik` — alan tanımları için oraya bak, burada tekrar edilmez). Tek fark:

| tur | Ne zaman kullanılır | Zorunlu alanlar |
|---|---|---|
| `onizleme` | Bir component'in tarayıcıda gerçekte NASIL davrandığı — hangi route'ta görünür, kullanıcı akışı, hangi backend endpoint'ine gider, farklı durumlarda (yükleniyor/hata) ne render edilir | `baslik`, `rota`, `akis[]` (+ opsiyonel `aciklama`, `apiCagrisi`, `durumlar[]`, `notlar[]`) |

`onizleme` türü **her component bir route'a bağlandığında (§3, CLAUDE.md §4 adım 6 "Route/
Import") o component'in `kod` slaytından hemen sonra** eklenir — backend'deki `postman`'in
CLAUDE.md §3 adım 13 kuralıyla birebir aynı mantık, yalnızca tetikleyici adım farklı. Alanlar:
- `rota`: kopyala-yapıştır yapılabilir gerçek route path'i (`/words`, `/users/:id`) — uydurma değil.
- `akis[]`: `{ eylem, sonuc }` çiftlerinden oluşan sıralı liste — kullanıcı ne yapar → arayüzde
  ne olur. En az bir adım zorunlu.
- `apiCagrisi`: `{ yontem, url }` — bu component hangi gerçek backend endpoint'ine (`apiClient`
  üzerinden, bkz. `admin/src/store/api.ts`) istek atıyor; backend akademideki ilgili `postman`
  slaytına zihinsel köprü kurar (component saf UI/local-state ise, örn. bir modal aç/kapa, bu
  alan hiç yazılmaz).
- `durumlar[]`: `{ durum, gorunum }` — component'in yükleniyor/hata/boş-liste gibi farklı
  state'lerinde ne göründüğü (opsiyonel ama form/liste component'lerinde önerilir).
- `notlar[]`: ön koşul/bağımlılık (ör. "önce login ile admin token al") — backend'deki
  `postman.notlar[]` ile aynı amaç.

`kod` türünde `satirlar[]` kuralı (satır↔açıklama eşleşmesi) `AKADEMI/backend/STANDART.md`
§3 ile aynı — tek fark kaynak kodun `.tsx`/`.ts` olması.

### 3.1 `kod-degisiklik` ve "saf ekleme" kuralları

`AKADEMI/backend/STANDART.md` §3.1/§3.2 ile **birebir aynı**, `.cs` yerine `.tsx`/`.ts` dosyaları
için geçerli — burada tekrar edilmez.

## 4. Değişmez yazım kuralları (CLAUDE.md ile tutarlı)

- Her `kod` slaytı gerçek admin dosyasından **birebir** kopyalanır — kısaltılmaz, uydurulmaz.
  Satır numarası/dosya yolu doğru olmalı (`admin/src/...`).
- Her `kod`/`kavram` slaytında zorunlu üçlü: **ne** (aciklama) → **neden** (mühendislik
  gerekçesi, "kural böyle" değil) → **olmasaydı ne olurdu** (somut senaryo).
- Metinler Türkçe, kod/tanımlayıcı isimler İngilizce (CLAUDE.md §1 ile aynı disiplin).
- Hedef okuyucu junior'dan daha acemi — jargon kullanılıyorsa aynı slaytta veya `sozluk`
  türünde tanımlanmalı.
- **Eski bölümler, sonradan yapılan SAF EKLEMELER için güncellenmez** (`AKADEMI/backend/
  STANDART.md` §4 ile aynı istisna kuralı geçerli).
- **Var olan bir kodun SATIRI/İMZASI DEĞİŞTİĞİNDE** eski bölüm dokunulmadan bırakılır,
  değişikliği yapan görevin bölümüne bir `kod-degisiklik` slaytı eklenir.
- **Akademinin KENDİ yazdığı öğretici metinlerinde (`aciklama`/`neden`/`olmasaydi`/
  `nedenBuKlasor`/`altBaslik`/`notlar` gibi alanlar) okuyucuya proje kural dosyası (CLAUDE.md,
  DESIGN_SYSTEM.md, REFERENCE/*.md, TASK.md vb.) İSİMLE gösterilmez/atıf
  yapılmaz.** Akademiyi okuyan kişi bu dosyaları hiç görmemiştir, "CLAUDE.md §1 kuralına göre"
  gibi bir gerekçe onun için bir OTORİTEYE atıftan ibarettir, mühendislik gerekçesi DEĞİLDİR.
  `neden`/`olmasaydi` her zaman dosya adı geçmeden, KENDİ başına ayakta duran somut bir
  mühendislik gerekçesiyle yazılır (ör. "CLAUDE.md §1'e göre" yerine değişikliğin GERÇEK
  sonucunu — hangi senaryoda ne bozulur — anlat). İstisna 1: akademinin KENDİ İÇİNDEKİ diğer
  bölümlere/slaytlara ("Bölüm 3'teki authSlice", "bir önceki slayt") ve gerçek kaynak dosya
  yollarına (`admin/src/...`) atıf serbest — bunlar okuyucunun akademi İÇİNDE zaten
  gezdiği/göreceği şeyler. İstisna 2: `kod` slaytının `kod` alanı GERÇEK dosyanın birebir
  kopyasıdır (§4'ün ilk maddesi) — o dosyada GERÇEKTEN böyle bir yorum satırı varsa (ör.
  `// CLAUDE.md §1: ...`), birebir kopya kuralı gereği DOKUNULMAZ; bu kural yalnızca
  akademinin KENDİ ürettiği açıklama metinlerine uygulanır.

## 5. Kök `index.html`'e kart ekleme

Görev tamamlanınca `AKADEMI/admin/index.html` içindeki `.landing-grid` bloğuna, `AKADEMI/backend/
index.html`'deki kartlarla birebir aynı yapıda yeni bir `.landing-card` eklenir. Kart eklemeden
önceki kartlara **dokunulmaz**.

## 6. Motoru genişletmek istersen (yeni slayt türü)

`engine/` klasörü `AKADEMI/backend/engine/`'den bağımsız bir KOPYA (paylaşılan/sembolik link
değil — iki akademi ayrı hızda değişebilir). Yeni bir `tur` gerekiyorsa bu klasördeki
`slides-engine.js`'e `renderXxx(s)` fonksiyonu yazılır, `RENDERERS` objesine `xxx: renderXxx`
eklenir, `slides.css`'e `.slide-xxx` sınıfı eklenir. Var olan render fonksiyonlarına
dokunulmaz (geriye dönük uyumluluk — eski bölümler bozulmasın). Motora eklenen genel bir
iyileştirme (yeni slayt türü DEĞİL, ör. bir bug fix) **her iki** `engine/` kopyasına da
uygulanmalı — tek bir akademide sessizce farklı davranış bırakılmaz.
