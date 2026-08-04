# TASARIM SİSTEMİ (Admin + Web — ortak)

> **Kapsam (2026-08-05'ten itibaren):** Bu doküman artık **Admin Panel (`/admin`, Faz B) ve Web
> (`/web`, Faz D) için ortak, tek tasarım sistemidir.** Önceden ("Turkuaz + Mercan" dönemi) yalnızca
> Admin'i kapsıyordu, Web/Mobil kendi kararlarını ayrı alacaktı — bu ayrım kaldırıldı: iki frontend
> aynı renk/tipografi/radius token'larını kullanır, tek bir Tailwind `@theme` kaynağından türer.
> **Mobil (Faz E)** aynı renk/tipografi kararlarını native bileşenlere (React Native `StyleSheet`)
> uyarlar — piksel-birebir Tailwind eşleşmesi şart değil, ama token değerleri (hex/px) aynı kalır.

> **Not (2026-08-05 — Tasarım sistemi baştan yazıldı):** Önceki "Turkuaz + Mercan" paleti
> (Primary `#4E93BC`, Nunito+DM Sans, B-01'de zaten koda işlenmişti) tamamen değiştirildi. Karar
> süreci: kullanıcı Apple.com, Stripe.com (light) ile Anthropic.com, Cursor.com, Superhuman.com
> (dark) sitelerini referans göstererek "sade, göz yormayan, dark modda da yorucu olmayan" bir
> sistem istedi. Ortak payda: **saf siyah/beyaz zemin yok, tek aksan rengi, bol boşluk, düşük
> kontrastlı ayrım (gölge yerine sınır çizgisi)**. Aşağıdaki her karar bu prensipten türer. Bu not,
> önceki "Menekşe→Turkuaz" değişiklik notu gibi kalıcı bir karar kaydıdır — silinmez, üstüne yazılır.
> **Etkilenen kod:** `admin/src/index.css` (`@theme` + `.dark` override) B-01'de zaten yazılmıştı,
> bu doküman değiştiğinde o dosya da senkronize edilmeli (B-01'e "🔄 palet güncellemesi" notu
> düşülür — B-01 tekrar ⬜'e alınmaz, yalnızca bir alt-görev olarak palet token'ları güncellenir).

## 1. Renk Paleti — Light

| Token | Hex | Kullanım |
|---|---|---|
| `background` | `#FAFAFA` | Sayfa zemini (saf beyaz değil — hafif gri) |
| `surface` | `#FFFFFF` | Kart/panel/modal yüzeyi |
| `surface-muted` | `#F2F2F5` | İkincil yüzey (ör. tablo başlığı, disabled input zemini) |
| `text-primary` | `#1D1D1F` | Ana metin (saf siyah değil) |
| `text-secondary` | `#6E6E73` | Yardımcı/ikincil metin |
| `text-muted` | `#9A9AA0` | Placeholder, en düşük öncelikli metin |
| `border` | `#E5E5E5` | Kart/input/tablo kenarlığı |
| `accent` (primary) | `#5B54F0` | Tek aksan rengi — primary buton, aktif nav linki, link, focus ring |
| `accent-hover` | `#4A44D6` | Accent'in hover/active tonu (~%10 koyu) |
| `success` | `#10B981` | Başarı durumu |
| `warning` | `#F59E0B` | Uyarı durumu |
| `destructive` | `#DC2626` | Silme/tehlikeli aksiyon |

## 2. Renk Paleti — Dark

Dark mod, light modun ters çevrilmişi (invert) **değildir** — her token koyu zeminde okunabilirlik
gözetilerek ayrı ayarlanır (bkz. Anthropic/Cursor referansı: saf `#000` yerine sıcak/nötr koyu gri,
metin saf `#FFF` yerine kırık beyaz, accent rengi biraz açılıp parlaklığı artırılır).

| Token | Hex | Light karşılığı | Not |
|---|---|---|---|
| `background` | `#131217` | `#FAFAFA` | Saf siyah değil — hafif mor-gri ton |
| `surface` | `#1C1B22` | `#FFFFFF` | |
| `surface-muted` | `#242329` | `#F2F2F5` | |
| `text-primary` | `#F2F1F5` | `#1D1D1F` | Saf beyaz değil |
| `text-secondary` | `#A1A1AA` | `#6E6E73` | |
| `text-muted` | `#6E6E76` | `#9A9AA0` | |
| `border` | `#2A2A32` | `#E5E5E5` | |
| `accent` | `#8A83FF` | `#5B54F0` | Koyu zeminde kontrast için ~%20 daha açık/parlak |
| `accent-hover` | `#A29CFF` | `#4A44D6` | |
| `success` | `#34D399` | `#10B981` | |
| `warning` | `#FBBF24` | `#F59E0B` | |
| `destructive` | `#F87171` | `#DC2626` | |

**Kural:** accent rengi dark modda asla light moddakiyle birebir aynı hex değil — doygun bir renk
koyu zemin üzerinde göz yorar (bkz. Linear/Raycast eleştirisi), bu yüzden dark'ta hep biraz daha
açık/az doygun bir ton kullanılır. Uygulama: Tailwind v4 `@custom-variant dark (&:where(.dark, .dark *));`
+ `.dark { --color-*: ... }` token override'ı — utility class'lar (`bg-accent` vb.) değişmez, yalnızca
çözümledikleri değer değişir. Tercih üçlü (Light/Dark/System, `Users.ThemePreference`), `System`
seçiliyken OS tercihi canlı takip edilir (`useThemeSync.ts`). FOUC önlemi: `index.html`'de React
yüklenmeden önce çalışan senkron script (B-01'de zaten var, değişmedi).

## 3. Tipografi

- **Font ailesi:** **Inter** (Google Fonts, ücretsiz, değişken ağırlık) — tek font, Nunito/DM Sans
  ikilisi kaldırıldı. Alternatif/yedek: **Geist** (Vercel'in fontu, aynı aileden, aynı `@theme`
  değişkeniyle takas edilebilir).
- **Ağırlıklar:** yalnızca **400 (regular)**, **500 (medium)**, **600 (semibold)** kullanılır. 700+
  kullanılmaz — kalın ağırlıklar bu sistemde sert/agresif durur.
- **Ölçek — pazarlama/genel sayfa (Web'de içerik ağırlıklı ekranlar, ör. `HomePage` hero'su):**

  | Rol | Boyut | Ağırlık | Line-height |
  |---|---|---|---|
  | Display (hero) | 48–56px | 600 | 1.1 |
  | H1 | 32–40px | 600 | 1.15 |
  | H2 | 24–28px | 600 | 1.2 |
  | H3 | 18–20px | 500 | 1.3 |
  | Body | 16–18px | 400 | 1.6 |
  | Caption | 13–14px | 400 | 1.4 |

- **Ölçek — admin/veri yoğun ekran (tablo, form, dashboard — Admin panelin ve Web'in ayar/liste
  sayfalarının çoğu):** pazarlama ölçeğinden daha küçük, yoğun veriye uygun.

  | Rol | Boyut | Ağırlık | Line-height |
  |---|---|---|---|
  | Sayfa başlığı (H1) | 24–28px | 600 | 1.25 |
  | Bölüm başlığı (H2) | 18–20px | 600 | 1.3 |
  | Kart başlığı (H3) | 15–16px | 500 | 1.4 |
  | Gövde/tablo hücresi | 14px | 400 | 1.5 |
  | Yardımcı metin/etiket | 12–13px | 400 | 1.4 |

- Metin rengi asla saf `#000`/`#FFF` değil — §1/§2'deki `text-*` token'ları kullanılır.
- İstersen büyük hero başlıklarında (yalnızca Web pazarlama tipi ekranlarda, ör. giriş/landing)
  Anthropic tarzı sıcaklık için **tek** bir serif (**Source Serif 4**) eklenebilir; gövde metin yine
  Inter kalır. Admin panelde serif **kullanılmaz** (veri yoğun ekranda editoryal ton gereksiz).

## 4. Radius Skalası

Hiçbir yerde keskin (0px) köşe **yok**; hiçbir yerde de aşırı yuvarlak (claymorphism/neumorphism
tarzı >24px genel köşe) **yok**. Bileşen türüne göre sabit skala:

| Bileşen | Radius | Not |
|---|---|---|
| Input / Select / Textarea | `8px` | |
| Buton (tüm boyutlar) | `8px` | Pill (999px) yalnızca "chip/tag/badge" için, buton için değil |
| Checkbox | `4px` | Küçük kontrol, orantılı küçük radius |
| Küçük kart / liste satırı | `12px` | Ör. tablo satırı hover kartı, dropdown menü öğesi |
| Kart (genel — form, istatistik, panel) | `16px` | Varsayılan "kart" radius'u budur |
| Modal / Dialog | `20px` | Kart'tan bir tık daha yuvarlak — hiyerarşik olarak "üstte yüzen" his |
| Badge / Chip / Pill / Avatar (kare değilse) | `999px` (tam yuvarlak) | |
| Tooltip / Popover | `8px` | Input ile aynı — küçük, geçici UI |
| Toast/bildirim | `12px` | |

**Kural:** bir bileşenin radius'u üst kapsayıcısından (ör. kart içindeki buton) **her zaman küçük ya
da eşit** olmalı — kart 16px ise içindeki buton asla kartla aynı veya daha büyük radius'ta olamaz
(görsel olarak "taşar"). Yeni bir bileşen eklenirken bu tablo genişletilir, rastgele değer seçilmez.

## 5. Gölge (Shadow) Skalası

Referans sistemlerin (Apple/Stripe/Linear) ortak noktası: **derinlik gölgeyle değil, sınır çizgisiyle
(`border`) verilir.** Gölge yalnızca gerçekten "yüzen" (floating) elemanlarda, çok hafif kullanılır.

| Seviye | Değer | Kullanım |
|---|---|---|
| `shadow-none` | yok | Kart, buton, input — varsayılan. Ayrım `border` ile yapılır |
| `shadow-xs` | `0 1px 2px rgba(0,0,0,.04)` | Hover durumunda kart (opsiyonel, çok hafif) |
| `shadow-sm` | `0 2px 8px rgba(0,0,0,.06)` | Dropdown menü, popover |
| `shadow-md` | `0 8px 24px rgba(0,0,0,.10)` | Modal/Dialog, Toast |

Glow/neon efekti (renkli, yayılan gölge) **hiçbir yerde kullanılmaz** — Linear/Raycast'in dark
modunda beğenilmeyen nokta tam olarak buydu. Dark modda gölge rengi de siyaha değil, `rgba(0,0,0,.4)`
gibi daha yüksek opaklığa çekilir (koyu zeminde açık gölge görünmez, ama parlama da eklenmez).

## 6. Boşluk (Spacing) Skalası

8px taban birim, Tailwind'in varsayılan skalasıyla birebir örtüşür — özel bir değer icat edilmez:

`4 · 8 · 12 · 16 · 24 · 32 · 48 · 64 · 96 · 128` (px)

- Kart iç boşluğu (padding): `16–24px`
- Form alanları arası boşluk: `16px`
- Section (sayfa içi büyük blok) dikey boşluğu: `48–64px` (admin/dashboard), `96–140px` (Web
  pazarlama/hero tipi bölüm — Apple/Stripe referansındaki "bol nefes alanı")
- Sayfa maksimum içerik genişliği: admin `1440px` (geniş tablo), Web pazarlama içerik `1200px`

## 7. Diğer Stil Kuralları

- **İkon kullanımı:** yalnızca nav linki ve birincil aksiyon butonlarında (ekle/sil/düzenle/ara)
  işlevsel olduğu yerde ikon kullanılır. Dekoratif ikon yok. Görsel/fotoğraf bulunamadığında ikon
  türetme/elle çizme **yok** — nötr placeholder (gri blok, baş harf rozeti) kullanılır.
- **İkon kütüphanesi:** `lucide-react` (Admin/Web) — değişmedi.
- **Durum/rol bilgisi** (aktif/donduran, admin/user, log seviyesi) renkle birlikte etiket metniyle
  de gösterilir — yalnızca renge güvenilmez (kontrast erişilebilirliği).
- **Responsive:** masaüstünde sidebar + geniş tablo (Admin) / geniş içerik (Web); tablet/mobilde
  alt navigasyon veya hamburger menü, tablolar kart listesine döner (yatay scroll yok). Dokunma
  hedefi min 44×44px.
- **Geçiş kuralı (light↔dark):** yalnızca §1/§2'deki renk token'ları değişir; radius (§4), gölge
  (§5), spacing (§6) ve tipografi boyutları (§3) **sabit kalır** — dark mod ayrı bir layout değil,
  aynı layout'un renk katmanı.

## 8. Ekranlar

### Admin (B-01 → B-09, `docs/TASK/TASK_B_admin_panel.md`)

| # | Ekran | Sayfa(lar) |
|---|-------|-----------|
| B-02 | Auth (e-posta+şifre+OTP+QR, Google/Apple yok) | LoginPage, OtpVerifyPage, QrLoginPage |
| B-03 | Kelime Yönetimi | WordListPage, WordFormModal, WordPairingPage |
| B-04 | Kategori Yönetimi (hiyerarşik ağaç) | CategoryTreePage, CategoryFormModal |
| B-05 | Kullanıcı Yönetimi | UserListPage, UserDetailPage |
| B-06 | İçerik Moderasyonu | ModerationPage |
| B-07 | İstatistik Paneli (ana sayfa) | DashboardPage |
| B-08 | Log Görüntüleme (3 sekme) | LogsPage |
| B-09 | SMTP Ayarları | SmtpSettingsPage |

### Web (D-01 → D-12, `docs/TASK/TASK_D_web_app.md`)

Bkz. ilgili dosyadaki D-03 → D-12 başlıkları — aynı token setini kullanır, ayrı bir tabloya gerek yok.

Diğer agent'a verilen tam tasarım brief'i (ekran detayları + kısıtlar) bu paletle birebir aynıdır —
bu doküman o brief'in kalıcı, kod-tarafı referans halidir.
