# Almanca ↔ Türkçe Kelime Doldurma ve Eşleştirme Kuralları

> Bu dosya sadece **özet/referans** amaçlıdır, kod içermez. Tek doğruluk kaynağı her zaman
> `docs/REFERENCE/GERMAN_LANGUAGE_FEATURES.md` (§10), `docs/REFERENCE/TURKISH_LANGUAGE_FEATURES.md`
> (§9) ve `docs/DATABASE_SCHEMA/Icerik.md` ("Eşleştirme" bölümü) — bu dosya onların insan-okunur
> özetidir, güncellenmeyi unutursak orijinaller geçerlidir. **v2 (2026-07-14):** kullanıcının gerçek
> veriye dayalı incelemesi sonrası 3 gerçek tasarım hatası düzeltildi — bkz. §5, §6, §9. **v3 (aynı
> gün):** ikinci inceleme turunda 2 ek nokta netleştirildi — "birincil taraf" seçim kuralı (§6) ve
> çoklu eşanlamlı kaybı riski (§9).

## 1. Neden iki ayrı kural var?

Almanca ve Türkçe kelime tabloları **birbirinden bağımsız** dolduruluyor: her dilin kendi Excel/
toplu-import şablonu var (795 satırlık Almanca ölçüm, kendi kolonlarıyla Türkçe tablo). İki dilin
grameri temelden farklı olduğu için (Almancada cinsiyet+artikel+4 hâl var, Türkçede yok; Türkçede
6 hâl+ünlü uyumu+iyelik var, Almancada yok) kurallar **kopyalanamaz** — her dilin kendi matrisi var,
ortaklaştırma zorlanmaz (YAGNI). Ortak olan tek şey: **hangi `Tür` (PartOfSpeech) seçilirse o türe
ait alan grubu doldurulur, kalanı "–"/NULL kalır.**

## 2. Almanca (`de`) — Tür Bazlı Doldurma Matrisi

| Tür | Her zaman dolu | Koşullu | Hiç dolmaz |
|-----|----------------|---------|------------|
| **İsim (Noun)** | Kelime, Tür, Anlamı, Çoğul, Cinsiyet (Maskulin/Feminin/Neutrum — tek değer), 4 hâl (Nominativ/Akkusativ/Dativ/Genitiv) | Bileşik Kelime Notu (yalnızca bileşik isimde, ör. *der Anrufbeantworter*) | Fiil alanları (Ayrılabilir mi, Ön ek, Yardımcı Fiil, Partizip II, 18 çekim hücresi) |
| **Fiil (Verb)** | Kelime, Tür, Anlamı, Ayrılabilir mi, Yardımcı Fiil, Partizip II, 18 çekim hücresi (Präsens/Präteritum/Perfekt × 6 kişi) | Ön ek (yalnızca ayrılabilir fiilde, ör. *an-*) | Cinsiyet, Çoğul, 4 hâl |
| **Diğer** (Sıfat, Zarf, Sayı, Bağlaç, Edat, Zamir, Ünlem → şemada `Other`) | Kelime, Tür, Anlamı | — | Ne cinsiyet/çoğul/hâl ne fiil çekimi |

**Ortak (her türde):** Örnek Cümle her satırda **%100 dolu**. Kategori 1 yalnızca kelime belirgin
bir temaya giriyorsa (soyut kelimelerde genelde boş). Kategori 2 yalnızca ikinci bir kategoriye de
uyuyorsa (çoğu satırda boş). "Örnek Cümle Anlamı"nın neden ayrı bir garanti olmadığı → §6.

## 3. Türkçe (`tr`) — Tür Bazlı Doldurma Matrisi

| Tür | Her zaman dolu | Koşullu | Hiç dolmaz |
|-----|----------------|---------|------------|
| **İsim (Noun)** | Kelime, Tür, Anlamı, Çoğul, 6 hâl (Yalın/Belirtme/Yönelme/Bulunma/Ayrılma/Tamlayan) | Bileşik Kelime Notu (yalnızca bileşik isimde, ör. *başöğretmen*) | Fiil Kökü, Olumsuz Biçimi, 30 çekim hücresi |
| **Fiil (Verb)** | Kelime, Tür, Anlamı, Fiil Kökü, Olumsuz Biçimi, 30 çekim hücresi (Şimdiki/Geniş/Di'li Geçmiş/Miş'li Geçmiş/Gelecek × 6 kişi) | Bileşik Kelime Notu (**Almancadan farklı olarak burada da var** — bileşik fiilde, ör. *göz atmak*) | Çoğul, 6 hâl |
| **Diğer** (Sıfat, Zarf, Sayı, Zamir, Edat, Bağlaç, Ünlem → şemada `Other`) | Kelime, Tür, Anlamı | — | Ne hâl ne çekim |

**Ortak (her türde):** Örnek Cümle her satırda dolu. Kategori 1/2 aynı Almanca kuralıyla — belirgin/
ikinci temaya uyuyorsa dolu, çoğu satırda boş. **Fiil Kökü/Olumsuz Biçimi henüz gerçek Türkçe
veriyle doğrulanmadı** (bunlar öneriydi, Almanca matrisi gibi 795 satır üzerinden ölçülmedi) — gerçek
`tr` içeriği gelince gözden geçirilecek, bkz. §9.

**Tek gerçek fark:** Bileşik kelime notu Almancada yalnızca **İsim**'de olabilirken, Türkçede hem
**İsim** hem **Fiil**'de olabilir. Validator bu ayrımı dile göre yapar, tek bir ortak kural yazılmaz.

## 4. Neden "3 cinsiyet kolonundan biri dolu" ama şemada tek `gender` alanı var?

Kullanıcının ölçtüğü tablo bir **Excel/insan-okunur** görünüm — kadın/erkek/nötr üç ayrı sütun,
hangisi kelimenin cinsiyetiyse o dolu, diğer ikisi "–". Veritabanı/`GrammarData` JSON'unda bu üç
sütun **tek bir `gender` alanına** karşılık gelir (`"Maskulin"|"Feminin"|"Neutrum"`) — üç ayrı DB
kolonu açılmaz, import mantığı Excel'deki "hangi sütun dolu" bilgisini tek değere indirger.

## 5. "Anlamı" Alanının Gerçek Anlamı (düzeltildi)

**İlk taslakta hata:** "Anlamı" alanını "kelimenin kendi dilinde tanımı" varsaymıştım. Kullanıcının
gerçek 795 satırlık verisi bunu çürüttü: Almanca tablonun "Anlamı" kolonu zaten **Türkçe** karşılığı
içeriyor (ör. `aber` → "ama, fakat, ancak") — yani baştan çift dilli, kelimenin kendi eşleşmesi
satırın içine gömülü.

**Doğru model:** `Words.Definition` dili **sabit değildir** — serbest bir "anlam notu"dur, genelde
karşı dilde kısa bir gloss olarak yazılır ama şema bunu zorunlu kılmaz. Kartta kullanıcıya gösterilen
**resmi çeviri buradan gelmez** — eşleşen kelimenin kendi `Text`'inden gelir. Bu alanın asıl işi:
ayrı import edilen 795+ satırı elle eşleştirmek yerine, zaten satırda duran bu glossu **eşleştirme
önerisi** üretmek için kullanmaktır (bkz. §6 — `suggestedMatchConceptId`). Yani önceden "kayıp/yanlış
yerde duran veri" gibi görünen şey aslında **elimizde zaten olan bir eşleştirme ipucu**.

## 6. Almanca ↔ Türkçe Eşleştirme (Pairing) — düzeltildi

İki dil **ayrı ayrı** toplu import edilir (her biri kendi §2/§3 matrisiyle doğrulanır). Bu, her satır
kendi başına bir `WordConcept` + tek dilde `Words` satırı yaratır — henüz **eşleşmemiş** durumda.
**Almanca satırı girerken Türkçe için yapılandırılmış bir kayıt (Words/WordConcept/gramer) hiç
açılmaz** — ama "Anlamı" kolonuna serbest metin olarak Türkçe gloss yazılması bunu bozmaz (§5), çünkü
bu ayrı bir kelime kaydı değil, sadece satırın yanında duran bir not.

- **Eşleştirme ekranı** (Admin panel, B-03 `WordPairingPage`): solda eşleşmemiş Almanca kelimeler,
  sağda eşleşmemiş Türkçe kelimeler. Her satırda **varsa bir öneri** (`suggestedMatchConceptId`) —
  bu konsepti Almanca `Definition`'ı ile Türkçe `Text`'i (veya tersi) karşılaştırarak bulunur, admin
  795 satırı elle taramak yerine öneriyi **onaylar**. Öneri yoksa manuel arar.
- Admin bir Almanca + bir Türkçe satırı seçip "Eşleştir" der → sistem ikinci kavramın `Words`
  satırını birinciye taşır, ikinci (artık boş) kavramı siler → sonuç: **tek `WordConcept`, iki dilde
  `Words`**.
- **Tür (PartOfSpeech) uyuşmazlığı ARTIK BLOKLANMIYOR (önceki taslakta 409+force vardı, kaldırıldı).**
  İki dil arasında çevrilen bir kavramın grameri türü doğal olarak kayabilir — ör. Almanca ayrılabilir
  bir fiil Türkçede bir deyim/isim tamlaması olabilir. Bu bir veri hatası **değil**, dillerin doğası;
  admin'e sürtünme yaratacak bir onay istenmez, "birincil" seçilen tarafın türü sessizce kazanır,
  arayüzde yalnızca bilgi amaçlı gösterilir.
- **Kategori (tema) çakışması** da bloklamaz ama bilgi değeri daha yüksektir (ör. "aile" temalı bir
  kelime yanlışlıkla "iş" kategorisiyle eşleşmesi fark edilmeye değer) — iki tarafın kategorileri yan
  yana gösterilir, admin isterse vazgeçer.
- **Seviye** (`WordConcepts.DifficultyLevel`, A1-C2) tür-matrisinden bağımsız concept-seviyesi bir
  alan — §2/§3'te yer almaz çünkü türe göre değişmez. Çakışmada "birincil" tarafınki kazanır.
- **"Birincil taraf" nasıl seçilir:** varsayılan olarak admin'in **"Eşleştir" işlemini başlattığı**
  taraf birincil olur (hangi satırdan tıklanırsa o). Onaydan önce arayüzde açık bir **"birincil
  tarafı değiştir"** kontrolü bulunur — admin isterse karşı tarafı birincil seçebilir. Yani bu
  gizli/rastgele bir click-order kuralı değil, her zaman görünür ve değiştirilebilir bir seçimdir.
- Eşleşmemiş kelime **hiçbir öğrenme oturumuna girmez** — yalnızca iki dilli (eşleşmiş) kavramlar
  kullanıcıya sunulur.

## 7. Örnek Cümleler: "Çeviri" mi, Bağımsız mı? — düzeltildi

**İlk taslakta hata:** "İki dilin örnek cümleleri eşleşince karşılıklı çeviri gibi görünür" demiştim.
Bu, iki dil **ayrı** import edildiğinde **garanti değil** — bir kelimenin Almanca örnek cümlesiyle
eşleştiği Türkçe kelimenin örnek cümlesi gerçek bir çeviri çifti olmak zorunda değil, sadece o
kelimeyi kullanan iki bağımsız cümle olabilir. (Elimizdeki "Guten Abend!" → "İyi akşamlar!" gibi
örnekler gerçek çeviri çünkü aynı satırda birlikte yazılmıştı — ayrı importlarda bu garanti kaybolur.)

**Doğru model:** `WordExamples`e nullable bir `PairedExampleId` (karşı dildeki örneğe self-referans)
eklendi. Bu alan **yalnızca** (a) iki dil `translations[]` ile **birlikte** girildiyse (o zaman
otomatik, sıradaki örnekle eşlenir) veya (b) eşleştirme ekranında admin **elle** iki örneği
birbirine bağladıysa dolar. Bağlı değilse örnek **bağımsızdır** — arayüz onu "Örnek Cümle Anlamı"
diye **sunmaz**, o kelimeyi kullanan ayrı bir cümle olarak gösterir.

## 8. Hangi Yönde Öğreniliyor? (`de→tr` / `tr→de`)

Bir kavram eşleşince kullanıcı onu **iki yönde de** öğrenebilir:
- `de→tr`: Almanca gramer (artikel/cinsiyet/hâl/ayrılabilirlik) test edilir, Türkçe yalnızca anlam
  olarak gösterilir.
- `tr→de`: Türkçe gramer (hâl eki/ünlü uyumu/iyelik) test edilir, Almanca yalnızca anlam.

Bu yön **kullanıcı profilinde sabit bir alan değil** — her öğrenme oturumu kendi yönünü seçer
(`targetLanguageId`, `POST /learning-sessions` gövdesinde zorunlu alan). Aynı hesap aynı anda hem
Almanca hem Türkçe öğrenebilir, **iki yön birbirinden tamamen bağımsız ilerler** (ayrı streak, ayrı
SM-2 ilerlemesi) — çünkü ilerleme tablosu (`UserProgress`) zaten **dile özel** `Words.Id` üzerinden
tutuluyor (Almanca satırın Id'si ile Türkçe satırın Id'si farklı), bu yüzden hiçbir şema değişikliği
gerekmedi.

## 9. Bilinen Riskler / Sonradan Doğrulanacaklar

- **Çoklu eşanlamlı kaybı (bilinçli sadeleştirme):** `Definition`="ama, fakat, ancak" gibi
  çok-adaylı durumlarda `UQ_Words_Concept_Language` kısıtı (kavram başına dilde tek `Words` satırı)
  yüzünden eşleştirme yalnızca **birini** resmi çift yapar — "ama" eşleşince "fakat"/"ancak" ayrı,
  sonsuza dek eşleşmemiş Türkçe kavramlar olarak kalabilir, `aber` çalışırken hiç gösterilmez. Bu
  **kabul edilen bir tradeoff** — şema büyütülüp çoklu eşanlamlı birinci sınıf desteklenmez (YAGNI),
  çoğu flashcard uygulaması zaten tek bir birincil çeviri gösterir. Buna bağlı olarak öneri
  algoritması `Definition`'ı **virgülle token'lara böler**, her adayı ayrı dener (tek string olarak
  denerse "ama, fakat, ancak" hiçbir tekil Türkçe kelimeye tam eşleşmez, öneri hiç bulunmaz).
  İstenirse eşleşmeyen ek eşanlamlılar `WordDetails.Notes`'a serbest metin olarak eklenebilir (yeni
  kolon açılmaz).
- **Türkçe Fiil Kökü/Olumsuz Biçimi** — benim önerimdi, gerçek veriyle henüz doğrulanmadı. Almanca
  matrisi 795 satırlık ölçümle kanıtlanmış durumda, Türkçe için elimizde henüz veri yok. Gerçek `tr`
  içeriği dolmaya başlayınca bu iki alan (ve genel olarak §3 matrisi) tekrar gözden geçirilmeli.
- **Eşleştirme önerisi (`suggestedMatchConceptId`) algoritması** henüz basit bir string
  normalize+karşılaştırma (artık virgül-token'lı) olarak tarif edildi — büyük hacimde (795+) yanlış
  pozitif/negatif oranı gerçek veriyle test edilmeden bilinmiyor; A-05 implementasyonunda ölçülüp
  ayarlanmalı.
- **`PairedExampleId`'nin elle bağlanması** operasyonel bir yük — otomatikleştirme (ör. iki dilin
  örnek cümle sayısı ve sırası tutuyorsa varsayılan eşleme öner) ileride değerlendirilebilir, şu an
  YAGNI gereği yalnızca temel mekanizma (alan + manuel bağlama) tanımlı.

## 10. Nerede Kodlanacak?

| Katman | Task | Ne yapar |
|--------|------|----------|
| Backend validator | A-05 | `WordGrammarValidator` — dil+tür'e göre §2/§3 matrisini uygular |
| Backend eşleştirme | A-05 | `GetUnmatchedWordConceptsQuery` (+ öneri) + `PairWordConceptsCommand` (bloklayıcı hata yok) |
| Backend toplu import | A-07 | Aynı `WordGrammarValidator`'ı satır bazında kullanır |
| Backend öğrenme yönü | C-05 | `LearningSessions.TargetLanguageId`, oturum kelime havuzu buna göre filtrelenir |
| Frontend form | B-03 | `WordFormModal` — önce dil, sonra Tür seçilir, alan grupları koşullu render edilir |
| Frontend eşleştirme | B-03 | `WordPairingPage` — öneri onayı + manuel arama |
| Frontend yön seçimi | D-05 / E-07 | `HomePage`/`HomeScreen`'de dil anahtarı (Almanca öğren / Türkçe öğren) |
| Mobil/Web kelime girişi | — | **Yok** — sistem kelimesi yalnızca Admin panelden girilir, mobil/web salt-okur kart gösterir |
