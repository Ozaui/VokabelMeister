# Admin Akademi Sistemi (kök `AKADEMI/admin/`)

**Özet:** [[Backend_Akademi_Sistemi]]'nin admin panel (`/admin`, Faz B) karşılığı — aynı motor
(`engine/slides-engine.js`/`slides.css`, kopya, paylaşılmıyor), aynı klasör/slayt disiplini, tek
somut fark backend'deki `postman` slaytının yerini **`onizleme`** slaytının alması (bir
component'in tarayıcıda gerçekte nasıl davrandığı — route, kullanıcı akışı, backend'e giden
çağrı, durumlar). Kararın arka planı: kullanıcı Faz B'ye (B-01 Kurulum) başlamadan hemen önce,
backend'de kurulan "her kod parçası neden yazıldığıyla anlatılır" disiplininin admin
frontend'inde de aynı şekilde sürmesini istedi. Aynı gün içinde ikinci bir karar geldi:
Admin için artık **hiç roadmap yazılmayacak** — eski `docs/ADMIN_YOL_HARITASI/` (hiç gerçek
içerik yazılmamış, yalnızca boş şablondu) bu yüzden **silindi**, Admin'in TEK dokümantasyon
kanalı bu akademi oldu (Web/Mobil hâlâ kendi roadmap'lerini kullanıyor, bkz.
[[Backend_Akademi_Sistemi]] "Frontend Kardeşleri").

**Kütüphaneler:** Saf HTML/CSS/JS (framework yok, `AKADEMI/backend/engine/`'in birebir kopyası + genişletmesi)
**Bağlantılar:** [[Backend_Akademi_Sistemi]] · [[Gelistirme_Yol_Haritasi]] · [[Kodlama_Standartlari]]

## Dosyalar

| Dosya | Amaç |
|-------|------|
| `AKADEMI/admin/index.html` | Hub — tamamlanmış görevlerin kart listesi (`landing-grid`, şu an boş) |
| `AKADEMI/admin/STANDART.md` | Ortak kurallarda `AKADEMI/backend/STANDART.md`'ye referans verir, yalnızca frontend'e özel farkı (`onizleme` şeması) tekrar yazar |
| `AKADEMI/admin/_TASLAK/` | Çalışan örnek görev klasörü — her slayt türünün (`kapak/kavram/kod/karsilastirma/sozluk/onizleme/ozet`) bir örneğini içerir, React/TS koduyla |
| `AKADEMI/admin/engine/slides-engine.js` | `AKADEMI/backend/engine/`'den kopyalandı + `renderOnizleme` fonksiyonu ve `onizleme: renderOnizleme` RENDERERS kaydı eklendi |
| `AKADEMI/admin/engine/slides.css` | Kopya + `.slide-onizleme`/`.onizleme-*` stilleri eklendi (postman kartlarıyla aynı görsel dil) |
| `AKADEMI/admin/B-0X_<konu>/index.html` + `NN_<baslik>.html` | Bir görevin landing sayfası + bölümleri |

## `onizleme` Slaytı — `postman`'in Frontend Karşılığı

```js
{
  tur: 'onizleme',
  baslik: '...', rota: '/words',           // zorunlu — gerçek route path'i
  aciklama: '...',                          // opsiyonel
  akis: [{ eylem: '...', sonuc: '...' }],   // zorunlu, en az 1 — kullanıcı ne yapar → ne olur
  apiCagrisi: { yontem: 'POST', url: '...' },// opsiyonel — RTK Query'nin gerçekte gittiği backend endpoint'i
  durumlar: [{ durum: 'Yükleniyor', gorunum: '...' }], // opsiyonel
  notlar: ['...']                           // opsiyonel
}
```

Tetikleyici backend'deki "controller'a bağlandığında" kuralıyla birebir aynı mantık, yalnızca
adım farklı: CLAUDE.md §4 adım 6 (**Route/Import**) tamamlandığında, o component akademiye
işlenirken `kod` slaytından HEMEN SONRA bir `onizleme` slaytı eklenir. `apiCagrisi` alanı,
component saf UI/local-state ise (ör. bir modal aç/kapa) hiç yazılmaz — yalnızca gerçekten
RTK Query üzerinden backend'e giden component'lerde var.

## Motor Neden Kopya, Paylaşılan Değil

`AKADEMI/backend/engine/` ve `AKADEMI/admin/engine/` ayrı dosyalar (sembolik link/paket değil)
— iki akademi ayrı hızda değişebilsin diye bilinçli bir tercih (CLAUDE.md §6 "Motor
değişikliği" notu). Bedeli: motora genel bir bug fix gelirse (yeni slayt türü değil) her iki
kopyaya da elle uygulanmalı — bu disiplin CLAUDE.md'de açıkça yazılı, unutulursa akademiler
sessizce birbirinden sapar.

## CLAUDE.md §6 Genelleşmesi

Bu karar öncesinde CLAUDE.md §6 yalnızca "Backend Akademi Kuralı" başlığıyla `AKADEMI/backend`'ye
özeldi. §6 artık "Kod Akademisi Kuralı" — faz→akademi klasörü→task kodu→"nasıl denerim" slaytı
eşlemesini bir tabloyla veren, backend+admin'i (ve D/E fazları başlayınca açılacak
`AKADEMI/web`/`AKADEMI/mobile`'yi) aynı kural setiyle kapsayan genel bir bölüm. `CLAUDE.md` §1
"Yorum satırları" istisnası (akademi `aciklama`/`neden`/`olmasaydi` alanları minimal-yorum
kuralının dışında) de bu genellemeye göre güncellendi.

## Durum (2026-07-26 itibarıyla)

Henüz hiçbir gerçek görev klasörü yok (`B-01` Kurulum akademiye işlenecek ilk görev değil —
CLAUDE.md §3'teki "dikey dilim/roadmap kuralı A-01 gibi burada uygulanmaz" notuyla aynı mantık,
B-01 salt kurulum, ilk gerçek akademi bölümü B-02'den başlayacak). İskelet (`engine/`,
`STANDART.md`, `_TASLAK/`, boş `index.html`) hazır.
