/// <summary>
/// User.cs
///
/// AMAÇ:
///   Sistemdeki kullanıcıları temsil eden ana entity.
///   Kimlik doğrulama (e-posta/şifre, Google, Apple), profil,
///   öğrenme istatistikleri ve hesap durumu bilgilerini saklar.
///
/// NEDEN:
///   ASP.NET Identity kullanılmıyor — tüm alanlar manuel tanımlandı.
///   Sosyal giriş (GoogleId, AppleId) ve öğrenme istatistikleri
///   (XP, streak, seviye) aynı tabloda tutularak sorgu sayısı azaltıldı.
///
/// BAĞIMLILIKLAR:
///   - BaseEntity (Id, CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
///   - RefreshToken (1:N — kullanıcının aktif token'ları)
///   - UserCard, UserCategory, UserProgress, UserCardProgress (1:N)
///   - LearningSession, LearningHistory (1:N)
///   - UserAchievement (1:N)
///   - ClassMembership, Friendship, SharedContent (1:N)
/// </summary>

using WordLearner.Domain.Common;

namespace WordLearner.Domain.Entities;

/// <summary>
/// Kullanıcı entity'si.
///
/// AMAÇ: Tüm kullanıcı verilerini tek tabloda (Users) saklamak.
/// NEDEN: Kimlik doğrulama, profil ve öğrenme istatistikleri sıkça birlikte sorgulandığı için
///        ayrı tablolara bölmek yerine JOIN maliyetini ortadan kaldırmak tercih edildi.
/// </summary>
public class User : BaseEntity
{
    // ─── Kimlik Doğrulama ───────────────────────────────────────────────────

    /// <summary>
    /// Kullanıcının e-posta adresi — benzersiz, giriş için kullanılır.
    /// KISIT: Max 254 karakter (RFC 5321 standardı).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt ile hashlenmiş şifre (work factor 12).
    /// NEDEN NULL OLABİLİR: Google/Apple ile giriş yapan kullanıcıların şifresi yoktur.
    /// UZUNLUK: BCrypt her zaman 60 karakter üretir.
    /// </summary>
    public string? PasswordHash { get; set; }

    // ─── Sosyal Giriş ───────────────────────────────────────────────────────

    /// <summary>
    /// Google OAuth ile girişte Google'ın döndürdüğü kullanıcı ID'si.
    /// NEDEN: Aynı kişi hem e-posta hem Google ile kayıt olursa birleştirilir.
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// Apple Sign In ile girişte Apple'ın döndürdüğü kullanıcı ID'si.
    /// </summary>
    public string? AppleId { get; set; }

    /// <summary>
    /// Giriş yöntemi: Local | Google | Apple
    /// VARSAYILAN: Local (e-posta + şifre)
    /// </summary>
    public string AuthProvider { get; set; } = "Local";

    // ─── Profil ─────────────────────────────────────────────────────────────

    /// <summary>Ad (zorunlu, max 50 karakter)</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Soyad (zorunlu, max 50 karakter)</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Görünen ad — NULL ise FirstName + LastName birleşimi kullanılır.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Profil fotoğrafı URL'si (max 500 karakter)</summary>
    public string? AvatarUrl { get; set; }

    // ─── Dil Tercihleri ─────────────────────────────────────────────────────

    /// <summary>
    /// Öğrenme dil çifti, örn: "TR-DE" (Türkçe → Almanca).
    /// GELECEK: EN-DE gibi başka çiftler de desteklenebilir.
    /// </summary>
    public string PreferredLanguagePair { get; set; } = "TR-DE";

    /// <summary>Uygulama arayüzünün dili, örn: "tr", "en"</summary>
    public string PreferredUILanguage { get; set; } = "tr";

    // ─── Öğrenme Hedefleri ──────────────────────────────────────────────────

    /// <summary>
    /// Günlük toplam kelime hedefi (varsayılan: 10).
    /// NEDEN: Kullanıcı kendi hızını belirler; sistem bu sayıya ulaşınca "Bugünlük tamamlandı" gösterir.
    /// </summary>
    public int DailyWordGoal { get; set; } = 10;

    /// <summary>
    /// Günlük yeni kelime limiti — DailyWordGoal içinde kaçı yeni olsun? (varsayılan: 5).
    /// NEDEN: Geri kalanı SRS tekrarlarından oluşur. Yeni kelime oranı kontrol edilmezse
    ///        tekrar yükü hızla birikir (SuperMemo "overload" problemi).
    /// KISIT: DailyWordGoal'dan büyük olamaz — uygulama katmanında kontrol edilir.
    /// </summary>
    public int DailyNewWordLimit { get; set; } = 5;

    // ─── Öğrenme İstatistikleri ─────────────────────────────────────────────

    /// <summary>
    /// Kullanıcının CEFR dil seviyesi: A1 | A2 | B1 | B2 | C1 | C2
    /// NASIL GÜNCELLENİR: Doğru cevap sonrası XP birikince otomatik yükselir.
    /// </summary>
    public string CurrentLevel { get; set; } = "A1";

    /// <summary>
    /// Mevcut XP puanı — seviye atlarken sıfırlanabilir.
    /// NEDEN: Oyun mekaniklerinde "level bar" için kullanılır.
    /// </summary>
    public int TotalXP { get; set; } = 0;

    /// <summary>
    /// Tüm zamanların toplam XP'si — hiç sıfırlanmaz.
    /// NEDEN: Başarı rozetleri ve liderlik tablosu için kalıcı sayaç.
    /// </summary>
    public int LifetimeXP { get; set; } = 0;

    /// <summary>Toplam öğrenme süresi (dakika)</summary>
    public int TotalLearningMinutes { get; set; } = 0;

    /// <summary>
    /// Art arda öğrenme günü sayısı (streak).
    /// NEDEN: Günlük alışkanlık oluşturmak için motivasyon aracı.
    /// </summary>
    public int StreakDays { get; set; } = 0;

    /// <summary>Son streak güncelleme tarihi — bugünle karşılaştırılarak streak kırılır mı kontrol edilir.</summary>
    public DateTime? LastStreakDate { get; set; }

    // ─── Hesap Durumu ────────────────────────────────────────────────────────

    /// <summary>Hesap aktif mi? Admin tarafından dondurulabilir.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Kayıt sonrası adım adım sorulan onboarding akışı tamamlandı mı?
    /// NEDEN: FALSE ise mobile uygulama onboarding ekranını gösterir.
    ///        TRUE ise doğrudan ana ekrana yönlendirilir.
    /// NASIL TAMAMLANIR: Seviye + günlük hedef sorularını cevaplayınca TRUE yapılır.
    /// </summary>
    public bool IsOnboardingCompleted { get; set; } = false;

    /// <summary>E-posta adresi doğrulandı mı?</summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>E-posta doğrulama tarihi</summary>
    public DateTime? EmailVerifiedAt { get; set; }

    /// <summary>Son giriş tarihi (UTC)</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Son giriş IP adresi (IPv4 veya IPv6, max 45 karakter)</summary>
    public string? LastLoginIP { get; set; }

    /// <summary>Toplam giriş sayısı — güvenlik analizi için izlenir.</summary>
    public int LoginCount { get; set; } = 0;

    // ─── Bekleyen OTP Kodu (tek set — tüm amaçlar için ortak) ───────────────
    //
    // NEDEN TEK SET:
    //   Bir kullanıcının aynı anda birden fazla farklı OTP işlemi yapması beklenmez.
    //   (E-posta doğrulama VEYA şifre sıfırlama VEYA giriş VEYA silme — sadece biri)
    //   Ayrı alanlar yerine tek set = daha temiz şema, daha az nullable sütun.
    //
    // Purpose değerleri:
    //   "EmailVerification" | "PasswordReset" | "LoginOtp" | "AccountDeletion"

    /// <summary>
    /// Aktif OTP kodunun SHA-256 hash'i (max 88 karakter — Base64).
    /// NEDEN HASH: Veritabanı sızıntısında ham kod ele geçirilmez.
    /// </summary>
    public string? PendingOtpCodeHash { get; set; }

    /// <summary>
    /// OTP kodunun son kullanma tarihi (UTC).
    /// LoginOtp / PasswordReset → 5 dakika | EmailVerification → 24 saat | AccountDeletion → 15 dakika
    /// </summary>
    public DateTime? PendingOtpCodeExpiresAt { get; set; }

    /// <summary>
    /// OTP kodunun amacı — doğru akışın kullanıldığını kontrol etmek için.
    /// NEDEN: Şifre sıfırlama kodu ile giriş OTP'si birbirinin yerine kullanılamasın.
    /// </summary>
    public string? PendingOtpCodePurpose { get; set; }

    // ─── Hesap Silme ve Kalıcı Blok ─────────────────────────────────────────

    /// <summary>
    /// Hesabın kalıcı olarak silineceği tarih (UTC).
    /// NASIL ÇALIŞIR:
    ///   1. Kullanıcı silme onayı verince → IsDeleted=true, ScheduledDeletionAt=şimdi+30gün
    ///   2. 30 gün içinde giriş yaparsa → hesap otomatik kurtarılır (IsDeleted=false, bu alan null)
    ///   3. 30 gün dolarsa → arka plan görevi PII'yi anonimleştirir (IsAnonymized=true)
    /// </summary>
    public DateTime? ScheduledDeletionAt { get; set; }

    /// <summary>
    /// Kişisel veriler (PII) anonimleştirildi mi?
    /// NEDEN: GDPR — 30 gün sonunda ad, soyad, e-posta anonimleştirilir.
    ///        E-posta alanı placeholder ile değiştirilir; OriginalEmailHash blok için tutulur.
    /// </summary>
    public bool IsAnonymized { get; set; } = false;

    /// <summary>
    /// Anonimleştirme sonrası e-posta adresinin SHA-256 hash'i (max 88 karakter).
    /// NEDEN: E-posta alanı silinmiş olsa bile bu mail ile yeniden kayıt engellenir.
    /// NASIL: Kayıt sırasında SHA256(yeniEmail) == OriginalEmailHash ise kayıt reddedilir.
    /// </summary>
    public string? OriginalEmailHash { get; set; }

    // ─── Rol ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Kullanıcı rolü: User | Instructor | Admin
    /// NEDEN: ASP.NET Identity yerine manuel rol kontrolü yapılıyor.
    /// NASIL: JWT claim'ine eklenir, controller'da [Authorize(Roles = "Admin")] ile kontrol edilir.
    /// </summary>
    public string Role { get; set; } = "User";

    // ─── Navigation Properties ───────────────────────────────────────────────

    /// <summary>Kullanıcının aktif refresh token'ları (1:N)</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>Kullanıcının oluşturduğu kişisel kartlar (1:N)</summary>
    public ICollection<UserCard> UserCards { get; set; } = new List<UserCard>();

    /// <summary>Kullanıcının oluşturduğu kişisel kategoriler (1:N)</summary>
    public ICollection<UserCategory> UserCategories { get; set; } = new List<UserCategory>();

    /// <summary>Sistem kelimeleri için öğrenme ilerlemesi (1:N)</summary>
    public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();

    /// <summary>Kişisel kartlar için öğrenme ilerlemesi (1:N)</summary>
    public ICollection<UserCardProgress> UserCardProgresses { get; set; } = new List<UserCardProgress>();

    /// <summary>Tüm öğrenme girişimleri tarihçesi (1:N)</summary>
    public ICollection<LearningHistory> LearningHistories { get; set; } = new List<LearningHistory>();

    /// <summary>Öğrenme oturumları (1:N)</summary>
    public ICollection<LearningSession> LearningSessions { get; set; } = new List<LearningSession>();

    /// <summary>Kazanılan başarı rozetleri (1:N)</summary>
    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    /// <summary>Sınıf üyelikleri (1:N)</summary>
    public ICollection<ClassMembership> ClassMemberships { get; set; } = new List<ClassMembership>();

    /// <summary>Gönderilen arkadaşlık istekleri (1:N)</summary>
    public ICollection<Friendship> SentFriendships { get; set; } = new List<Friendship>();

    /// <summary>Alınan arkadaşlık istekleri (1:N)</summary>
    public ICollection<Friendship> ReceivedFriendships { get; set; } = new List<Friendship>();

    /// <summary>Oluşturulan paylaşım linkleri (1:N)</summary>
    public ICollection<SharedContent> SharedContents { get; set; } = new List<SharedContent>();
}
