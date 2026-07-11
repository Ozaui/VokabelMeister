# TASARIM SİSTEMİ (Admin Panel)

> Kapsam: yalnızca **Admin Panel** (`/admin`, Faz B). Web (`/web`, Faz D) ve Mobil (Faz E) kendi
> tasarım kararlarını ayrı alacak — bu doküman onlara otomatik uygulanmaz. B-01 (kurulum) henüz
> yapılmadığı için bu yalnızca bir **tasarım kararı**, henüz hiçbir Tailwind config'ine işlenmedi.

## 1. Renk Paleti — "Menekşe + Mercan"

| Rol | Hex | Kullanım |
|-----|-----|----------|
| Primary | `#6D5DFC` | Ana marka rengi, aktif nav linki, primary buton |
| Accent | `#FB923C` | Vurgular, rozet, ikincil CTA |
| Background | `#F8F7FC` | Sayfa arka planı (saf beyaz değil — hafif lavanta-beyaz) |
| Surface/Card | `#FFFFFF` | Kart/panel yüzeyi |
| Text | `#1E1B2E` | Ana metin (sıcak koyu, saf siyah değil) |
| Muted text | `#6B7280` | İkincil/yardımcı metin |
| Border | `#E9E5F5` | Kart/input kenarlığı |
| Success | `#10B981` | Başarı durumu |
| Warning | `#F59E0B` | Uyarı durumu |
| Destructive | `#DC2626` | Silme/tehlikeli aksiyon |

Light mode odaklı — dark mode şu an kapsam dışı (`Users.ThemePreference` özelliği eklendiğinde,
bkz. `wiki/Database/Auth_Domain.md`, admin panel de bu tercihi okuyabilir hale gelecek, ama bugün
admin panelin kendisi light-only tasarlanıyor).

## 2. Tipografi

- **Başlıklar:** Nunito (yuvarlak, sıcak karakter)
- **Gövde/tablo metni:** DM Sans veya Inter (veri yoğun tablolarda okunabilirlik önceliği)

## 3. Stil Kuralları

- **Radius:** kart 16px, buton/input 12px — hiçbir yerde keskin (0px) köşe yok.
- **Gölge:** tek katmanlı, yumuşak (ağır neumorphism/claymorphism değil — admin panelde performans
  ve okunabilirlik önceliği).
- **İkon kullanımı — sıkı kural:** yalnızca nav linki ve birincil aksiyon butonlarında (ekle/sil/
  düzenle/ara) işlevsel olduğu yerde ikon kullanılır. Dekoratif ikon yok. Bir görsel/fotoğraf
  bulunamadığında ikon türetme/elle çizme **yok** — nötr placeholder (gri/açık mor blok, baş harf
  rozeti) kullanılır. Kategori `Icon`/`Color` gibi veride zaten var olan alanlar gösterilir, geri
  kalan boşluk ikonla doldurulmaz.
  Not: veriye gerçekten karşılık gelen bir ikon/renk alanı olduğu, `docs/DATABASE_SCHEMA/Icerik.md`
  → `Categories.Icon`/`Categories.Color` ile doğrulandı (uydurma alan değil).
- **Durum/rol bilgisi** (aktif/donduran, admin/user, log seviyesi) renkle birlikte etiket metniyle
  de gösterilir — yalnızca renge güvenilmez.
- **Mobil uyumlu / responsive:** masaüstünde sidebar + geniş tablo; tablet/mobilde alt navigasyon
  veya hamburger menü, tablolar kart listesine döner (yatay scroll yok). Dokunma hedefi min 44×44px.

## 4. Genel Layout

- **Masaüstü:** sol sabit sidebar (Dashboard, Kelimeler, Kategoriler, Kullanıcılar, Moderasyon,
  Loglar, Ayarlar) + üst topbar (admin adı, çıkış).
- **Mobil:** sidebar yerine alt navigasyon/hamburger menü.

## 5. Ekranlar (B-01 → B-09, `docs/TASK/B_admin_panel.md`)

| # | Ekran | Sayfa(lar) |
|---|-------|-----------|
| B-02 | Auth (e-posta+şifre+OTP, Google/Apple yok) | LoginPage, OtpVerifyPage |
| B-03 | Kelime Yönetimi | WordListPage, WordFormModal |
| B-04 | Kategori Yönetimi (hiyerarşik ağaç) | CategoryTreePage, CategoryFormModal |
| B-05 | Kullanıcı Yönetimi | UserListPage, UserDetailPage |
| B-06 | İçerik Moderasyonu | ModerationPage |
| B-07 | İstatistik Paneli (ana sayfa) | DashboardPage |
| B-08 | Log Görüntüleme (3 sekme) | LogsPage |
| B-09 | SMTP Ayarları | SmtpSettingsPage |

Diğer agent'a verilen tam tasarım brief'i (ekran detayları + kısıtlar) bu paletle birebir aynıdır —
bu doküman o brief'in kalıcı, kod-tarafı referans halidir.
