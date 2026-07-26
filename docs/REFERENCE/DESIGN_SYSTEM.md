# TASARIM SİSTEMİ (Admin Panel)

> Kapsam: yalnızca **Admin Panel** (`/admin`, Faz B). Web (`/web`, Faz D) ve Mobil (Faz E) kendi
> tasarım kararlarını ayrı alacak — bu doküman onlara otomatik uygulanmaz. B-01'de (Kurulum, ✅
> tamamlandı) `admin/src/index.css`'teki Tailwind v4 `@theme`'e işlendi — burada yazılı her değer
> artık gerçek kodda karşılığı olan bir karar, salt tasarım niyeti değil.

## 1. Renk Paleti — "Turkuaz + Mercan"

> **Not (B-01, 2026-07-26):** Primary ilk tasarım kararında `#6D5DFC` ("Menekşe") idi — B-01
> uygulamasında admin panel gerçek tarayıcıda görülünce kullanıcı bu rengin fazla mavi kaçtığını
> belirtti. Bir dizi soft/mavi-olmayan aday (Dut Moru, Toz Gülü, Toprak Kiremidi, Adaçayı, Hardal)
> ve ardından tam bir HSL tayfı taranarak `hsl(202, 45%, 52%)` ("Turkuaz") seçildi — palet adı da
> buna göre güncellendi. Diğer renkler (Accent/Background/Surface/Text/Muted/Border/Success/
> Warning/Destructive) değişmedi, yalnızca Primary.

| Rol | Hex | Kullanım |
|-----|-----|----------|
| Primary | `#4E93BC` (`hsl(202, 45%, 52%)`) | Ana marka rengi, aktif nav linki, primary buton |
| Accent | `#FB923C` | Vurgular, rozet, ikincil CTA |
| Background | `#F8F7FC` | Sayfa arka planı (saf beyaz değil — hafif lavanta-beyaz) |
| Surface/Card | `#FFFFFF` | Kart/panel yüzeyi |
| Text | `#1E1B2E` | Ana metin (sıcak koyu, saf siyah değil) |
| Muted text | `#6B7280` | İkincil/yardımcı metin |
| Border | `#E9E5F5` | Kart/input kenarlığı |
| Success | `#10B981` | Başarı durumu |
| Warning | `#F59E0B` | Uyarı durumu |
| Destructive | `#DC2626` | Silme/tehlikeli aksiyon |

## 1b. Koyu Tema Paleti (Dark Mode)

> **Not (B-01, 2026-07-26):** İlk kararda dark mode "kapsam dışı" bırakılmıştı; kullanıcı B-01
> sırasında fikrini değiştirdi — `Users.ThemePreference` (A-03.3) zaten DB'de olduğu için backend'e
> hiç dokunmadan eklendi (yazma ucu hâlâ C-01'de, `themeSlice` şimdilik yalnızca `localStorage`).
> Aşağıdaki değerler yukarıdaki light paletin DOĞRUDAN tersi (invert) DEĞİL — her token, koyu
> zeminde okunabilirlik/kontrast gözetilerek AYRI ayarlandı (bkz. `AKADEMI/admin/B-01_kurulum/
> 06_dark-mode.html`).

| Rol | Hex | Light Karşılığı |
|-----|-----|-----------------|
| Primary | `#5FA3C9` | `#4E93BC` (koyu zeminde kontrast için biraz açıldı) |
| Accent | `#FDA65D` | `#FB923C` |
| Background | `#13161B` | `#F8F7FC` (saf siyah DEĞİL — hafif mavi-gri) |
| Surface/Card | `#1C2027` | `#FFFFFF` |
| Text | `#EDEFF3` | `#1E1B2E` (saf beyaz DEĞİL) |
| Muted text | `#8A93A3` | `#6B7280` |
| Border | `#2B303A` | `#E9E5F5` |
| Success | `#34D399` | `#10B981` |
| Warning | `#FBBF24` | `#F59E0B` |
| Destructive | `#F87171` | `#DC2626` |

Uygulama: Tailwind v4 `@custom-variant dark (&:where(.dark, .dark *));` + `.dark { --color-*: ... }`
token override'ı (`admin/src/index.css`) — utility class'ların kendisi (`bg-primary` vb.) hiç
değişmez, yalnızca çözümledikleri değer `.dark` kapsamında değişir. Tercih üç seçenekli
(Light/Dark/System, `Users.ThemePreference` ile aynı), `System` seçiliyken OS tercihi CANLI takip
edilir (`useThemeSync.ts`). FOUC önlemi: `index.html`'de React yüklenmeden önce çalışan senkron
bir script.

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
- **İkon kütüphanesi:** `lucide-react` (B-01, `ThemeSwitcher` — Sun/Moon/Monitor — ilk kullanım).
  İkonlar **elle SVG olarak çizilmez**, bu kütüphaneden import edilir — yukarıdaki "ikon türetme/
  elle çizme yok" kuralının somut karşılığı. Admin panelde ihtiyaç duyulan TÜM ikonlar (B-03'ten
  B-09'a) bu kütüphaneden seçilir, farklı bir ikon paketi eklenmez.
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
