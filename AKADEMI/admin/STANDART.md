# Admin Akademi (`AKADEMI/admin/`) — Yeni Görev Ekleme Standardı

> **Ortak kurallar (klasör yapısı, `_TASLAK/` kopyalama akışı, `kod`/`kavram`/`karsilastirma`/
> `sozluk`/`ozet`/`kod-degisiklik` slayt şemaları, yazım disiplini, kök `index.html`'e kart
> ekleme, motor genişletme) → `AKADEMI/backend/STANDART.md`, birebir aynı — burada tekrar
> edilmez.** Bu dosya yalnızca admin'e özgü ÜÇ farkı anlatır: task kodu deseni, `postman`
> yerine kullanılan `onizleme` slaytı, ve terminal komutları için `kod` slaytının `terminal: true`
> uzantısı.

## 1. Task kodu deseni

Backend'de klasörler `A-0X_konu-adi/`; admin'de **`B-0X_konu-adi/`** (`docs/TASK/
TASK_B_admin_panel.md`'deki koduyla birebir aynı, ör. `B-01_kurulum`, `B-02_auth-sayfalari`).

## 2. `postman` yerine `onizleme`

Backend akademide bir endpoint controller'a bağlandığında `kod` slaytından hemen sonra `postman`
slaytı eklenir (o endpoint'e Postman'dan gerçekte nasıl istek atılacağını gösterir). Admin'de
karşılığı yoktur — bunun yerine bir **component bir route'a bağlandığında** (`CLAUDE.md` §4 adım
6), o component'in `kod` slaytından **hemen sonra** bir `onizleme` slaytı eklenir: gerçek route,
kullanıcı akışı, varsa gerçek backend endpoint çağrısı, ekran durumları.

```js
{
  tur: 'onizleme',
  baslik: 'Tarayıcıda Dene — LoginPage',       // zorunlu
  route: '/login',                              // zorunlu — React Router path'i
  aciklama: 'Bu ekranın ne zaman/ne amaçla görüldüğü (opsiyonel, 1-2 cümle).',
  akis: [                                       // opsiyonel — kullanıcının attığı adımlar sırayla
    'Kullanıcı e-posta + şifre girer, Gönder\'e tıklar.',
    'authApi.login çağrılır → backend OTP gönderir.',
    'Başarılıysa /verify-otp\'a yönlendirilir.'
  ],
  apiCagrisi: {                                 // opsiyonel — bu akışın tetiklediği GERÇEK backend çağrısı
    yontem: 'POST',
    url: 'http://localhost:5001/api/v1/auth/login',
    govde: `{ "email": "...", "password": "..." }`,   // request body yoksa alan hiç yazılmaz
    yanit: { durum: 200, govde: `{ "message": "OTP gönderildi" }` }
  },
  durumlar: [                                   // opsiyonel — ekranın alabileceği farklı UI durumları
    { durum: 'Yükleniyor', aciklama: 'Submit butonu disabled, spinner gösterilir.' },
    { durum: 'Hata', aciklama: '401 dönerse toast: "E-posta veya şifre hatalı".' }
  ],
  notlar: [                                     // opsiyonel — ön koşul veya sık yapılan hata
    'Bu ekranı denemeden önce backend\'in çalışıyor olması gerekir (dotnet run).'
  ]
}
```

- `apiCagrisi` alanı, backend akademideki `postman` slaytıyla AYNI disiplini taşır: `govde`/
  `yanit.govde` gerçek DTO/Command alan adlarıyla birebir olmalı, uydurma alan yazılmaz.
- Saf UI component'i (hiçbir backend çağrısı yapmayan, ör. yalnızca state gösteren bir sunum
  bileşeni) için `apiCagrisi` alanı hiç yazılmaz.
- `akis[]` en az bir öğe içermeli — `onizleme` slaytının var olma amacı "bunu tarayıcımda nasıl
  denerim" sorusuna cevap vermek, akış olmadan bu cevaplanamaz.

## 3. `kod` slaytında `terminal: true` — çalıştırılan komutlar

Bir bölümde `npm i ...`, `npm create vite@latest ...` gibi TERMİNAL komutları çalıştırıldıysa
(dosya değil), bunlar `kavram` slaytı içine düz metin olarak GÖMÜLMEZ — backend'deki `kod`
slaytıyla AYNI "birebir + satır satır tıklanabilir açıklama" deneyimini alan, ama kaynak koddan
görsel olarak (kalın, yeşil, gerçek bir terminal gibi) AYRILAN bir `kod` slaytı kullanılır:

```js
{
  tur: 'kod',
  baslik: 'Bu Bölümde Kullandığımız Terminal Komutları',
  terminal: true,           // dosyaYolu YOK — bir kaynak dosyası değil, çalıştırılan komutlar
  kod: `$ npm create vite@latest admin -- --template react-ts
$ npm i axios formik yup`,
  satirlar: [
    {
      satir: '$ npm create vite@latest admin -- --template react-ts',
      aciklama: 'Bu komutun ne yaptığı.',
      neden: 'Neden bu komut / bu bayraklarla çalıştırıldı.',
      olmasaydi: 'Bu komut atlansaydı ne olurdu.'
    }
  ]
}
```

- Bir bölümde birden fazla ayrı terminal komutu grubu çalıştırıldıysa (ör. önce proje iskeleti,
  sonra kütüphaneler, sonra test araçları) HER biri ayrı bir `kavram` slaytına DAĞITILMAZ — tek
  bir `terminal: true` `kod` slaytında, her komut kendi satırı ve kendi tıklanabilir açıklamasıyla
  TOPLANIR. Bu slayt, o bölümün **"Bu Bölümde Öğrendiklerimiz" (`ozet`) slaytından HEMEN ÖNCE**
  yer alır.
- Her komut satırı gerçekten çalıştırıldığı haliyle (`$ ` öneki dahil) birebir yazılır — kısaltılmaz,
  uydurulmaz (backend'deki `kod` slaytının "gerçek dosyadan birebir" kuralıyla aynı disiplin).
- `dosyaYolu`/`nedenBuKlasor` alanları bu türde YAZILMAZ (bir kaynak dosyası değil).

## 4. Motor kopyası

`AKADEMI/admin/engine/` backend'in kopyasıdır (CLAUDE.md §6 — "engine'ler akademiler arası
paylaşılmaz"), TEK farkla: `RENDERERS` haritasında `postman: renderPostman` yerine
`onizleme: renderOnizleme` kayıtlı, `slides.css`'te `.slide-postman` bloğu `.slide-onizleme` +
`.onizleme-*` sınıflarıyla değiştirilmiş (API çağrısı kartı için `postman-method`/`postman-url`/
`postman-pre` gibi bazı düşük seviye sınıflar aynı isimle yeniden kullanılıyor). Genel bir motor
düzeltmesi (render bug fix) yapılırsa hem burada hem `AKADEMI/backend/engine/`'de uygulanır.
