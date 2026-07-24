// ─────────────────────────────────────────────────────────────────────────────
// ErrorMessages.cs
//
// AMAÇ: AppException.Code değerlerinin her dildeki karşılığını tutan merkezi sözlük.
// NEDEN: Exception'lar mesajı kendi içinde sabitlemez — aynı kod, isteğin diline
//        göre farklı bir metne çevrilebilsin diye (REFERENCE/API_ENDPOINTS.md §1).
//        Şu an yalnızca tr+de var — uygulamanın gerçek hedef kitlesi DE↔TR
//        (bkz. DATABASE_SCHEMA/Icerik.md, Languages seed). İngilizce gibi henüz
//        hiçbir gerçek istemcinin istemediği bir dil spekülatif olarak eklenmez
//        (YAGNI — bkz. TASK.md "Spekülatif ortak tip yazılmaz" kuralı, aynı
//        gerekçeyle ApiResponse<T>/PagedResult<T> A-02'de geri alınmıştı).
//        Yeni bir dil eklemek yalnızca buraya bir sütun eklemekle olur, hiçbir
//        exception sınıfına dokunulmaz.
// BAĞIMLILIKLAR: Yok.
// ─────────────────────────────────────────────────────────────────────────────

namespace WordLearner.Application.Common.Localization;

public static class ErrorMessages
{
    // NEDEN varsayılan "tr": İstekte Accept-Language yoksa veya bilinmeyen bir dilse,
    //        proje Türkçe öncelikli olduğu için Türkçe'ye düşülür (00_INDEX.md kuralı).
    private const string DefaultLanguage = "tr";

    private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
    {
        ["INVALID_CREDENTIALS"] = new()
        {
            ["tr"] = "E-posta veya şifre hatalı.",
            ["de"] = "E-Mail oder Passwort ist falsch.",
        },
        ["INVALID_OTP"] = new()
        {
            ["tr"] = "Girilen kod geçersiz veya süresi dolmuş.",
            ["de"] = "Der eingegebene Code ist ungültig oder abgelaufen.",
        },
        ["EMAIL_ALREADY_REGISTERED"] = new()
        {
            ["tr"] = "Bu e-posta adresi zaten kayıtlı.",
            ["de"] = "Diese E-Mail-Adresse ist bereits registriert.",
        },
        ["ACCOUNT_SUSPENDED"] = new()
        {
            ["tr"] = "Hesabınız dondurulmuş. Lütfen destekle iletişime geçin.",
            ["de"] = "Ihr Konto wurde gesperrt. Bitte wenden Sie sich an den Support.",
        },
        ["ACCOUNT_DELETED"] = new()
        {
            ["tr"] = "Bu hesap kalıcı olarak silinmiş.",
            ["de"] = "Dieses Konto wurde dauerhaft gelöscht.",
        },
        ["INVALID_REFRESH_TOKEN"] = new()
        {
            ["tr"] = "Oturumunuzun süresi dolmuş. Lütfen tekrar giriş yapın.",
            ["de"] = "Ihre Sitzung ist abgelaufen. Bitte melden Sie sich erneut an.",
        },
        ["INVALID_SOCIAL_TOKEN"] = new()
        {
            ["tr"] = "Sosyal giriş doğrulanamadı.",
            ["de"] = "Die soziale Anmeldung konnte nicht verifiziert werden.",
        },
        ["QR_SESSION_GONE"] = new()
        {
            ["tr"] = "QR kodunun süresi doldu veya zaten kullanıldı. Lütfen yeni bir kod oluşturun.",
            ["de"] = "Der QR-Code ist abgelaufen oder wurde bereits verwendet. Bitte erstellen Sie einen neuen Code.",
        },
        ["QR_SESSION_FORBIDDEN"] = new()
        {
            ["tr"] = "Bu QR oturumunu yalnızca onu tarayan cihaz onaylayabilir/reddedebilir.",
            ["de"] = "Diese QR-Sitzung kann nur von dem Gerät bestätigt/abgelehnt werden, das sie gescannt hat.",
        },
        ["WORD_TEXT_ALREADY_EXISTS"] = new()
        {
            ["tr"] = "Bu dilde aynı metne sahip bir kelime zaten var. Yine de eklemek için 'force' seçeneğini kullanın.",
            ["de"] = "Es gibt bereits ein Wort mit demselben Text in dieser Sprache. Verwenden Sie die Option 'force', um es trotzdem hinzuzufügen.",
        },
        ["PART_OF_SPEECH_REQUIRED"] = new()
        {
            ["tr"] = "Kelime türü (partOfSpeech) zorunludur.",
            ["de"] = "Die Wortart (partOfSpeech) ist erforderlich.",
        },
        ["DIFFICULTY_LEVEL_REQUIRED"] = new()
        {
            ["tr"] = "Zorluk seviyesi (difficultyLevel) zorunludur.",
            ["de"] = "Der Schwierigkeitsgrad (difficultyLevel) ist erforderlich.",
        },
        ["TRANSLATIONS_REQUIRED"] = new()
        {
            ["tr"] = "En az bir dilde çeviri girilmelidir.",
            ["de"] = "Mindestens eine Übersetzung in einer Sprache ist erforderlich.",
        },
        ["LANGUAGE_CODE_REQUIRED"] = new()
        {
            ["tr"] = "Dil kodu (languageCode) zorunludur.",
            ["de"] = "Der Sprachcode (languageCode) ist erforderlich.",
        },
        ["WORD_TEXT_REQUIRED"] = new()
        {
            ["tr"] = "Kelime metni (text) zorunludur.",
            ["de"] = "Der Wortlaut (text) ist erforderlich.",
        },
        ["SAME_CONCEPT_PAIR_NOT_ALLOWED"] = new()
        {
            ["tr"] = "Bir kavram kendisiyle eşleştirilemez.",
            ["de"] = "Ein Konzept kann nicht mit sich selbst verknüpft werden.",
        },

        // NEDEN bu blok: CategoryHasChildrenException/CategoryHasActiveWordsException (A-06) —
        //       DeleteCategoryCommand'ın 409 silme koruması (API_ENDPOINTS.md §6).
        ["CATEGORY_HAS_CHILDREN"] = new()
        {
            ["tr"] = "Bu kategorinin alt kategorileri var. Önce alt kategorileri silin veya taşıyın.",
            ["de"] = "Diese Kategorie hat Unterkategorien. Löschen oder verschieben Sie zuerst die Unterkategorien.",
        },
        ["CATEGORY_HAS_ACTIVE_WORDS"] = new()
        {
            ["tr"] = "Bu kategoriye bağlı aktif kelimeler var. Önce kelimeleri başka bir kategoriye taşıyın.",
            ["de"] = "Dieser Kategorie sind aktive Wörter zugeordnet. Verschieben Sie zuerst die Wörter in eine andere Kategorie.",
        },
        ["CATEGORY_NAME_REQUIRED"] = new()
        {
            ["tr"] = "Kategori adı (name) zorunludur.",
            ["de"] = "Der Kategoriename (name) ist erforderlich.",
        },
        ["CATEGORY_TRANSLATIONS_REQUIRED"] = new()
        {
            ["tr"] = "En az bir dilde kategori adı girilmelidir.",
            ["de"] = "Mindestens ein Kategoriename in einer Sprache ist erforderlich.",
        },
        ["CATEGORY_CANNOT_BE_OWN_PARENT"] = new()
        {
            ["tr"] = "Bir kategori kendisinin veya kendi alt kategorisinin altına taşınamaz.",
            ["de"] = "Eine Kategorie kann nicht sich selbst oder ihrer eigenen Unterkategorie untergeordnet werden.",
        },

        // NEDEN bu blok: FluentValidation validator'ları (Application/Validators/Auth/)
        //       WithMessage() ile yalnızca sabit İngilizce bir LOG mesajı taşır — istemciye
        //       giden gerçek mesaj, her kuralın WithErrorCode() ile taşıdığı bu kodlar
        //       üzerinden ValidationFilter tarafından buradan çözülür (AppException ile
        //       birebir aynı ayrım: log=sabit İngilizce, API yanıtı=dile göre).
        ["EMAIL_REQUIRED"] = new()
        {
            ["tr"] = "E-posta adresi zorunludur.",
            ["de"] = "E-Mail-Adresse ist erforderlich.",
        },
        ["EMAIL_INVALID"] = new()
        {
            ["tr"] = "Geçerli bir e-posta adresi girin.",
            ["de"] = "Geben Sie eine gültige E-Mail-Adresse ein.",
        },
        ["PASSWORD_REQUIRED"] = new()
        {
            ["tr"] = "Şifre zorunludur.",
            ["de"] = "Passwort ist erforderlich.",
        },
        ["PASSWORD_TOO_SHORT"] = new()
        {
            ["tr"] = "Şifre en az 12 karakter olmalı.",
            ["de"] = "Das Passwort muss mindestens 12 Zeichen lang sein.",
        },
        ["PASSWORD_MISSING_UPPERCASE"] = new()
        {
            ["tr"] = "Şifre en az 1 büyük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Großbuchstaben enthalten.",
        },
        ["PASSWORD_MISSING_LOWERCASE"] = new()
        {
            ["tr"] = "Şifre en az 1 küçük harf içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Kleinbuchstaben enthalten.",
        },
        ["PASSWORD_MISSING_DIGIT"] = new()
        {
            ["tr"] = "Şifre en az 1 rakam içermeli.",
            ["de"] = "Das Passwort muss mindestens 1 Ziffer enthalten.",
        },
        ["PASSWORD_MISSING_SPECIAL_CHAR"] = new()
        {
            ["tr"] = "Şifre en az 1 özel karakter içermeli (!@#$%^&*).",
            ["de"] = "Das Passwort muss mindestens 1 Sonderzeichen enthalten (!@#$%^&*).",
        },
        ["OTP_REQUIRED"] = new()
        {
            ["tr"] = "Doğrulama kodu zorunludur.",
            ["de"] = "Bestätigungscode ist erforderlich.",
        },
        ["OTP_INVALID_FORMAT"] = new()
        {
            ["tr"] = "Doğrulama kodu 6 haneli olmalı.",
            ["de"] = "Der Bestätigungscode muss 6-stellig sein.",
        },
        ["FIRST_NAME_REQUIRED"] = new() { ["tr"] = "Ad zorunludur.", ["de"] = "Vorname ist erforderlich." },
        ["LAST_NAME_REQUIRED"] = new()
        {
            ["tr"] = "Soyad zorunludur.",
            ["de"] = "Nachname ist erforderlich.",
        },
        ["TOKEN_REQUIRED"] = new()
        {
            ["tr"] = "Token zorunludur.",
            ["de"] = "Token ist erforderlich.",
        },

        // NEDEN bu blok: WordGrammarValidator (Application/Validators/Words/) — WordDetail.
        //       GrammarData JSON'unu dile (de/tr) ve PartOfSpeech'e göre doğrular; her kural
        //       GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9'daki
        //       Zorunlu/Koşullu/Yasak matrisinin bir satırına karşılık gelir.
        ["GRAMMAR_DATA_INVALID_JSON"] = new()
        {
            ["tr"] = "Gramer verisi geçerli bir JSON olmalı.",
            ["de"] = "Die Grammatikdaten müssen gültiges JSON sein.",
        },
        ["GRAMMAR_DATA_MUST_BE_NULL_FOR_OTHER"] = new()
        {
            ["tr"] = "Bu kelime türü için gramer verisi girilemez, boş bırakılmalı.",
            ["de"] = "Für diese Wortart dürfen keine Grammatikdaten eingegeben werden, das Feld muss leer bleiben.",
        },
        ["GRAMMAR_DATA_REQUIRED"] = new()
        {
            ["tr"] = "Bu kelime türü için gramer verisi zorunludur.",
            ["de"] = "Für diese Wortart sind Grammatikdaten erforderlich.",
        },
        ["GRAMMAR_LANGUAGE_UNSUPPORTED"] = new()
        {
            ["tr"] = "Bu dil için gramer doğrulaması henüz desteklenmiyor.",
            ["de"] = "Die Grammatikprüfung wird für diese Sprache noch nicht unterstützt.",
        },
        ["GRAMMAR_DE_NOUN_GENDER_REQUIRED"] = new()
        {
            ["tr"] = "Almanca isimler için cinsiyet (gender) zorunludur.",
            ["de"] = "Für deutsche Substantive ist das Geschlecht (gender) erforderlich.",
        },
        ["GRAMMAR_DE_NOUN_PLURAL_REQUIRED"] = new()
        {
            ["tr"] = "Almanca isimler için çoğul (plural) biçimi zorunludur.",
            ["de"] = "Für deutsche Substantive ist die Pluralform erforderlich.",
        },
        ["GRAMMAR_DE_NOUN_CASES_INCOMPLETE"] = new()
        {
            ["tr"] = "Almanca isimler için 4 hâlin (Nominativ/Akkusativ/Dativ/Genitiv) tamamı doldurulmalı.",
            ["de"] = "Für deutsche Substantive müssen alle 4 Fälle (Nominativ/Akkusativ/Dativ/Genitiv) ausgefüllt sein.",
        },
        ["GRAMMAR_DE_NOUN_VERB_FIELDS_FORBIDDEN"] = new()
        {
            ["tr"] = "İsimlerde fiil alanları (ayrılabilir fiil, yardımcı fiil, çekim vb.) bulunamaz.",
            ["de"] = "Substantive dürfen keine Verbfelder (trennbares Verb, Hilfsverb, Konjugation usw.) enthalten.",
        },
        ["GRAMMAR_DE_VERB_ISSEPARABLE_REQUIRED"] = new()
        {
            ["tr"] = "Almanca fiiller için ayrılabilir fiil bilgisi (isSeparableVerb) zorunludur.",
            ["de"] = "Für deutsche Verben ist die Angabe zur Trennbarkeit (isSeparableVerb) erforderlich.",
        },
        ["GRAMMAR_DE_VERB_AUXILIARY_REQUIRED"] = new()
        {
            ["tr"] = "Almanca fiiller için yardımcı fiil (auxiliaryVerb) zorunludur.",
            ["de"] = "Für deutsche Verben ist das Hilfsverb (auxiliaryVerb) erforderlich.",
        },
        ["GRAMMAR_DE_VERB_PASTPARTICIPLE_REQUIRED"] = new()
        {
            ["tr"] = "Almanca fiiller için Partizip II (pastParticiple) zorunludur.",
            ["de"] = "Für deutsche Verben ist das Partizip II (pastParticiple) erforderlich.",
        },
        ["GRAMMAR_DE_VERB_CONJUGATION_INCOMPLETE"] = new()
        {
            ["tr"] = "Almanca fiiller için 3 zamanın (present/preterite/perfect) 6 kişilik çekiminin tamamı doldurulmalı.",
            ["de"] = "Für deutsche Verben müssen alle 6 Personenformen der 3 Zeiten (present/preterite/perfect) ausgefüllt sein.",
        },
        ["GRAMMAR_DE_VERB_SEPARABLE_PREFIX_REQUIRED"] = new()
        {
            ["tr"] = "isSeparableVerb=true iken ayrılabilir fiil öneki (separablePrefix) zorunludur.",
            ["de"] = "Wenn isSeparableVerb=true ist, ist das trennbare Präfix (separablePrefix) erforderlich.",
        },
        ["GRAMMAR_DE_VERB_SEPARABLE_PREFIX_FORBIDDEN"] = new()
        {
            ["tr"] = "isSeparableVerb=false iken ayrılabilir fiil öneki (separablePrefix) boş kalmalı.",
            ["de"] = "Wenn isSeparableVerb=false ist, muss das trennbare Präfix (separablePrefix) leer bleiben.",
        },
        ["GRAMMAR_DE_VERB_NOUN_FIELDS_FORBIDDEN"] = new()
        {
            ["tr"] = "Fiillerde isim alanları (cinsiyet, çoğul, hâl) bulunamaz.",
            ["de"] = "Verben dürfen keine Substantivfelder (Geschlecht, Plural, Fälle) enthalten.",
        },
        ["GRAMMAR_TR_NOUN_PLURAL_REQUIRED"] = new()
        {
            ["tr"] = "Türkçe isimler için çoğul biçimi zorunludur.",
            ["de"] = "Für türkische Substantive ist die Pluralform erforderlich.",
        },
        ["GRAMMAR_TR_NOUN_CASES_INCOMPLETE"] = new()
        {
            ["tr"] = "Türkçe isimler için 6 hâlin tamamı doldurulmalı.",
            ["de"] = "Für türkische Substantive müssen alle 6 Fälle ausgefüllt sein.",
        },
        ["GRAMMAR_TR_NOUN_VERB_FIELDS_FORBIDDEN"] = new()
        {
            ["tr"] = "İsimlerde fiil alanları (fiil kökü, olumsuz biçim, çekim) bulunamaz.",
            ["de"] = "Substantive dürfen keine Verbfelder (Verbstamm, Verneinung, Konjugation) enthalten.",
        },
        ["GRAMMAR_TR_VERB_VERBROOT_REQUIRED"] = new()
        {
            ["tr"] = "Türkçe fiiller için fiil kökü (verbRoot) zorunludur.",
            ["de"] = "Für türkische Verben ist der Verbstamm (verbRoot) erforderlich.",
        },
        ["GRAMMAR_TR_VERB_NEGATIVEFORM_REQUIRED"] = new()
        {
            ["tr"] = "Türkçe fiiller için olumsuz biçim (negativeForm) zorunludur.",
            ["de"] = "Für türkische Verben ist die Verneinungsform (negativeForm) erforderlich.",
        },
        ["GRAMMAR_TR_VERB_CONJUGATION_INCOMPLETE"] = new()
        {
            ["tr"] = "Türkçe fiiller için 5 zamanın 6 kişilik çekiminin tamamı doldurulmalı.",
            ["de"] = "Für türkische Verben müssen alle 6 Personenformen der 5 Zeiten ausgefüllt sein.",
        },
        ["GRAMMAR_TR_VERB_NOUN_FIELDS_FORBIDDEN"] = new()
        {
            ["tr"] = "Fiillerde isim alanları (çoğul, hâl) bulunamaz.",
            ["de"] = "Verben dürfen keine Substantivfelder (Plural, Fälle) enthalten.",
        },

        // NEDEN bu kod (A-07): UpdateUserRoleCommandValidator — Role yalnızca User/Admin olabilir
        //       (Users.Role CHECK constraint'iyle aynı küme, ama DB hatasına düşmeden önce
        //       uygulama katmanında yakalanır).
        ["INVALID_USER_ROLE"] = new()
        {
            ["tr"] = "Rol yalnızca User veya Admin olabilir.",
            ["de"] = "Die Rolle kann nur User oder Admin sein.",
        },

        // NEDEN bu kod (A-07): UpdateUserRoleCommandHandler/UpdateUserStatusCommandHandler —
        //       bir admin kendi rolünü/hesap durumunu DEĞİŞTİREMEZ (kaza sonucu kilitlenme riski).
        ["CANNOT_MODIFY_OWN_ACCOUNT"] = new()
        {
            ["tr"] = "Kendi rolünüzü veya hesap durumunuzu değiştiremezsiniz.",
            ["de"] = "Sie können Ihre eigene Rolle oder Ihren Kontostatus nicht ändern.",
        },

        // NEDEN bu kod (A-07): BulkImportWordsCommandValidator — Rows boşsa (hiç satır
        //       gönderilmemişse) TÜM istek bu tek kod ile 400 alır; satır bazlı hatalar
        //       BURADAN DEĞİL, BulkImportResultDto.Results'taki ErrorCode'lardan okunur
        //       (200 yanıtın içinde, HTTP hata kanalını KULLANMAZ).
        ["BULK_IMPORT_ROWS_REQUIRED"] = new()
        {
            ["tr"] = "En az bir satır gereklidir.",
            ["de"] = "Mindestens eine Zeile ist erforderlich.",
        },

        // NEDEN bu kod (A-08): LocalFileStorageService — yalnızca .jpg/.jpeg/.png/.webp
        //       kabul edilir, diğer uzantılar diske yazılmadan ÖNCE reddedilir.
        ["UNSUPPORTED_FILE_TYPE"] = new()
        {
            ["tr"] = "Desteklenmeyen dosya türü. Yalnızca JPG, PNG veya WEBP yükleyebilirsiniz.",
            ["de"] = "Nicht unterstützter Dateityp. Sie können nur JPG, PNG oder WEBP hochladen.",
        },

        // NEDEN bu kod (A-08): LocalFileStorageService — MaxFileSizeBytes (5 MB) sınırını
        //       aşan dosyalar diske yazılmadan ÖNCE reddedilir.
        ["FILE_TOO_LARGE"] = new()
        {
            ["tr"] = "Dosya boyutu izin verilen üst sınırı (5 MB) aşıyor.",
            ["de"] = "Die Dateigröße überschreitet das zulässige Limit (5 MB).",
        },

        // NEDEN bu kod (A-08, kod denetiminde bulundu): MediaController — hiç dosya
        //       gönderilmeden (veya 0 baytlık bir dosyayla) istek atıldığında fırlatılır.
        ["FILE_REQUIRED"] = new()
        {
            ["tr"] = "Yüklenecek bir dosya seçmelisiniz.",
            ["de"] = "Sie müssen eine Datei zum Hochladen auswählen.",
        },

        // NEDEN bu kod: ExceptionHandlingMiddleware, AppException'dan türemeyen (beklenmeyen)
        //       her exception için bu kodu kullanır — gerçek exception mesajı istemciye asla
        //       sızdırılmaz, sabit ve dile göre çözülen bir mesaj döner.
        ["INTERNAL_SERVER_ERROR"] = new()
        {
            ["tr"] = "Beklenmeyen bir hata oluştu.",
            ["de"] = "Ein unerwarteter Fehler ist aufgetreten.",
        },
    };

    // AMAÇ: Bir hata koduna, istenen dile (bulunamazsa Türkçe'ye) karşılık gelen mesajı döner.
    // NEDEN: bkz. LocalizedMessageResolver.Resolve — sözlükte olmayan bir kod gelirse
    //        (programlama hatası — yeni bir AppException eklenip buraya çevirisi
    //        eklenmemişse) exception fırlatmak yerine kodun kendisi döner; API asla
    //        yalnızca çeviri eksik diye 500'e düşmemeli.
    public static string Resolve(string code, string? language) =>
        LocalizedMessageResolver.Resolve(Messages, code, language, DefaultLanguage);
}
