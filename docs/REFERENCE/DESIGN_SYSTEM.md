# TASARIM SİSTEMİ (Admin + Web — ortak)

> **Kapsam (2026-08-05'ten itibaren):** Bu doküman artık **Admin Panel (`/admin`, Faz B) ve Web
> (`/web`, Faz C) için ortak, tek tasarım sistemidir.** Önceden ("Turkuaz + Mercan" dönemi) yalnızca
> Admin'i kapsıyordu, Web/Mobil kendi kararlarını ayrı alacaktı — bu ayrım kaldırıldı: iki frontend
> aynı renk/tipografi/radius token'larını kullanır, tek bir Tailwind `@theme` kaynağından türer.
> **Mobil (Faz D)** aynı renk/tipografi kararlarını native bileşenlere (React Native `StyleSheet`)
> uyarlar — piksel-birebir Tailwind eşleşmesi şart değil, ama token değerleri (hex/px) aynı kalır.

> **Not (2026-08-06 — Tasarım sistemi baştan yazıldı):** Önceki renk paleti tamamen değiştirildi. Karar
> süreci: kullanıcı Apple'ın modern, sade ve presizli estetiği ile Duolingo'nun samimi, neşeli ve
> kart sistemli öğrenme UX'ini birleştiren bir sistem istedi. Ortak payda: **saf siyah/beyaz zemin yok,
> ana aksan rengi Canlı Turuncu (`#FF6B00`), bol nefes alanı, yüksek yuvarlatılmış samimi kartlar
> ve düşük kontrastlı kenarlıklar/hafif süzülen gölgeler**. Aşağıdaki her karar bu prensipten türer. Bu not,
> önceki değişiklik notları gibi kalıcı bir karar kaydıdır — silinmez, üstüne yazılır.
> **Etkilenen kod:** `admin/src/index.css` (`@theme` + `.dark` override) B-01'de zaten yazılmıştı,
> bu doküman değiştiğinde o dosya da senkronize edilmeli (B-01'e "🔄 palet güncellemesi" notu
> düşülür — B-01 tekrar ⬜'e alınmaz, yalnızca bir alt-görev olarak palet token'ları güncellenir).

> **⚠️ Not (2026-08-16 — yasaklı desen listesiyle çelişki, ÇÖZÜLMEDİ):** `CLAUDE.md §1`'e
> eklenen "Görsel tasarım — yasaklı desenler" kuralı (Admin+Web+Mobil ortak, kapsam kullanıcı
> kararıyla netleşti) bu dosyadaki üç mevcut kararla çelişiyor: **§4** (radius skalası — "yumuşak/
> yuvarlak köşe, Squircle" felsefesi ↔ yasaklı "soft corner radius"), **§5** (Apple çizgisi için
> "hafif süzülen" gölgeler ↔ yasaklı "drop shadows") ve **§7/§8** (`lucide-react` ikon kütüphanesi
> ↔ yasaklı "lucide icons"). Bu üçü burada **kendiliğinden çözülmedi** — admin (`B-01`) yeniden
> yazılırken veya Web tasarımı somutlaşırken kullanıcıyla birlikte yeniden karara bağlanacak. O
> karara kadar §4/§5/§7/§8'deki değerler **taslak/referans** sayılır; kod bunları birebir
> uygulamaya başlamadan önce bu notun hâlâ güncel olup olmadığı kontrol edilmeli.

## 1. Renk Paleti — Light

| Token              | Hex       | Kullanım                                                                        |
| ------------------ | --------- | ------------------------------------------------------------------------------- |
| `background`       | `#FAFAFA` | Sayfa zemini (saf beyaz değil — hafif krem/gri)                                 |
| `surface`          | `#FFFFFF` | Kart/panel/modal yüzeyi                                                         |
| `surface-muted`    | `#F2F2F5` | İkincil yüzey (ör. tablo başlığı, disabled input zemini, rozet arka planı)      |
| `text-primary`     | `#1C1C1E` | Ana metin (saf siyah değil — Apple Deep Charcoal)                               |
| `text-secondary`   | `#8E8E93` | Yardımcı/ikincil metin, okunuşlar, dil bilgisi ipuçları                         |
| `text-muted`       | `#AEAEE2` | Placeholder, en düşük öncelikli metin                                           |
| `border`           | `#E5E5EA` | Kart/input/tablo kenarlığı                                                      |
| `accent` (primary) | `#FF6B00` | Tek aksan rengi — primary buton, aktif nav linki, streak ateşi, ilerleme çubuğu |
| `accent-hover`     | `#E05E00` | Accent'in hover/active tonu (~%10 koyu)                                         |
| `success`          | `#34C759` | Başarı durumu, doğru cevap (Apple Yeşili)                                       |
| `warning`          | `#FFCC00` | Uyarı durumu, XP/Puan vurgusu                                                   |
| `destructive`      | `#FF3B30` | Silme/tehlikeli aksiyon, yanlış cevap                                           |

## 2. Renk Paleti — Dark

Dark mod, light modun ters çevrilmişi (invert) **değildir** — her token koyu zeminde okunabilirlik
gözetilerek ayrı ayarlanır (saf `#000` yerine sıcak koyu gri, metin saf `#FFF` yerine kırık beyaz,
accent rengi koyu zeminde göz yormaması için hafif yumuşatılır).

| Token            | Hex       | Light karşılığı | Not                                             |
| ---------------- | --------- | --------------- | ----------------------------------------------- |
| `background`     | `#121214` | `#FAFAFA`       | Saf siyah değil — sıcak koyu füme ton           |
| `surface`        | `#1C1C1E` | `#FFFFFF`       |                                                 |
| `surface-muted`  | `#2C2C2E` | `#F2F2F5`       |                                                 |
| `text-primary`   | `#F2F1F5` | `#1C1C1E`       | Saf beyaz değil                                 |
| `text-secondary` | `#98989D` | `#8E8E93`       |                                                 |
| `text-muted`     | `#636366` | `#AEAEE2`       |                                                 |
| `border`         | `#2C2C2E` | `#E5E5EA`       |                                                 |
| `accent`         | `#FF7A1A` | `#FF6B00`       | Koyu zeminde kontrast için daha yumuşak turuncu |
| `accent-hover`   | `#FF8F3D` | `#E05E00`       |                                                 |
| `success`        | `#30D158` | `#34C759`       |                                                 |
| `warning`        | `#FFD60A` | `#FFCC00`       |                                                 |
| `destructive`    | `#FF453A` | `#FF3B30`       |                                                 |

**Kural:** accent rengi dark modda asla light moddakiyle birebir aynı hex değil — doygun bir renk
koyu zemin üzerinde göz yorar, bu yüzden dark'ta hep biraz daha açık/az doygun bir ton kullanılır.
Uygulama: Tailwind v4 `@custom-variant dark (&:where(.dark, .dark *));` + `.dark { --color-*: ... }`
token override'ı — utility class'lar (`bg-accent` vb.) değişmez, yalnızca çözümledikleri değer değişir.
Tercih üçlü (Light/Dark/System, `Users.ThemePreference`), `System` seçiliyken OS tercihi canlı takip edilir
(`useThemeSync.ts`). FOUC önlemi: `index.html`'de React yüklenmeden önce çalışan senkron script (B-01'de zaten var, değişmedi).

## 3. Tipografi

- **Font ailesi:** **Plus Jakarta Sans** (Google Fonts, ücretsiz, değişken ağırlık) — modern, yuvarlatılmış
  ve son derece okunabilir yapısıyla hem Apple presizyonunu hem Duolingo samimiyetini sağlar.
  Alternatif/yedek: **SF Pro** veya **Inter**.
- **Ağırlıklar:** **400 (regular)**, **500 (medium)**, **600 (semibold)** ve kart kelimelerinde **700 (bold)** kullanılır.
- **Ölçek — pazarlama/genel sayfa (Web'de içerik ağırlıklı ekranlar, ör. `HomePage` hero'su):**

  | Rol            | Boyut   | Ağırlık | Line-height |
  | -------------- | ------- | ------- | ----------- |
  | Display (hero) | 48–56px | 600     | 1.1         |
  | H1             | 32–40px | 600     | 1.15        |
  | H2             | 24–28px | 600     | 1.2         |
  | H3             | 18–20px | 500     | 1.3         |
  | Body           | 16–18px | 400     | 1.6         |
  | Caption        | 13–14px | 400     | 1.4         |

- **Ölçek — kelime öğrenme/kart ve admin ekranı (öğrenme kartları, veri yoğun ekranlar):** öğrenme
  odaklı kart sistemine ve yoğun veriye uygun.

  | Rol                        | Boyut   | Ağırlık | Line-height |
  | -------------------------- | ------- | ------- | ----------- |
  | Kart Kelime Başlığı        | 36–44px | 700     | 1.1         |
  | Sayfa başlığı (H1)         | 24–28px | 600     | 1.25        |
  | Bölüm başlığı (H2)         | 18–20px | 600     | 1.3         |
  | Kart başlığı / Okunuş (H3) | 15–16px | 500     | 1.4         |
  | Gövde / Örnek Cümle        | 14–15px | 400     | 1.5         |
  | Yardımcı metin / Kısayol   | 12–13px | 400     | 1.4         |

- Metin rengi asla saf `#000`/`#FFF` değil — §1/§2'deki `text-*` token'ları kullanılır.
- İstersen büyük hero başlıklarında (yalnızca Web pazarlama tipi ekranlarda, ör. giriş/landing)
  sıcaklık için **tek** bir serif (**Source Serif 4**) eklenebilir; gövde metin yine Plus Jakarta Sans kalır.
  Admin panelde ve kart modülünde serif **kullanılmaz**.

## 4. Radius Skalası

Hiçbir yerde keskin (0px) köşe **yok**. Duolingo samimiyetini yakalamak için kart ve etkileşim öğelerinde
cömert ve yumuşak kavisler (Squircle) tercih edilir. Bileşen türüne göre sabit skala:

| Bileşen                                        | Radius                 | Not                                                                   |
| ---------------------------------------------- | ---------------------- | --------------------------------------------------------------------- |
| Input / Select / Textarea                      | `12px`                 |                                                                       |
| Buton (tüm boyutlar)                           | `16px`                 | Pill (999px) yalnızca "chip/tag/badge" için, buton için değil         |
| Checkbox                                       | `4px`                  | Küçük kontrol, orantılı küçük radius                                  |
| Küçük kart / liste satırı / mini seçenek kartı | `16px`                 | Ör. quiz şık kartı, dropdown menü öğesi                               |
| Kart (genel — form, istatistik, panel)         | `20px`                 | Varsayılan genel kart radius'u                                        |
| **Öğrenme Kartı (Main Flashcard)**             | **`28px` - `32px`**    | **Odak Bileşeni: Samimi, dokunma hissi yüksek dev ana çalışma kartı** |
| Modal / Dialog                                 | `24px`                 | Kart'tan bir tık daha yuvarlak — hiyerarşik olarak "üstte yüzen" his  |
| Badge / Chip / Pill / Avatar (kare değilse)    | `999px` (tam yuvarlak) |                                                                       |
| Tooltip / Popover                              | `8px`                  | Küçük, geçici UI                                                      |
| Toast/bildirim                                 | `12px`                 |                                                                       |

**Kural:** bir bileşenin radius'u üst kapsayıcısından (ör. kart içindeki buton) **her zaman küçük ya
da eşit** olmalı — kart 28px ise içindeki buton asla kartla aynı veya daha büyük radius'ta olamaz
(görsel olarak "taşar"). Yeni bir bileşen eklenirken bu tablo genişletilir, rastgele değer seçilmez.

## 5. Gölge (Shadow) Skalası

Apple minimalist çizgisini korumak için **derinlik esas olarak kenarlık (`border`) ve hafif süzülme
hissi veren geniş gölgelerle verilir.** Sert, koyu gölgeler ve renkli neon parlamalar kullanılmaz.

| Seviye        | Değer                         | Kullanım                                                                 |
| ------------- | ----------------------------- | ------------------------------------------------------------------------ |
| `shadow-none` | yok                           | Düz kartlar, inputlar — varsayılan                                       |
| `shadow-xs`   | `0 2px 4px rgba(0,0,0,.03)`   | Hover durumunda buton/kart (çok hafif)                                   |
| `shadow-sm`   | `0 4px 12px rgba(0,0,0,.05)`  | Dropdown menü, popover, ikincil kartlar                                  |
| `shadow-card` | `0 10px 30px rgba(0,0,0,.04)` | **Ana Flashcard:** Ekranın ortasında hafif süzülen çalışma kartı gölgesi |
| `shadow-md`   | `0 12px 32px rgba(0,0,0,.08)` | Modal/Dialog, Toast                                                      |

Glow/neon efekti (renkli, yayılan gölge) **hiçbir yerde kullanılmaz**. Dark modda gölge rengi de siyaha değil,
`rgba(0,0,0,.4)` gibi daha yüksek opaklığa çekilir (koyu zeminde açık gölge görünmez, ama parlama da eklenmez).

## 6. Boşluk (Spacing) Skalası

8px taban birim, Tailwind'in varsayılan skalasıyla birebir örtüşür — özel bir değer icat edilmez:

`4 · 8 · 12 · 16 · 24 · 32 · 48 · 64 · 96 · 128` (px)

- Kart iç boşluğu (padding): `20–32px` (Öğrenme kartında nefes alan geniş alan)
- Form alanları arası boşluk: `16px`
- Section (sayfa içi büyük blok) dikey boşluğu: `48–64px` (admin/dashboard), `96–140px` (Web
  pazarlama/hero tipi bölüm — Apple referansındaki "bol nefes alanı")
- Sayfa maksimum içerik genişliği: admin `1440px` (geniş tablo), Web pazarlama içerik `1200px`,
  Öğrenme Kartı konteyneri max `480px` (ortalanmış tek odak alanı)

## 7. Yardımcı Kütüphaneler ve Bağımlılıklar

Tasarım sistemini ve kart mimarisindeki samimi etkileşimleri desteklemek için eklenen harici kütüphaneler:

| Kütüphane       | Kullanım Amacı                                                               |
| --------------- | ---------------------------------------------------------------------------- |
| ~~`lucide-react`~~ | ⚠️ YASAKLI (CLAUDE.md §1) — bkz. dosya başındaki 2026-08-16 uyarı notu, alternatif henüz seçilmedi |
| `framer-motion` | Kart çevirme (flip), yaylanma (`spring`) ve sarsılma (`shake`) animasyonları |
| `use-sound`     | Kart çevirme, buton tıklama ve doğru/yanlış geribildirim ses efektleri       |

## 8. Diğer Stil Kuralları

- **İkon kullanımı:** yalnızca nav linki, ses oynatma butonları ve birincil aksiyon butonlarında (ekle/sil/düzenle/ara)
  işlevsel olduğu yerde ikon kullanılır. Dekoratif ikon yok. Görsel/fotoğraf bulunamadığında ikon
  türetme/elle çizme **yok** — nötr placeholder (gri blok, baş harf rozeti) kullanılır.
- **İkon kütüphanesi:** ~~`lucide-react`~~ ⚠️ yasaklandı (bkz. dosya başındaki 2026-08-16 uyarı notu) — alternatif henüz seçilmedi.
- **Durum/rol bilgisi** (aktif/donduran, admin/user, log seviyesi) renkle birlikte etiket metniyle
  de gösterilir — yalnızca renge güvenilmez (kontrast erişilebilirliği).
- **Responsive:** masaüstünde sidebar + geniş tablo (Admin) / geniş içerik ve ortalanmış ana çalışma kartı (Web);
  tablet/mobilde alt navigasyon veya hamburger menü, tablolar kart listesine döner. Dokunma hedefi min 44×44px.
- **Geçiş kuralı (light↔dark):** yalnızca §1/§2'deki renk token'ları değişir; radius (§4), gölge
  (§5), spacing (§6) ve tipografi boyutları (§3) **sabit kalır** — dark mod ayrı bir layout değil,
  aynı layout'un renk katmanı.

## 9. Ekranlar

### Admin (B-01 → B-09, `docs/TASK/TASK_B_admin_panel.md`)

| #    | Ekran                                         | Sayfa(lar)                                   |
| ---- | --------------------------------------------- | -------------------------------------------- |
| B-02 | Auth (e-posta+şifre+OTP+QR, Google/Apple yok) | LoginPage, OtpVerifyPage, QrLoginPage        |
| B-03 | Kelime Yönetimi                               | WordListPage, WordFormModal, WordPairingPage |
| B-04 | Kategori Yönetimi (hiyerarşik ağaç)           | CategoryTreePage, CategoryFormModal          |
| B-05 | Kullanıcı Yönetimi                            | UserListPage, UserDetailPage                 |
| B-06 | İçerik Moderasyonu                            | ModerationPage                               |
| B-07 | İstatistik Paneli (ana sayfa)                 | DashboardPage                                |
| B-08 | Log Görüntüleme (3 sekme)                     | LogsPage                                     |
| B-09 | SMTP Ayarları                                 | SmtpSettingsPage                             |

### Web (C-01 → C-12, `docs/TASK/TASK_C_web_app.md`)

Bkz. ilgili dosyadaki C-03 → C-12 başlıkları — aynı token setini kullanır, ayrı bir tabloya gerek yok.

Diğer agent'a verilen tam tasarım brief'i (ekran detayları + kısıtlar) bu paletle birebir aynıdır —
bu doküman o brief'in kalıcı, kod-tarafı referans halidir.
