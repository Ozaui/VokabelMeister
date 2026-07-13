# Backend Akademi Sistemi (kök `BACKEND_AKADEMI/`)

**Özet:** Her backend API'ın adım adım nasıl yazıldığını gösteren, junior geliştirici eğitimi amaçlı interaktif HTML slayt sistemi — `docs/API_YOL_HARITASI/`'nın (bkz. [[API_Yol_Haritasi_Sistemi]], artık silindi) 2026-07-13'te yerini aldı. `docs/` altında değil, repo kökünde yaşar (`BACKEND_AKADEMI/`) çünkü artık bir "roadmap" değil, bağımsız bir eğitim materyali. [[Kodlama_Standartlari]]'nın "her kod parçası yazılır yazılmaz hemen işlenir" kuralı burada somutlaşır — bir API bu akademiye işlenmeden tamamlanmış sayılmaz (`CLAUDE.md` §6).
**Kütüphaneler:** Saf HTML/CSS/JS (framework yok)
**Bağlantılar:** [[Gelistirme_Yol_Haritasi]] · [[Kodlama_Standartlari]] · [[Backend_Katmanli_Mimari]] · [[API_Yol_Haritasi_Sistemi]] (öncül sistem, tarihsel)

## Eski Sistemden Fark — Neden Değişti

Eski `API_YOL_HARITASI/` sistemi tek bir HTML sayfasında (`grup`/`katmanlar` ile bölünmüş) BÜYÜK bir akordiyon listesiydi — A-03 sayfası 64 adıma çıkınca (`adim.grup`/`API.katmanlar`, `adim.sonuclar` trx-embedding, `frontendRefs`/`backendRef` JSON çapraz linki gibi giderek karmaşıklaşan bir şema ile) bakımı ağırlaştı. `BACKEND_AKADEMI/` bunun yerine **"bir konsept = bir ekran, ileri/geri ile gez"** ilkesiyle çalışır: her görev kendi klasöründe, her klasör küçük numaralı bölüm dosyalarına (`01_...html`, `02_...html`…) bölünür, gezinme `oncekiBolum`/`sonrakiBolum` zinciriyle yapılır. Eski sistemin bazı özellikleri **bilinçli olarak taşınmadı** (YAGNI):
- `adim.sonuclar` (gerçek `dotnet test` çıktısının trx'ten gömülmesi) — testin gerçekten geçtiğinin kanıtı artık kod tabanındaki CI/test koşusunun kendisi, akademi yalnızca öğretir.
- `frontendRefs`/`backendRef` yapılandırılmış JSON çapraz linki — bu ihtiyaç zaten `docs/TASK/A_admin_panel_backend.md`/`C_kullanici_backend.md`'deki **"Frontend karşılığı:"** düz metin notuyla karşılanıyordu (bağımsız, roadmap sistemine bağlı değildi) — ayrı bir mekanizma gereksizdi.
- `adim.grup`/`API.katmanlar` (tek sayfa içi TOC bölümleme) — artık gerek yok, çünkü büyük bir API zaten birden çok KÜÇÜK bölüm dosyasına bölünüyor; motorun kendi İçindekiler paneli (`O` tuşu) tüm bölüm dosyalarını listeler.

## Dosyalar

| Dosya | Amaç |
|-------|------|
| `BACKEND_AKADEMI/index.html` | **Hub** — tüm tamamlanmış görevlerin kart listesi (`landing-grid`) |
| `BACKEND_AKADEMI/STANDART.md` | Tek doğruluk kaynağı: yeni görev/bölüm eklerken izlenecek şema + kurallar (`window.MODULE` alanları, 6 slayt türü, klasör/numaralama kuralı) |
| `BACKEND_AKADEMI/_TASLAK/` | Çalışan örnek görev klasörü — yeni görev eklerken kopyalanır, her slayt türünün (`kapak/kavram/kod/karsilastirma/sozluk/ozet`) bir örneğini içerir |
| `BACKEND_AKADEMI/engine/slides-engine.js` | `window.MODULE` objesini okuyup tam ekran slayt sunumuna çeviren paylaşımlı render motoru (ileri/geri, İçindekiler, kod satırı ↔ açıklama senkronu, `esc()` ile XSS güvenliği) |
| `BACKEND_AKADEMI/engine/slides.css` | Tüm bölüm dosyalarının paylaştığı ortak stil |
| `BACKEND_AKADEMI/A-0X_<konu>/index.html` | Bir görevin landing sayfası — o görevin bölüm listesi |
| `BACKEND_AKADEMI/A-0X_<konu>/NN_<baslik>.html` | Tek bir bölüm — `window.MODULE.slaytlar[]` içeriği |

## `window.MODULE` Şeması (özet — tam detay `STANDART.md` §3)

```js
window.MODULE = {
  id: 'kebab-case-id',
  bolumBaslik: 'A-0X — Görev Adı',
  oncekiBolum: '../onceki/dosya.html',   // yoksa null
  sonrakiBolum: 'sonraki-dosya.html',    // yoksa null
  slaytlar: [ /* kapak | kavram | kod | karsilastirma | sozluk | ozet */ ]
};
```

`kod` türünde `dosyaYolu` + `kod` (gerçek dosyadan birebir, kırpılmaz) + `satirlar[]` (`{satir, aciklama, neden, olmasaydi}` — `satir` alanı `kod` içindeki bir satırla trim edilmiş hâliyle karakter karakter eşleşmeli, motor bunu tıklanabilir işaretler). Her `kod`/`kavram` slaytında zorunlu üçlü: **ne** (`aciklama`) → **neden** (`neden`) → **böyle olmasaydı** (`olmasaydi`) — [[Kodlama_Standartlari]]'ndaki "aciklama Standardı" kuralının burada karşılığı.

## Temsili Öğretim Kuralı (YAGNI)

Roadmap'teki HER adım kendi slaytını almaz. Tekrarlayan kod aileleri (ör. 13 Auth handler'ından yalnızca `RegisterCommandHandlerTests`, 7 exception sınıfından yalnızca `DuplicateEmailException`) TEK bir temsili `kod` slaytıyla öğretilir; geri kalanlar aynı bölümün `sozluk` slaytında "aynı pattern'i izler" notuyla listelenir. Bu, 2026-07-13'teki büyük senkronizasyon turunda (bkz. Yirmi dokuzuncu INGEST) API_YOL_HARITASI'nda yazılıp akademide hiç işlenmemiş konular (AppException/ErrorMessages mimarisi, PasswordService/EmailService, RefreshTokenRepository, validator katmanı, Deny/Forbidden QR akışı vb.) eklenirken belirlenen resmi öğretim yaklaşımıdır.

## Yeni Görev/Bölüm Ekleme Kuralı

`STANDART.md`'de tam adımlarıyla yazılı, özet:
1. `_TASLAK/` klasörünü `A-0X_konu-adi/` olarak kopyala, `index.html` + `01_ornek-bolum.html`'i doldur.
2. Yeni görevin ilk bölümünün `oncekiBolum`'unu önceki görevin SON bölümüne bağla (zincir kesintisiz kalsın).
3. Kök `BACKEND_AKADEMI/index.html`'e görev tamamlanınca bir kart eklenir.
4. Mevcut bir göreve sonradan eksik kalan bir konu eklenirken (retrofit): yeni bölüm dosyaları klasörün SONUNA eklenir, kapanış (`ozet-sozluk`) bölümü daima en son numaraya taşınır, `oncekiBolum`/`sonrakiBolum` zinciri (hem klasör içi hem komşu klasöre çıkan sınır bağlantıları) uçtan uca güncellenir.

## Mevcut Görevler (2026-07-13 itibarıyla)

| Klasör | Bölüm Sayısı | Durum |
|--------|-------------|-------|
| `A-02_ortak-altyapi/` | 9 (00-08) | tamamlandı |
| `A-03_auth-register/` | 25 (01-11, 13-25) | tamamlandı |
| `A-03.1_qr-login/` | 13 (01-13) | tamamlandı |
| `A-03.2_mesaj-lokalizasyonu/` | 8 (01-08) | tamamlandı |
| `A-03.3_tema-tercihi/` | 3 (01-03) | tamamlandı |

## Frontend Kardeşleri — Değişmedi

Admin (`docs/ADMIN_YOL_HARITASI/`, Faz B), Web (`docs/WEB_YOL_HARITASI/`, Faz D) ve Mobil (`docs/MOBILE_YOL_HARITASI/`, Faz E) kendi bağımsız roadmap sistemlerini (eski akordiyon mantığıyla, `render.js`) sürdürüyor — bu geçiş yalnızca backend'i etkiledi. Üçü de ortak `docs/style.css`'i paylaşır (eskiden `API_YOL_HARITASI/style.css`'ti, taşındı). Çapraz link ihtiyacı (bir API'yi hangi frontend feature'ın tükettiği) `docs/TASK/A_admin_panel_backend.md`/`C_kullanici_backend.md`'deki **"Frontend karşılığı:"** düz metin notuyla karşılanır — akademiye ayrıca bir JSON alanı eklenmedi.
