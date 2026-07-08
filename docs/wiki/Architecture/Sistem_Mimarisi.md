# Sistem Mimarisi

**Özet:** VokabelMeister, tek bir .NET 9 Web API'ye bağlanan üç ayrı istemciden (React Native mobil, React web, React admin panel) oluşur; tüm veri MS SQL Server'da tutulur. İstemciler arasında kod paylaşımı yok, yalnızca API sözleşmesi ortak.
**Kütüphaneler:** ASP.NET Core (API) · React/Vite (web, admin) · React Native/Expo (mobil) · MS SQL Server
**Bağlantılar:** [[Backend_Katmanli_Mimari]] · [[Roller_ve_Erisim]] · [[API_Sozlesmesi]] · [[Veritabani_Semasi]]

## Genel Akış

```
React Native (/mobile) ─┐
React Web App (/web)    ─┼─ HTTPS/TLS 1.3 → .NET 9 Web API → MS SQL Server
React Admin (/admin)    ─┘
```

- **Mobil:** Google + Apple + e-posta + **QR tarayıcı** (web/masaüstü oturumunu onaylama). Token → Expo Secure Store.
- **Web:** Google + e-posta + **QR ile giriş** (Apple ileriye bırakıldı — bkz. [[Guvenlik_Politikalari]]). Token → localStorage.
- **Admin:** Yalnızca e-posta + şifre (sosyal giriş/QR yok). Yalnızca `Admin` rolü erişebilir.

Backend içi katman akışı: `Controllers → (MediatR) Command/Handler → Repositories` → detay [[Backend_Katmanli_Mimari]].

## Klasör Yapısı (hedef)

```
VokabelMeister/
├── docs/ · docs/wiki/ (bu bilgi grafiği)
├── backend/{WordLearner.API, .Application, .Infrastructure, .Domain, .Tests}
├── admin/   ← React + Vite (henüz yok, Faz B)
├── web/     ← React + Vite (henüz yok, Faz D)
├── mobile/  ← React Native Expo (henüz yok, Faz E)
└── WordLearner.sln
```

Şu an yalnızca `backend/` gerçekten var; `admin/`, `web/`, `mobile/` planlanan ama henüz oluşturulmamış klasörler (bkz. [[Gelistirme_Yol_Haritasi]]).

## Geliştirme Sırası (neden bu sıra?)

```
A) Admin Panel Backend → B) Admin Panel → C) Kullanıcı Backend → D) Web → E) Mobil → F) Test
```

Admin backend + panel önce çünkü gerçek kelime/kategori içeriği admin panelden girilmeden kullanıcı
tarafı anlamlı test edilemez. Web, mobilden önce çünkü tarayıcıda test döngüsü hızlı ve mobile referans olur.
