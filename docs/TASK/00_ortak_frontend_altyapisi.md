# FAZ 00 — Ortak Frontend Altyapısı (`frontend/` pnpm monorepo)

> **Neden bu faz var:** Admin (Faz B), Web (Faz C), Mobil (Faz D) ve Tanıtım Sitesi (Faz F) aynı
> button/input/card gibi component'leri ve aynı renk/radius/spacing token'larını paylaşsın diye
> kurulan tek seferlik ortak zemin. Hiçbir tek faza ait olmadığı için harf dizisinin (A/B/C/D/F)
> dışında/önünde `00` olarak numaralandırıldı. **Faz A'ya bağımlı değil** (paralel yürüyebilir),
> yalnızca **B-01'i bloklar** — B-01 bu fazda yazılan paketleri `workspace:*` ile tüketerek başlar.
> Detaylı mimari gerekçe → `REFERENCE/DESIGN_SYSTEM.md` (kod karşılığı notları) ve `CLAUDE.md §4.1`.

> **Kullanıcı kararları (2026-08-21):** (1) Admin+Web+Site component KODUNU literal paylaşır
> (`packages/ui-web`), Mobil (React Native, farklı runtime) yalnızca `packages/design-tokens`'ı
> tüketir ve aynı prop sözleşmesiyle (ortak TS interface'leri) kendi native component'lerini yazar.
> (2) Next.js sitesi de aynı `frontend/` monorepo'suna dahil. (3) İkon kütüphanesi: **Phosphor
> Icons** (`@phosphor-icons/react` + `phosphor-react-native`) — `lucide-react` yasağının yerine.
> (4) Frontend kodu kök dizin yerine `frontend/` şemsiye klasörü altında yaşar (`.NET backend/` ile
> simetrik, `.NET` çözüm dosyalarıyla karışmasın diye).
> **Ertelenen karar:** `DESIGN_SYSTEM.md` başındaki radius/gölge (§4/§5) ↔ `CLAUDE.md`'nin
> yasaklı-desen listesi çelişkisi bu fazda ÇÖZÜLMEDİ — `design-tokens`'a mevcut değerler
> "taslak/provisional" etiketiyle geçici yazılır, gerçek görsel karar B-01 başlarken ayrı bir
> konuşmada verilir.

## Klasör Ağacı

```
Zausel/
├── backend/                       (değişmez)
└── frontend/                      # pnpm workspace kökü
    ├── pnpm-workspace.yaml · package.json · tsconfig.base.json
    ├── packages/
    │   ├── design-tokens/          # @zausel/design-tokens
    │   └── ui-web/                 # @zausel/ui-web
    ├── admin/ · web/ · mobile/ · site/    (ilgili faz başlayınca açılır)
```

### 00-01 — Monorepo İskeleti ⬜
- [ ] `frontend/pnpm-workspace.yaml` (`packages: [admin, web, mobile, site, "packages/*"]`)
- [ ] `frontend/package.json` (`private`, `packageManager: pnpm@11.x`, `pnpm.overrides.react`/`react-dom` sabit sürüm — aynı React ağacı paylaşılmazsa `ui-web` "invalid hook call" hatası verir)
- [ ] `frontend/tsconfig.base.json` (strict, `jsx: react-jsx`, `moduleResolution: bundler`)
- [ ] Kök `.gitignore`'a frontend girdileri (`node_modules/`, `dist/`, `build/`, `.next/`, `.expo/`, `*.tsbuildinfo`, `coverage/`)
- [ ] ➜ **`AKADEMI/admin/B-01_kurulum/`ye işle** (B-01 başladığında, bkz. dosya başı Akademi notu)

### 00-02 — `packages/design-tokens` ⬜
**Referans:** `REFERENCE/DESIGN_SYSTEM.md §1-6` (renk/tipografi/radius/gölge/spacing — birebir transkripsiyon, radius/gölge "taslak" etiketiyle)
- [ ] `src/colors.ts` (light/dark hex değerleri), `src/typography.ts` (§3), `src/spacing.ts` (§6)
- [ ] `src/radius.ts`, `src/shadows.ts` — **provisional** olarak işaretli (yukarıdaki "ertelenen karar" notuna bkz.), her ikisi hem CSS string hem RN `StyleSheet`-uyumlu obje üretecek şekilde
- [ ] `src/contracts.ts` — paylaşılan prop interface'leri (`ButtonProps`, `InputProps`, `CardProps`, `BadgeProps`, `ModalProps`, `SkeletonProps`) — Mobil'in native component'leri de Faz D'de bu interface'leri implement edecek (derleyici seviyesinde sözleşme, bkz. dosya başı karar 1)
- [ ] `src/index.ts` (barrel — Mobil'in doğrudan tükettiği saf TS giriş noktası)
- [ ] `scripts/generate-css.ts` — `colors/radius/shadows.ts`'ten `dist/tailwind.css` (`@theme` bloğu) üretir, elle iki kez yazılmaz
- [ ] Birim test (vitest) — token tutarlılığı (ör. her renk geçerli hex, her radius pozitif sayı)
- [ ] ➜ **`AKADEMI/admin/B-01_kurulum/`ye işle**

### 00-03 — `packages/ui-web` ⬜
**Referans:** `CLAUDE.md §4` (component granülerliği), 00-02'deki `contracts.ts`
- [ ] `Button/` (`Button.tsx` + `Button.test.tsx` + `index.ts`) — `variant`(primary/secondary/ghost/destructive)/`size`(sm/md/lg)/`disabled`/`loading`/`icon?`/`iconPosition?`, `'use client'` (Next.js App Router sınırı)
- [ ] `Input/` — `label`/`error?`/`helperText?`/`disabled`/`leftIcon?`/`rightIcon?`, `'use client'`
- [ ] `Card/` — `variant`(default/flashcard)/`padding` (salt görsel, `'use client'` gerekmez)
- [ ] `Badge/` — `variant`(success/warning/destructive/neutral/accent)/`size` (salt görsel)
- [ ] `Modal/` — `open`/`onClose`/`size`/`title`, native `<dialog>` elementi (ayrı headless-UI bağımlılığı eklenmez — YAGNI), `'use client'`
- [ ] `Skeleton/` — `variant`(text/circle/rect)/`width`/`height`/`count?` (salt görsel) — **atlanmaz**: `CLAUDE.md §1`'deki "no skeleton loaders" yasaklı-desen maddesi bir EKSİKLİĞİ yasaklıyor, gerçek yükleme durumu ilk paketten itibaren var olmalı
- [ ] Her component `00-02`'deki ilgili `*Props` interface'ini implement eder (`Button.tsx: React.FC<ButtonProps>`)
- [ ] `package.json` — `react`/`react-dom` PEER dependency (bundled değil)
- [ ] ➜ **`AKADEMI/admin/B-01_kurulum/`ye işle**

### 00-04 — İkon Kütüphanesi Kararı ⬜
- [x] Karar: **Phosphor Icons** (`@phosphor-icons/react` Admin/Web/Site, `phosphor-react-native`+`react-native-svg` Mobil) — `lucide-react` yasağının (`CLAUDE.md §1`) yerine
- [ ] `CLAUDE.md §1`/`§4.1`, `DESIGN_SYSTEM.md §7/§8` güncellemesi (bkz. bu dosyaların 2026-08-21 notları) ✅ zaten işlendi
- [ ] Paketlerin ilgili app'lere eklenmesi — Admin/Web/Site B-01/C-0X/F-0X başladığında, Mobil D-0X başladığında (`TECHNICAL_SPECIFICATIONS.md §2`'deki kurulum komutları)

## Doküman Güncellemeleri (bu fazın parçası, kod yazmadan önce)
- [x] `CLAUDE.md` §1 (çelişki notu — ikon çözüldü, radius/gölge açık), §2 (yönlendirme satırı), §4.1 (ikon satırı)
- [x] `DESIGN_SYSTEM.md` başlık/§7/§8/§9 (kod karşılığı notları, Phosphor kararı, YAGNI güncellemesi)
- [x] `TASK/TASK.md` (Faz Haritası + İlerleme Durumu + not)
- [x] `TASK_B_admin_panel.md` (B-01 ön koşul notu)
- [x] `REFERENCE/DEVELOPMENT_SETUP.md` (pnpm komutları, klasör ağacı)
- [x] `REFERENCE/TECHNICAL_SPECIFICATIONS.md §2` (pnpm paket kurulum komutları)

## Akademi
`packages/*` için ayrı bir akademi katmanı **açılmaz** (spekülatif olur, bkz. `CLAUDE.md §6`).
design-tokens + ilk 6 component, **B-01 fiilen başladığında** `AKADEMI/admin/B-01_kurulum/` içinde
öğretilir (kod fiziksel olarak `frontend/packages/` altında yaşasa da, ilk yazan/ilk gerçek tüketen
task B-01'dir). Web (Faz C) veya Site (Faz F) bu component'leri sonradan yalnızca import ettiğinde
akademi tekrar anlatmaz, kısa bir çapraz-referans notu yeterli.

⬜ Başlanmadı · 🔄 Devam ediyor · ✅ Tamamlandı · ⛔ Engellendi
