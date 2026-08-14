# Backend Akademi (`AKADEMI/backend/`) — Yeni Görev Ekleme Standardı

> Bu dosya, yeni bir API/görev eklerken **kod taramadan, sıfırdan tasarlamadan** doğrudan
> kopyalayıp doldurabileceğin bir şablon + kısa bir kural seti sağlar. Yeni bir konuşmada/
> oturumda bu dosyayı okumak, tüm `engine/` mimarisini yeniden keşfetmekten daha hızlı olmalı.

## 1. Klasör kuralı

Her görev (mevcut task numarasıyla, ör. `A-04`) kendi klasöründe yaşar:

```
AKADEMI/backend/
├── A-0X_kisa-konu-adi/
│   ├── index.html          -- bu görevin bölüm listesi (landing sayfası)
│   ├── 01_ilk-bolum.html
│   ├── 02_ikinci-bolum.html
│   └── ...
```

- Klasör adı: `<TaskKodu>_<kebab-case-konu>` (ör. `A-04_kelime-crud`). Task kodu
  `docs/TASK/TASK.md`/`docs/TASK/<faz>.md`'deki koduyla **birebir aynı** olmalı — okuyucu iki
  dosya arasında geçiş yaptığında kafası karışmasın.
- Bölüm dosyaları klasör içinde `01_`, `02_`... öneki ile numaralanır (iki haneli, sıralama
  bozulmasın diye).
- Yeni görev eklerken **mevcut hiçbir klasöre dokunulmaz** — yalnızca yeni klasör açılır +
  kök `index.html`'e bir kart eklenir (bkz. §5).

## 2. Nasıl başlanır — `_TASLAK/` klasörünü kopyala

`AKADEMI/backend/_TASLAK/` gerçek, çalışan bir örnek görev klasörüdür (tarayıcıda açılabilir).
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

- **Kapsam — TÜM anlamlı satırlar:** `kod` bloğunda görünen HER anlamlı satır (`using` direktifi,
  namespace bildirimi, alan/property/metot bildirimi, metot gövdesindeki her ifade — atama, çağrı,
  `return`, `if`/`foreach` koşulu vb.) `satirlar[]`'da kendi girdisiyle eşleşmeli — yalnızca
  "ilginç" birkaç satır değil, TÜMÜ.
- **İstisna (satır AÇILMAZ):** (a) boş satırlar, (b) yalnızca yapısal kapanış/açılış
  karakterlerinden oluşan satırlar (`{`, `}`, `};`, `},`, `)`, `);`, `),` gibi — başka hiçbir
  tanımlayıcı/değer İÇERMİYORSA); satırda kapanışla birlikte GERÇEK bir ifade de varsa (ör. bir
  constructor'ın son parametresi `ILogger<X> logger)` gibi kapanış parantezini taşıyorsa) o satır
  YİNE kapsanır — ayrım "satır SADECE kapanış mı" (muaf) ile "kapanışın yanında gerçek bir ifade de
  var mı" (kapsanmalı) arasında, (c) `//` ya da `///` ile başlayan yorum satırları.
- **Tekrar eden BİREBİR AYNI satırlar — TEK girdi:** Motor `satirlar[]`'ı `satir` metnine göre bir
  haritaya (Map) yazar; aynı `kod`/`diff` bloğunda birden fazla kez geçen, karakter karakter AYNI
  bir satır (ör. `return;`, `[Fact]`, tekrarlayan bir `using service.Object` çağrısı) için BİRDEN
  FAZLA `satirlar[]` girdisi yazılırsa, motor yalnızca SONUNCUSUNU kullanır — kaynak koddaki O
  metnin TÜM tekrarları (ilk, ikinci, üçüncü...) tıklandığında AYNI (son yazılan) açıklamayı
  gösterir, önceki girdiler SESSİZCE görünmez olur. Bu yüzden aynı metinden birden çok kez geçen
  bir satır için TEK bir `satirlar[]` girdisi yazılır, `aciklama` bu satırın TÜM tekrarlarını
  kapsayacak şekilde genel yazılır (ör. "üç ayrı erken çıkıştan biri — env eksik/format geçersiz/
  kayıt zaten var, hangisi sağlanırsa buraya düşülür"). İki girdi yazıp "ilki bunun için, ikincisi
  onun için" diye ayırmaya ÇALIŞMAK bir hatadır, motor bunu DESTEKLEMEZ.

### 3.1 `kod-degisiklik` — daha önce TAM olarak öğretilmiş bir dosyaya SONRADAN dokunulduğunda

Yazılım bir süreçtir: bir dosya ÖNCEKİ bir bölümde bir `kod` slaytıyla (o zamanki) TAM haliyle
öğretildikten SONRA, BAŞKA bir görev/bölüm o dosyaya geri dönüp bir şey ekleyebilir ya da var olan
bir satırı/imzayı/metodu değiştirebilir (ör. A-04, A-03'te yazılmış bir Handler'a `ISecurityLogger`
parametresi ekledi; ya da `store.ts`'e tek bir yeni reducer satırı eklendi). **Bu ayrım —
"saf ekleme mi, gerçek bir değişiklik mi" — SONUCU ETKİLEMEZ: ikisi de aynı kuralla işlenir.**
Gerçek akademi verisinde (`AKADEMI/admin/B-01_kurulum/`, `store.ts`/`App.tsx`/`main.tsx`/
`Topbar.tsx` dosyalarının her biri BİRDEN ÇOK bölümde tekrar dokunulmuş örnekleri) bunun istisnası
YOKTUR — tek bir yeni `import` satırı ya da tek bir yeni obje alanı eklenmesi bile normal bir `kod`
slaytına İNDİRGENMEZ:

- **Eski bölüm SESSİZCE bırakılmaz, güncellenmez de** — onun yerine bu değişikliğin işlendiği
  bölüme (genelde değişikliği yapan görevin kendi klasörüne) bir `kod-degisiklik` slaytı eklenir;
  o slayt hem ESKİ hem YENİ hâli git-diff tarzı TEK blokta gösterir.
- `diff` alanı dosyanın **O ANA KADAR ULAŞTIĞI TÜM içeriğini** gösterir — yalnızca değişen birkaç
  satır değil. Önceki bölümlerde zaten eklenmiş satırlar (BU değişiklikten ÖNCEKİ deltalar dahil)
  ` ` (context) olarak, YALNIZCA bu bölümde eklenen/silinen satırlar `+`/`-` olarak işaretlenir.
  HER satırın İLK karakteri `+` (eklendi), `-` (silindi) veya ` ` (boşluk — değişmedi/bağlam)
  olmalı, ikinci karakterden itibaren kodun kendisi gelir (girinti dahil). Motor context satırları
  **turuncu**, silinenleri kırmızı/üstü-çizili, eklenenleri **yeşil** render eder — okuyucu "bu
  satırlar DAHA ÖNCE yazılmıştı" (turuncu) ile "bu satır BU bölümde yeni" (yeşil) ayrımını
  bakar bakmaz görür. Bir dosya üçüncü kez değişiyorsa (ör. `store.ts` önce auth, sonra language,
  sonra theme reducer'ı aldıysa), ÜÇÜNCÜ `kod-degisiklik`'in diff'i language reducer'ını da
  context (turuncu) olarak gösterir — yalnızca theme reducer'ı `+`'dır.
- `neden` **zorunlu** — bir kod değişikliği asla sebepsiz olmaz (yeni bir görev, yeni bir
  gereksinim); bu alan o olayı adıyla anar (ör. "A-04'te SecurityLog entegrasyonu gerektiği için").
- `satirlar[]` opsiyonel ama önerilir — özellikle YENİ eklenen satırlar için `{ satir, aciklama,
  neden, olmasaydi }` üçlüsü eklenir; `satir` alanı diff'teki `+`/`-`/` ` ÖNEKİ OLMADAN, yalnızca
  kodun kendisiyle (trim edilmiş) eşleşmeli — motor eşleştirmeyi önek çıkarıldıktan sonra yapar.
  `satirlar[]` yazılıyorsa §3'teki kapsam kuralı (TÜM anlamlı satırlar + üç istisna) SADECE
  `+`/`-` satırlar için önek çıkarıldıktan SONRA burada da aynen geçerlidir.
- **CONTEXT (` ` önekli, turuncu) satırlara `satirlar[]` girdisi YAZILMAZ — hiç, kısa bir not
  bile değil.** Context satır zaten ÖNCEKİ bir bölümde `kod` slaytıyla TAM açıklanmış — burada
  "değişmeden kalan satır" gibi bir tekrar notu bile okuyucuya yeni bir bilgi vermez, yalnızca
  aynı açıklamayı iki yerde bakımı gereken bir kopyaya çevirir. Bu, comment/brace satırlarının
  hiç açıklanmaması kuralıyla (§3, üçüncü istisna) AYNI mantık — motor zaten context'i turuncu
  render ederek "bu zaten biliniyor" sinyalini VERİYOR, `satirlar[]` bunu tekrar etmez. Yalnızca
  `+`/`-` satırlar `satirlar[]` girdisi alır.
- Bu slayt, DEĞİŞEN dosyanın SONRAKİ görevine (değişikliği yapan göreve) ait bölümde yer alır —
  eski görevin kendi `kod` slaytına dokunulmaz, yalnızca yeni görev "bak, bu dosyaya önceki bir
  görevde yazdığımız X metodu değişti" diye bu slaytla işaret eder.

### 3.2 Yeni bir dosya İLK kez öğretiliyorsa

Yukarıdaki §3.1 yalnızca DAHA ÖNCE bir `kod` slaytıyla tam öğretilmiş bir dosya için geçerli. Bir
dosya bu görevde ilk kez yazılıyorsa (önceki hiçbir bölümde `dosyaYolu` olarak geçmiyorsa), normal
bir `kod` slaytı kullanılır ve dosyanın TAMAMI gösterilir — henüz "eski"/"yeni" ayrımı yapılacak bir
geçmişi yok.

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
- **"Değişmedi"/"aynı"/"BİREBİR" gibi geçmişe atıfla YETİNEN bir `aciklama` YASAK — bir satır
  ÖNCEKİ bir bölümde (hatta AYNI dosyanın kendi içinde başka bir yerinde) görülmüş olsa bile,
  yeni bir `kod`/`kod-degisiklik` slaytındaki HER satır kendi başına, o satırı İLK kez okuyan bir
  okuyucuya NE yaptığını anlatan gerçek bir açıklama alır.** "Bölüm 3'ten değişmedi", "Aynı,
  sözleşmeyle BİREBİR", "X ile AYNI" gibi ifadeler bir açıklamanın YERİNE geçemez — bunlar en
  fazla, gerçek açıklamaya EKLENEN bir bağlam notu olabilir ("... — Bölüm 3'teki AYNI parametre,
  burada X'e atanıyor" gibi), ama cümlenin TAMAMI bu atıftan ibaret olamaz. Bu kural özellikle
  önceden yazılmış bir arayüzün (interface) implementasyonunu gösteren `kod` slaytlarında ihlal
  edilmeye eğilimlidir (imza tekrar ediyor diye "aynı" denip geçilir) — böyle bir satır için de
  "bu parametre aşağıda hangi alana/işleme gidiyor" sorusu YANITLANMALIDIR. Yeni bir slayt
  yazıldıktan sonra `grep -n "değişmedi\|BİREBİR\|AYNI,\|ile AYNI\." <dosya>` ile taranıp
  eşleşen HER `aciklama`/`neden`/`olmasaydi` alanı gerçek bir açıklamayı İÇERİP içermediği
  kontrol edilir; yalnızca atıftan ibaretse yeniden yazılır.
- **Proje meta-dokümanlarına (`CLAUDE.md`, `TASK.md`, `STANDART.md`'nin kendisi, `ENV.md`,
  `SECURITY.md`, `API_ENDPOINTS.md`, `DATABASE_SCHEMA.md`, `TECHNICAL_SPECIFICATIONS.md`,
  `CODING_STANDARDS.md`, `DEVELOPMENT_SETUP.md`) bir slaytın `aciklama`/`neden`/`olmasaydi`/
  `nedenBuKlasor`/`baslik`/`postman.notlar[]` gibi ANLATI alanlarında "§X" şeklinde ATIF
  YAPILMAZ** (ör. "CLAUDE.md §1'in zorunlu kıldığı..." YAZILMAZ) — akademiyi okuyan bir junior bu
  iç meta-dokümanları hiç görmüyor/bilmiyor, bir kural "orada öyle yazıyor" diye değil, kendi
  başına ayakta duran mühendislik gerekçesiyle anlatılmalı. Bu kuralın İKİ istisnası var: (1) bir
  `kod:`/`diff:` bloğunun İÇİNDEKİ metin gerçek kaynak dosyanın birebir kopyası olduğu için (yukarı
  bkz. "birebir kopya" kuralı) o metnin kendisi bir meta-doküman adı içerse bile (ör. gerçek bir
  `.cs` dosyasının yorumu `// SECURITY.md §1.4` içeriyorsa) DOKUNULMAZ; (2) akademinin KENDİ görev
  kodlarına (`A-02`, `B-08` gibi — akademinin gelecek/geçmiş bölümlerine işaret eder) atıf
  YAPILABİLİR, bunlar dış meta-doküman değil. Yeni bir slayt yazdıktan HEMEN sonra
  `grep -n "STANDART\.md\|CLAUDE\.md\|SECURITY\.md\|API_ENDPOINTS\.md\|DATABASE_SCHEMA\.md\|TECHNICAL_SPECIFICATIONS\.md\|CODING_STANDARDS\.md\|ENV\.md\|DEVELOPMENT_SETUP\.md"`
  ile dosya taranır, `kod:`/`diff:` blokları İÇİNDEKİ eşleşmeler hariç kalan HER eşleşme
  meta-doküman adı/bölüm numarası olmadan aynı gerekçeyi taşıyacak şekilde yeniden yazılır.

## 5. Kök `index.html`'e kart ekleme

Görev tamamlanınca `AKADEMI/backend/index.html` içindeki `.landing-grid` bloğuna, mevcut
kartların birebir aynı yapısında yeni bir `.landing-card` eklenir (bkz. dosyanın içindeki
yorum). Kart eklemeden önceki kartlara **dokunulmaz**.

## 6. Motoru genişletmek istersen (yeni slayt türü)

Yeni bir `tur` gerekiyorsa: `engine/slides-engine.js` içinde `renderXxx(s)` fonksiyonu yazılır,
`RENDERERS` objesine `xxx: renderXxx` eklenir, `engine/slides.css`'e `.slide-xxx` sınıfı
eklenir. Var olan render fonksiyonlarına dokunulmaz (geriye dönük uyumluluk — eski bölümler
bozulmasın).
