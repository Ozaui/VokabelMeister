# FAZ F — Tanıtım Sitesi (`/site`)

> **Kapsam:** Ürünün kendisinden (Faz C/D — oturum açmış kullanıcı akışı) tamamen ayrı, **anonim/
> public bir pazarlama sitesi**. Giriş/kayıt YOK, kullanıcı hesabı/veri işlemez — SRS/kart/kategori
> gibi hiçbir domain mantığı içermez. Amaç: projeyi tanıtmak (Duolingo ve benzeri dil öğrenme
> uygulamalarının pazarlama siteleri minvalinde) ve **Google Play Console / Apple App Store
> Connect'in mağaza başvurusunda zorunlu kıldığı public URL'leri** (Gizlilik Politikası, Kullanım
> Şartları, Hesap/Veri Silme Talimatı, Destek) barındırmak.
>
> **Teknoloji (2026-08-16 kullanıcı kararı — Admin/Web'den FARKLI):** **Next.js** (App Router,
> SSR/SSG) — pazarlama sitesi için SEO ve ilk yükleme hızı önemli, mevcut Admin/Web SPA yapısı
> (Vite+React, client-render) buna uygun değil. Bu yüzden `CLAUDE.md` §4.1'deki Admin/Web/Mobil
> ortak kütüphane seti (Redux Toolkit/Formik+Yup/axios/React Router) **bu faza uygulanmaz** — site
> girişte hesap yok, çoğu sayfa statik içerik, global state/form kütüphanesine gerek yok. `CLAUDE.md`
> §1 (dil kuralları, yasaklı görsel tasarım desenleri) ve §6 (Kod Akademisi kuralı) **değişmeden
> geçerli**.
>
> **Yeni/duruma-özel kütüphaneler (F-01'de eklenir):**
> - Animasyon **oynatma** (runtime): `lottie-react` veya `@lottiefiles/dotlottie-react` — [LottieFiles](https://lottiefiles.com)
>   tarzı JSON tabanlı vektör animasyonlarını sitede oynatır (hero, boş durum, CTA çevresi).
> - Animasyon **üretimi** (design-time): [Lottie Creator MCP](https://docs.lottiefiles.com/en/creator/13_ai-tools/lottie-creator-mcp)
>   — `claude mcp add lottiefiles-creator -- npx -y @lottiefiles/creator-mcp@latest` ile Claude'a
>   bağlanan resmi LottieFiles entegrasyonu, Claude'un Creator API üzerinden layer/keyframe
>   seviyesinde gerçek `.json` animasyon dosyası üretmesini/düzenlemesini sağlar (F-02.2). Dribbble/
>   LottieFiles galerisi yalnızca **ilham/stil referansı** — birebir varlık kopyalanmaz, marka
>   karakterine özgü orijinal animasyon üretilir (bkz. F-02.1).
> - Stil: TailwindCSS (Admin/Web ile aynı araç, ayrı `@theme` — bu sitenin kendi tasarım ihtiyacı,
>   bkz. F-02).
> - i18n: proje geneli hedef kitle DE↔TR (`CLAUDE.md` §1) — bu site de en az **tr/de** iki dilde.
>   Kütüphane (`next-intl` vb.) F-01'de seçilir.
>
> ⚠️ **Açık karar (henüz verilmedi):** Hosting/domain (`vokabelmeister.com` kökü mü, ayrı bir alan
> adı mı — `ENV.md`'deki `api.vokabelmeister.com` ile çakışmayacak şekilde). F-10'da, Faz E'nin
> deployment kararlarıyla birlikte netleşecek.

### F-01 — Kurulum ⬜
- [ ] Next.js + TS + Tailwind kurulumu (App Router, SSG tercih edilir — sayfaların büyük çoğunluğu
      derleme zamanında sabit, kullanıcıya özel veri yok)
- [ ] Lottie kütüphanesi (`lottie-react` veya `@lottiefiles/dotlottie-react`) + örnek bir animasyon
      dosyasıyla duman testi
- [ ] i18n kurulumu (tr/de, varsayılan tr — `ErrorMessages`/admin `languageSlice` ile aynı
      "desteklenmiyorsa tr'ye düş" ilkesi, `CLAUDE.md` §1)
- [ ] Temel layout (`Header`/`Footer` — nav linkleri, dil değiştirici, "Uygulamayı İndir" CTA'sı App
      Store/Play Store linkleri için yer tutucu, mağaza yayını netleşince doldurulur)
*(Kurulum task'ı — dikey dilim/roadmap kuralı burada uygulanmaz.)*

### F-02 — Tasarım Sistemi Uygulaması ⬜
**Referans:** REFERENCE/DESIGN_SYSTEM.md §3 ("pazarlama/genel sayfa" tipografi ölçeği — Display/H1-H3/
Body/Caption bu sitenin hero/başlık hiyerarşisidir), CLAUDE.md §1 (yasaklı görsel desenler)
- [ ] Renk/tipografi token'ları — Admin/Web ile **aynı marka rengi** (bkz. `DESIGN_SYSTEM.md` §1/§2,
      ⚠️ o dosyadaki 2026-08-16 çelişki notu çözülene kadar radius/gölge değerleri taslak sayılır)
      Tailwind `@theme`'e işlenir; bu sitenin kendi ayrı `tailwind.config`/`globals.css`'i olur
- [ ] Yasaklı desen kontrolü (CLAUDE.md §1) her sayfa yazıldığında elle doğrulanır — özellikle bu
      site "pazarlama sitesi" olduğu için listedeki bento grid/3'lü feature card/fake testimonial/
      3'lü pricing tier/terminal window gibi maddelere düşme riski en yüksek yer burası
- [ ] ➜ **Site Akademi'ye işle**

### F-02.1 — Marka Karakteri (Maskot) Tasarımı 🔄 ⚠️ **[2026-08-16 — yeni task, kullanıcı isteği]**
> Kod task'ı değil, **birlikte alınacak bir tasarım kararı** — Duolingo'nun baykuşu gibi, sitenin
> her yerinde (hero, boş durumlar, CTA, hata sayfaları) tekrar eden, tek ve tanınabilir bir marka
> karakteri. Bu adımın çıktısı sonraki her animasyon görevinin (F-02.2, F-03, F-04.x) girdisidir —
> karaktersiz Lottie üretimine geçilmez.
- [x] Karakter kimliği — **İsim: Zausel** (Almanca "zausig" = tüyü dağınık kökünden türetilmiş,
      gerçek bir Alman efsanesine/bölgesine bağlı olmayan özgün isim — Rübezahl gibi belirli bir
      efsane bilinçli olarak elenmişti, çünkü Silezya/II. Dünya Savaşı sonrası Alman-Polonya
      tarihiyle yüklü bir bagaj taşıyor; Wolpertinger/Tatzelwurm/Kobold yönleri de tür/ölçek
      uyuşmazlığı yüzünden elendi). **Domain:** zausel.com (2026-08-16 WHOIS'te boş çıktı, henüz
      kayıt edilmedi — teyit gerekir). İsim ayrıca DE/TR/EN arasında tutarlı okunacak şekilde
      seçildi (Duolingo/Babbel/Memrise emsali: 5-7 harf, azami 2 hece — 4 harf/tek heceli
      neredeyse tüm `.com`'lar zaten domain avcıları tarafından alınmış durumda). **Tür:** özgün,
      Alman dağ-ruhu/yeti folklor ailesinden esinlenilmiş yaratık, belirli bir efsaneye bağlı
      DEĞİL. **Kişilik:** enerjik.
- [x] Görsel stil kararı — Flat/vektör illüstrasyon, degrade/drop-shadow/stroke YOK (CLAUDE.md §1
      uyumlu). **Renk paleti:** ana turuncu `#F2661D`, karın/krem `#F5E6D3`, tüy tutamı gölge tonu
      `#C94E0C`, kontur/göz bebeği `#2B1B12`, göz akı `#FFF8EF`. **Vücut formu:** yuvarlak/tombul
      ama kafa ve gövde ayrı, tanınabilir iki form (aralarında boyun-gölgesi katmanıyla ayrım) —
      tek-blob "gövde=kafa" yaklaşımı (ilk taslakta denendi) kullanıcı isteğiyle terk edildi.
      **İmza detaylar:** baştaki tek yana yatık tüy tutamı + devasa yuvarlak yeti ayakları.
      **Gözler:** iri, yuvarlak.
  - ⚠️ **Açık alt kararlar** (2026-08-16, diğer bir AI'ın ürettiği ilk Lottie taslağı
    [`zausel_front/side/back.json`] incelendikten sonra tespit edildi, henüz kullanıcıyla
    kapatılmadı): (1) gülümseme iki diş gösteriyor, katman adı "Mischievous Smile" — seçilen
    "enerjik" kişilikle çelişebilecek "muzip/yaramaz" bir ifadeye kaymış; (2) kafa genişliği
    (265px) gövde genişliğine (270px) neredeyse eşit, oran gözden geçirilmeli; (3) arka
    görünümde brief dışı ek bir "Back Fur Tuft" (bel tüyü) katmanı var; (4) brief'te olmayan
    kaşlar eklenmiş. Bu dört madde kapanmadan görsel stil kararı NİHAİ sayılmaz.
- [ ] Poz/duygu varyasyon listesi — henüz kararlaştırılmadı (F-02.2'nin animasyon üretimi için
      gerekli: hero'da "karşılama", boş durumda "düşünceli", CTA'da "kutlama/teşvik", 404'te
      "şaşkın" gibi). Şu ana kadar yalnızca statik duruş (ön/yan/arka görünüm) üretildi, duygu/poz
      seti değil.
- [ ] ➜ **Site Akademi'ye işle** (tasarım kararı olarak — `kavram` slaydı, kod değil; yukarıdaki
      açık alt kararlar ve poz/duygu listesi kapanmadan işlenmez)

### F-02.2 — Lottie Creator MCP Bağlantısı + Karakter Animasyonu ⬜ ⚠️ **[2026-08-16 — yeni task, kullanıcı isteği]**
**Bağımlılık:** F-02.1 (karakter tasarımı bitmeden animasyon üretimine başlanmaz)
> **Kurulum (kullanıcı kendi LottieFiles hesabıyla):** `claude mcp add lottiefiles-creator --
> npx -y @lottiefiles/creator-mcp@latest` (bkz. [Lottie Creator MCP dokümantasyonu](https://docs.lottiefiles.com/en/creator/13_ai-tools/lottie-creator-mcp)).
> Bağlandıktan sonra Claude, LottieFiles Creator API'sine layer/keyframe seviyesinde erişip F-02.1'deki
> karakteri **adım adım** (önce statik illüstrasyon → sonra keyframe/easing → sonra varyasyon)
> birlikte inşa eder — bu, F-01'deki `lottie-react`/`dotlottie-react`'in yalnızca OYNATTIĞI `.json`
> dosyalarının asıl ÜRETİM adımıdır.
- [ ] MCP bağlantısı doğrulanır (basit bir "loading spinner" duman testiyle)
- [ ] Karakterin temel/karşılama animasyonu (F-03 hero'da kullanılacak — F-02.1'deki "karşılama" pozu)
- [ ] Kalan poz varyasyonlarının animasyonu (F-02.1 listesindeki diğer durumlar — boş durum/CTA/404
      ihtiyacı doğdukça, ilgili sayfanın task'ında değil burada toplu üretilir)
- [ ] Üretilen `.json` dosyaları `site/public/animations/`'a eklenir, her birinin hangi sayfada/
      component'te kullanılacağı **Site Akademi'ye işlenirken** not düşülür
- [ ] ➜ **Site Akademi'ye işle**

### F-03 — Ana Sayfa ⬜
**Bağımlılık:** F-02.2 (hero animasyonu için karşılama pozu hazır olmalı)
- [ ] **İçerik:** Hero (başlık + alt başlık + F-02.2'nin karşılama animasyonu + "Uygulamayı İndir"
      CTA'ları), öne çıkan özellikler özeti (gerçek ürün ekran görüntüleri/kısa ekran kaydı ile —
      CLAUDE.md §1 "no real product demos" yasağı gereği jenerik mockup/stok görsel KULLANILMAZ,
      her özelliğin F-04.x deep-dive sayfasına link), Almanca↔Türkçe çift yönlü öğrenme vurgusu,
      Goethe-Zertifikat sınavlarına hazırlık vurgusu (bkz. F-04.2), alt navigasyon (Özellikler/
      Gizlilik/Şartlar/Destek)
- [ ] **Component:** `HeroSection`, `LottieAnimation` (yeniden kullanılan sarmalayıcı — `reducedMotion`
      erişilebilirlik tercihine saygı gösterir), `FeatureHighlight`, `DownloadCta`
- [ ] **Route:** `/` (`/de` locale prefix'i i18n kararına göre)
- [ ] ➜ **Site Akademi'ye işle**

### F-04 — Özellikler (Genel Bakış) ⬜
> **Neden tek sayfa DEĞİL:** Özellikler, sitenin en önemli satış argümanı — tek bir sayfaya
> sıkıştırılmış kısa maddeler yerine, her önemli özellik **kendi ayrıntılı, adım adım anlatan
> sayfasına** sahip (F-04.1…F-04.5, aşağıda). Bu sayfa yalnızca kısa bir özet + o alt sayfalara
> giden linkler içerir — CLAUDE.md §1 gereği "3 feature cards in a row"/bento grid gibi sıkıştırılmış
> jenerik özet ızgarasına DÜŞÜLMEZ, düz/okunabilir bir liste düzeni kullanılır.
- [ ] **İçerik:** 5 özelliğin (F-04.1…F-04.5) kısa özeti + her biri kendi sayfasına link
- [ ] **Component:** `FeatureOverviewList` (F-04.x sayfalarına link veren liste öğeleri — kart
      IZGARASI değil, tek sütun/okunabilir liste)
- [ ] **Route:** `/features` (`/ozellikler`)
- [ ] ➜ **Site Akademi'ye işle**

### F-04.1 — Özellik Detayı: Aralıklı Tekrar Sistemi (SRS) ⬜
**Referans:** DATABASE_SCHEMA/SRS.md, A-09 (`A_backend.md`)
> Ürünün çekirdek mekanizması — "neden unutmuyorsun" sorusuna **adım adım** cevap verir, genel
> geçer "akıllı algoritma" lafı YETERSİZ, gerçek mekanizma anlatılır (uydurma sayı/istatistik yok).
- [ ] **İçerik:** Adım adım akış — (1) yeni kelimeyi öğren, (2) sistem unutma eğrine göre bir
      sonraki tekrar zamanını hesaplar (`UserProgress`/`UserCardProgress`, `CurrentLevel` 0-5 —
      A-09), (3) zamanı gelince hatırlatma, (4) doğru/yanlış cevaba göre bant güncellenir (🔴/🟡/🟢
      Zayıf/Orta/İyi, `Mastery` yüzdesi), (5) 5 ardışık yanlışta "leech" aksiyonu (askıya al/sıfırla)
      — gerçek üründeki karşılığı Faz C-05/C-11
- [ ] **Component:** `StepByStepExplainer` (yeniden kullanılan, numaralı adım listesi — F-04.2…
      F-04.5'te de kullanılır), varsa küçük bir Lottie döngüsü (kart çevirme/tekrar döngüsünü
      temsil eden, F-02.2'de üretilir)
- [ ] **Route:** `/features/srs` (`/ozellikler/tekrar-sistemi`)
- [ ] ➜ **Site Akademi'ye işle**

### F-04.2 — Özellik Detayı: CEFR Seviyeleri & Goethe-Zertifikat Uyumu ⬜
**Referans:** DATABASE_SCHEMA/Auth.md (`CurrentLevel`), DATABASE_SCHEMA/Icerik.md (`DifficultyLevel`)
> Gerçek veri temeli: kullanıcı seviyesi VE her kelime **A1-C2 CEFR** ölçeğinde tutuluyor (`CHECK`
> kısıtı — `Auth.md`/`Icerik.md`). Goethe-Institut'un Goethe-Zertifikat sınavları da BİREBİR aynı
> A1-C2 adlandırmasını kullanıyor — bu doğru ve dürüst bir pazarlama açısı (uydurma "sınav garantisi"
> iddiası YOK, yalnızca içerik yapısının sınav seviyeleriyle örtüştüğü anlatılır).
- [ ] **İçerik:** Adım adım — (1) kayıtta/onboarding'de seviye seçimi (`LevelSelectPage`, C-03),
      (2) kelime ve içerik o seviyeye göre filtrelenir (`GET /words?level=`), (3) seviye ilerledikçe
      daha üst CEFR kelimeleri açılır — Goethe-Zertifikat A1/A2/B1/B2/C1/C2 sınavlarına hazırlananlar
      için "hangi seviyedeyim, sınavdan önce nereye gelmem gerekiyor" netliği
- [ ] **Component:** `StepByStepExplainer` (F-04.1 ile ortak), `CefrLevelBadgeRow` (A1-C2 rozet
      şeridi — sistemin gerçek seviye etiketleriyle birebir)
- [ ] **Route:** `/features/goethe-sinavi-hazirlik`
- [ ] ➜ **Site Akademi'ye işle**

### F-04.3 — Özellik Detayı: Kart Sistemi (Sistem + Kişisel) ⬜
**Referans:** DATABASE_SCHEMA/Icerik.md, DATABASE_SCHEMA/Kisisel_Icerik.md, A-05/A-10 (`A_backend.md`)
- [ ] **İçerik:** Adım adım — sistem kelimesi kartının zengin gramer verisi (artikel/cinsiyet rengi,
      4 hâl, çoğul, fiil çekimi, IPA — `GERMAN_LANGUAGE_FEATURES.md`) vs. kullanıcının kendi kişisel
      kartını (ön yüz/arka yüz düz metin) eklemesi — ikisi de aynı SRS döngüsüne (F-04.1) girer
- [ ] **Component:** `StepByStepExplainer`, gerçek `SystemWordCard`/`PersonalCard` görselinin
      (Faz C-04) ekran görüntüsü/kısa GIF'i — CLAUDE.md §1 "no real product demos" yasağı gereği
      TASLAK/mockup değil gerçek render kullanılır
- [ ] **Route:** `/features/kart-sistemi`
- [ ] ➜ **Site Akademi'ye işle**

### F-04.4 — Özellik Detayı: Sosyal Öğrenme (Sınıf/Arkadaş/Paylaşım) ⬜
**Referans:** A-14/A-15/A-16 (`A_backend.md`)
- [ ] **İçerik:** Adım adım — sınıf oluşturup davet kodu paylaşma (öğretmen/grup senaryosu — ama
      CLAUDE.md §1 rol kuralı gereği "Instructor" rolü YOK, sınıf sahibi de bir `User`), arkadaş
      ekleyip ilerleme karşılaştırma, tek tıkla kelime listesi paylaşım linki
- [ ] **Component:** `StepByStepExplainer`, gerçek `ClassDetailPage`/`FriendsPage` (Faz C-08/C-09)
      ekran görüntüsü
- [ ] **Route:** `/features/sosyal-ogrenme`
- [ ] ➜ **Site Akademi'ye işle**

### F-04.5 — Özellik Detayı: İlerleme Takibi & Rozetler ⬜
**Referans:** DATABASE_SCHEMA/SRS.md (`Achievements`/`UserAchievements`), A-09 (`A_backend.md`)
- [ ] **İçerik:** Adım adım — 🔴/🟡/🟢 bant görünümü, "İncele"/"Sına" ayrımı, geçmiş oturumlar,
      rozet/başarı sistemi (`Rarity` renk kodu) — gerçek `ProgressPage`/`AchievementsSection`
      (Faz C-11) karşılığına referansla
- [ ] **Component:** `StepByStepExplainer`, gerçek `AchievementBadge` görselinin ekran görüntüsü
- [ ] **Route:** `/features/ilerleme-ve-rozetler`
- [ ] ➜ **Site Akademi'ye işle**

### F-05 — Gizlilik Politikası ⬜ *(Apple/Google mağaza başvurusu zorunlu alanı)*
**Referans:** REFERENCE/SECURITY.md (toplanan veri/şifreleme/hash kararları), REFERENCE/ENV.md
> İçerik hukuki metin değil, üründeki GERÇEK veri işleme kararlarının açıklamasıdır — hangi veri
> toplanıyor (e-posta, şifre hash'i, cihaz/oturum bilgisi, öğrenme istatistiği), nasıl saklanıyor
> (bcrypt, SHA-256 e-posta hash — `CLAUDE.md` §1), üçüncü taraf paylaşımı (Google/Apple OAuth
> girişi varsa), saklama süresi, kullanıcı hakları (erişim/silme). Faz A/backend'de karar
> değişirse (ör. yeni bir üçüncü taraf servis eklenirse) bu sayfa da güncellenir — tek doğruluk
> kaynağı `SECURITY.md`, burada kopyalanmaz, ondan türetilir.
- [ ] **İçerik:** Gizlilik Politikası metni (tr/de)
- [ ] **Route:** `/privacy` (`/gizlilik-politikasi`)
- [ ] ➜ **Site Akademi'ye işle**

### F-06 — Kullanım Şartları ⬜ *(Apple/Google mağaza başvurusu — özellikle özel EULA gerekiyorsa zorunlu)*
- [ ] **İçerik:** Kullanım Şartları/Hizmet Şartları metni (tr/de) — hesap sorumluluğu, içerik
      kuralları (paylaşım linki/sınıf özelliğindeki kullanıcı üretimi içerik), fesih koşulları
- [ ] **Route:** `/terms` (`/kullanim-sartlari`)
- [ ] ➜ **Site Akademi'ye işle**

### F-07 — Hesap/Veri Silme Talimatı ⬜ *(Google Play Data Safety zorunlu public URL)*
**Referans:** REFERENCE/API_ENDPOINTS.md §4 (`requestAccountDeletion`/`confirmAccountDeletion`, C-12)
> Google Play, hesap oluşturmaya izin veren her uygulamanın **uygulamayı indirmeden erişilebilen**
> bir "hesap ve veri silme" sayfası istiyor — silme işlemi zaten UYGULAMA İÇİNDE var (Faz C-12
> `DeleteAccountModal`, OTP onaylı); bu sayfa o adımları görsel/metinle anlatan bir **talimat**
> sayfası, ayrı bir silme mekanizması DEĞİL.
- [ ] **İçerik:** Adım adım "hesabını nasıl silersin" anlatımı (uygulama içi OTP onaylı akışın
      ekran görüntüleriyle), silinen veri kapsamı (`SECURITY.md`/`DATABASE_SCHEMA` anonimleştirme
      kararına referans), destek yoluyla talep seçeneği (uygulamaya erişemeyen kullanıcı için)
- [ ] **Route:** `/delete-account` (`/hesap-silme`)
- [ ] ➜ **Site Akademi'ye işle**

### F-08 — Destek/İletişim ⬜ *(Apple App Store Connect zorunlu "Support URL" alanı)*
- [ ] **İçerik:** SSS (kurulum/giriş/hesap sorunları), destek e-postası/iletişim bilgisi
- [ ] **Component:** `FaqAccordion`, `ContactInfo` — form varsa (ör. basit "bize yazın") native
      HTML form + `mailto:`/basit bir form servisi yeterli, Formik/axios gerekmez (bu site
      genelinde geçerli YAGNI kararı, bkz. dosya başı not)
- [ ] **Route:** `/support` (`/destek`)
- [ ] ➜ **Site Akademi'ye işle**

### F-09 — SEO & Meta Altyapısı ⬜
- [ ] Open Graph/Twitter Card meta etiketleri (her sayfa için başlık/açıklama/görsel), `sitemap.xml`,
      `robots.txt`, favicon/app icon seti, `next/image` ile görsel optimizasyonu
- [ ] Yapısal veri (JSON-LD `SoftwareApplication`/`Organization`) — arama sonuçlarında zengin snippet
- [ ] ➜ **Site Akademi'ye işle**

### F-10 — Yayına Alma ⬜
**Referans:** TASK/E_test_yayin.md E-04 (backend deployment ile paralel/bağımsız çalışır)
- [ ] ⚠️ Hosting/domain kararı (dosya başı nottaki açık karar çözülünce doldurulur)
- [ ] Prod build + deploy, `robots.txt`/sitemap canlı doğrulama, mağaza başvurularına (App Store
      Connect/Play Console) girilecek üç URL'nin (Privacy/Support/Account-Deletion) canlıda
      çalıştığının teyidi
