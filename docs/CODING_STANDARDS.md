# Coding Standards - Educational Purpose

## 0. Genel Felsefe

Bu proje, **junior developer eğitimi** amaçlı yazılmaktadır. Her kod satırı, 6 ay tecrübeli bir juniorun anlayabileceği şekilde açıklanmalıdır.

```
Kural: Kod kendi kendine konuşmalıdır + Açıklama yorumları
Amaç: Neden, Nasıl, Ne işe yaradığı belli olmalı
Hedef: Repetitive learning + Best practices internalization
```

---

## 1. Comment Structure (Yorum Yapısı)

### 1.1 File-Level Comments

**Her dosyanın başında bulunmalı:**

```csharp
/// <summary>
/// UserProgressService.cs
/// 
/// PURPOSE (Amaç):
///   Kullanıcının kelime öğrenme ilerlemesini yönetmek ve SRS (Spaced Repetition System)
///   hesaplamalarını gerçekleştirmek. User progress tracking, mastery level updates,
///   ve next review time'ı calculate etmek.
///
/// WHY (Neden):
///   - Öğrenme platformunda başarı ölçüsü = progress tracking
///   - SRS olmadan, kullanıcılar kelimeleri unutur (forgetting curve)
///   - Özel algoritmalar ile optimal review zamanlarını belirlemek lazım
///
/// KEY METHODS (Ana Metodlar):
///   - UpdateProgressAsync(): User cevapladığında progress güncelle
///   - CalculateNextReview(): SRS algoritması ile sonraki review zamanını hesapla
///   - GetMasteryLevel(): Şu anki mastery level'ini döndür (0-5)
///
/// DEPENDENCIES (Bağımlılıklar):
///   - IUserProgressRepository: Progress verilerine erişim
///   - ILogger: Logging için (debugging, monitoring)
///   - IMapper: DTO mapping için
///
/// EXAMPLE USAGE (Örnek Kullanım):
///   var service = new UserProgressService(repository, logger, mapper);
///   await service.UpdateProgressAsync(userId, wordId, isCorrect: true);
///
/// REFERENCES (Referanslar):
///   - SM-2 Algorithm: https://en.wikipedia.org/wiki/SuperMemo
///   - Ebbinghaus Forgetting Curve: https://en.wikipedia.org/wiki/Forgetting_curve
///   - Best Practices: See CODING_STANDARDS.md § 1
/// </summary>

namespace WordLearner.Application.Services.Implementation;
```

### 1.2 Method-Level Comments (Fonksiyon Üzeri)

**Her public method/property'nin açıklaması:**

```csharp
/// <summary>
/// Kullanıcının belirli bir kelime için ilerleme durumunu günceller.
/// </summary>
/// <param name="userId">Güncelleme yapacak kullanıcının ID'si</param>
/// <param name="wordId">Öğrenilen kelimenin ID'si</param>
/// <param name="isCorrect">Kullanıcı cevabın doğru olup olmadığı (bool)</param>
/// <returns>Güncellenmiş progress bilgileri ve kazanılan XP</returns>
/// 
/// WHY THIS METHOD EXISTS (Bu metod neden var):
///   - Her öğrenme aktivitesinden sonra progress tracking zorunlu
///   - SRS algoritması doğru cevaplara göre interval'ları ayarlar
///   - Yanlış cevaplarda reset yapılması gerekir
///
/// HOW IT WORKS (Nasıl çalışır):
///   1. Database'den mevcut progress'i çek
///   2. İstatistikler'i güncelle (TimesCorrect, TimesIncorrect, vb.)
///   3. SRS algoritması ile next review zamanını hesapla
///   4. LearningHistory'ye record ekle (audit trail için)
///   5. UserProgress kaydet
///   6. XP hesapla ve User'a ekle
///
/// ALGORITHM REFERENCE (Algoritma):
///   SM-2 Formula:
///   - I(1) = 1 (first interval = 1 day)
///   - I(2) = 3 (second interval = 3 days)  
///   - I(n) = I(n-1) * EF (subsequent intervals)
///   - EF = max(1.3, EF + (0.1 - (5-q) * (0.08 + (5-q) * 0.02)))
///   - q = quality (0-5) = doğru cevap 5, yanlış 0
///
/// BEST PRACTICE (Best Practice):
///   - Async/await kullan (non-blocking)
///   - CancellationToken support ekle (graceful shutdown)
///   - Exception handling + logging
///   - Transaction wrapper kullan (atomicity)
///
/// EXAMPLES (Örnekler):
///   // Doğru cevap
///   var result = await progressService.UpdateProgressAsync(
///       userId: 1,
///       wordId: 5,
///       isCorrect: true,
///       cancellationToken: CancellationToken.None);
///   // Returns: XP +10, NextReviewAt = today + 3 days
///
///   // Yanlış cevap
///   var result = await progressService.UpdateProgressAsync(
///       userId: 1,
///       wordId: 5,
///       isCorrect: false,
///       cancellationToken: CancellationToken.None);
///   // Returns: XP +0, NextReviewAt = today + 1 day, reset interval
///
/// EXCEPTION CASES (İstisna Durumları):
///   - UserProgressNotFoundException: Kullanıcı kelimeyi başlatmamış
///   - UserNotFoundException: Kullanıcı silinmiş
///   - Handled gracefully with ILogger.LogError()
///
public async Task<ProgressUpdateResponseDto> UpdateProgressAsync(
    int userId,
    int wordId,
    bool isCorrect,
    CancellationToken cancellationToken = default)
{
    // Implementation...
}
```

### 1.3 Inline Comments (Kod İçi Yorumlar)

**Complex logic'lerde açıklamalar:**

```csharp
public async Task<ProgressUpdateResponseDto> UpdateProgressAsync(
    int userId,
    int wordId,
    bool isCorrect,
    CancellationToken cancellationToken = default)
{
    // STEP 1: Validate input
    // WHY: Garbage in, garbage out - invalid data işlemeyiz
    // HOW: Null checks ve business rule validation
    if (userId <= 0 || wordId <= 0)
        throw new ArgumentException("IDs must be positive integers");

    // STEP 2: Fetch current progress from database
    // WHY: Mevcut state'i bilmeden update edemeyiz (SRS için previous stats lazım)
    // HOW: Repository pattern ile database query
    // REFERENCE: See TECHNICAL_SPECIFICATIONS.md § 5.1 (Repository Pattern)
    var userProgress = await _progressRepository.GetByUserAndWordAsync(
        userId, wordId, cancellationToken)
        ?? throw new EntityNotFoundException(
            $"Progress not found for user {userId} and word {wordId}");

    // STEP 3: Update statistics
    // WHY: Track doğru/yanlış oranı, sonraki interval'ı hesapla
    // HOW: Increment counters, recalculate percentages
    userProgress.TimesCorrect += isCorrect ? 1 : 0;
    userProgress.TimesIncorrect += isCorrect ? 0 : 1;
    userProgress.TotalAttempts += 1;
    
    // Calculate success rate as percentage (0-100)
    // FORMULA: (Correct attempts / Total attempts) * 100
    // EXAMPLE: 9 correct / 12 total = 75%
    userProgress.SuccessRate = (userProgress.TimesCorrect * 100.0m) 
        / userProgress.TotalAttempts;

    // STEP 4: Calculate next review using SM-2 Algorithm
    // WHY: SRS core - optimal zaman aralıklarını hesapla
    // HOW: İçinde Ebbinghaus forgetting curve'ü model eder
    // REFERENCE: GERMAN_LANGUAGE_FEATURES.md § 2.5.1 (SRS)
    var (intervalDays, newLevel) = CalculateNextReviewBySM2(
        userProgress.CurrentLevel,
        userProgress.SuccessRate,
        isCorrect);

    // Update next review date
    // CALCULATION: Today + intervalDays
    // EXAMPLE: If intervalDays = 3, nextReviewAt = 3 days from now
    userProgress.NextReviewAt = DateTime.UtcNow.AddDays(intervalDays);
    userProgress.IntervalDays = intervalDays;
    userProgress.CurrentLevel = newLevel;

    // STEP 5: Record learning history (for analytics & audit trail)
    // WHY: 
    //   1. Audit trail: Ne zaman, kimin, ne yaptığı kayıt
    //   2. Analytics: User behavior analysis
    //   3. Debugging: Sorun çıkarsa historical data var
    // HOW: LearningHistory entity'ye insert
    var historyEntry = new LearningHistory
    {
        UserProgressId = userProgress.Id,
        UserId = userId,
        WordId = wordId,
        IsCorrect = isCorrect,
        TimeSpentSeconds = 5, // Frontend'den gelir, şu örnek
        Difficulty = 3,        // User perceived difficulty
        CreatedAt = DateTime.UtcNow
    };

    await _learningHistoryRepository.AddAsync(historyEntry, cancellationToken);

    // STEP 6: Calculate and apply XP reward
    // WHY: Gamification - user engagement boost
    // HOW: Correct answers = XP, wrong = 0 XP, bonus for streak
    // REFERENCE: ARCHITECTURE.md § 1 (Gamification layer)
    int xpEarned = isCorrect 
        ? 10 + (userProgress.SuccessRate > 80 ? 5 : 0) // Bonus if 80%+ success
        : 0;

    // Add XP to user's total
    // TRANSACTION: Critical - must happen atomically with progress update
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
    user.TotalXP += xpEarned;
    user.LifetimeXP += xpEarned;

    // Check level up (every 500 XP)
    // FORMULA: nextLevel = (TotalXP / 500) + 1
    // EXAMPLE: 0-499 XP = A1, 500-999 = A2, etc.
    var nextLevelIndex = user.TotalXP / 500;
    var newLevel_str = new[] { "A1", "A2", "B1", "B2", "C1", "C2" }[
        Math.Min(nextLevelIndex, 5)];
    
    if (user.CurrentLevel != newLevel_str)
    {
        // LEVEL UP! Notify user (optional via push notification)
        _logger.LogInformation(
            $"User {userId} leveled up to {newLevel_str}");
    }

    // STEP 7: Persist changes to database
    // WHY: Without saving, all updates are lost
    // HOW: Entity Framework Core SaveChangesAsync()
    // IMPORTANT: Must be inside try-catch for transaction rollback
    try
    {
        await _progressRepository.UpdateAsync(userProgress, cancellationToken);
        await _userRepository.UpdateAsync(user, cancellationToken);
        
        // Explicit save (some repos auto-save on Update)
        await _progressRepository.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex)
    {
        // Log and throw - caller handles retry logic
        _logger.LogError(
            $"Database update failed for userId {userId}: {ex.Message}");
        throw;
    }

    // STEP 8: Return response DTO
    // WHY: API contract - Frontend expects specific structure
    // HOW: Map entity to DTO, include next review info
    // REFERENCE: TECHNICAL_SPECIFICATIONS.md § 5.2 (DTO Mapping)
    return new ProgressUpdateResponseDto
    {
        Id = userProgress.Id,
        CurrentLevel = userProgress.CurrentLevel,
        Mastery = userProgress.SuccessRate,
        XpEarned = xpEarned,
        NextReviewAt = userProgress.NextReviewAt,
        Message = isCorrect 
            ? "✅ Correct! Great job!"
            : "❌ Try again. Keep learning!"
    };
}

/// <summary>
/// SM-2 (Supermemo-2) algoritması ile sonraki review zamanını hesapla.
/// 
/// PURPOSE: SRS'nin matematiksel temelini oluştur
/// ALGORITHM: https://en.wikipedia.org/wiki/SuperMemo#History_of_SM-2_algorithm
/// INPUT: currentLevel (0-5), successRate (0-100), isCorrect (bool)
/// OUTPUT: (intervalDays, newLevel)
/// </summary>
private (int intervalDays, int newLevel) CalculateNextReviewBySM2(
    int currentLevel,
    decimal successRate,
    bool isCorrect)
{
    // Eğer yanlış cevapsa, seviyeyi 0'a reset et
    // WHY: Baştan başlamalı, olmayan bazı
    // REFERENCE: Ebbinghaus forgetting curve
    if (!isCorrect)
    {
        return (intervalDays: 1, newLevel: 0);
    }

    // Doğru cevapsa, SM-2 formula'yı uygula
    // FORMULA:
    //   - I(1) = 1 day
    //   - I(2) = 3 days
    //   - I(n) = I(n-1) * EF
    // WHERE EF (Easiness Factor) = Quality of response dependent
    
    var intervalDays = currentLevel switch
    {
        // First time seeing word
        0 => 1,
        
        // Second review
        1 => 3,
        
        // Subsequent reviews multiply by ease factor
        // Higher mastery = longer intervals
        >= 2 => currentLevel * 7 // Simplified for clarity
    };

    var newLevel = Math.Min(currentLevel + 1, 5); // Cap at 5 (mastery)

    return (intervalDays, newLevel);
}
```

---

## 2. Class & Interface Comments

### 2.1 Interface Definition

```csharp
/// <summary>
/// IUserProgressService
/// 
/// RESPONSIBILITY (Sorumluluk):
///   Kullanıcının kelime öğrenme ilerlemesini yönetmek
///
/// WHY INTERFACE (Interface neden var):
///   1. Dependency Injection için loosely coupled code
///   2. Testing - Mock implementation'lar yazabiliriz
///   3. Multiple implementations desteklemek (future)
///   REFERENCE: TECHNICAL_SPECIFICATIONS.md § 5.1
///
/// EXAMPLE IMPLEMENTATION:
///   - UserProgressService: Real database operations
///   - MockUserProgressService: Testing için
///
/// USAGE (Kullanım):
///   public MyClass(IUserProgressService progressService)
///   {
///       _progressService = progressService; // Dependency injection
///   }
/// </summary>
public interface IUserProgressService
{
    /// <summary>
    /// Kullanıcının belirli bir kelime için ilerlemesini günceller
    /// </summary>
    Task<ProgressUpdateResponseDto> UpdateProgressAsync(
        int userId, int wordId, bool isCorrect, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Müdür bir kullanıcının mücadele ettiği kelimeleri alır
    /// (success rate < 50%)
    /// </summary>
    Task<IEnumerable<WordDto>> GetStruggelingWordsAsync(
        int userId, CancellationToken cancellationToken = default);
}
```

### 2.2 Entity Definition

```csharp
/// <summary>
/// UserProgress Entity
/// 
/// PURPOSE (Amaç):
///   Bir kullanıcının belirli bir kelimeyle ilgili öğrenme ilerlemesini saklamak.
///   SRS sistemi bu veriyi kullanarak next review zamanını belirler.
///
/// RELATIONSHIP (İlişki):
///   - N:1 with User (Bir user birçok kelime öğrenebilir)
///   - N:1 with Word (Birçok user aynı kelimeyi öğrenebilir)
///
/// KEY FIELDS (Ana Alanlar):
///   - CurrentLevel (0-5): Mastery level
///   - SuccessRate (0-100): Doğru cevap yüzdesi
///   - NextReviewAt: SRS tarafından hesaplanan sonraki review zamanı
///
/// SRS ALGORITHM (SRS Algoritması):
///   Buradaki NextReviewAt, Supermemo-2 algoritması ile hesaplanır.
///   User başladığında, NextReviewAt = today + 1 day
///   Doğru cevap = interval uzar (3, 7, 14, 30, 60 gün)
///   Yanlış cevap = interval reset (1 güne dön)
///
/// EXAMPLE (Örnek):
///   var progress = new UserProgress
///   {
///       UserId = 1,
///       WordId = 5,
///       CurrentLevel = 2,
///       NextReviewAt = DateTime.UtcNow.AddDays(7)
///   };
///
/// REFERENCE (Referans):
///   - ARCHITECTURE.md § 3 (Entity relationships)
///   - DATABASE_SCHEMA.md § 2.7 (SQL definition)
/// </summary>
public class UserProgress : BaseEntity
{
    /// <summary>
    /// Hangi user'ın bu progress'i
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Hangi kelimeyle ilgili bu progress
    /// </summary>
    public int WordId { get; set; }

    /// <summary>
    /// Mastery level (0-5)
    /// 0 = Never seen
    /// 1 = Weak recall
    /// 3 = Good recall
    /// 5 = Automatic recall (Mastery)
    /// </summary>
    public int CurrentLevel { get; set; }

    /// <summary>
    /// Doğru cevap yüzdesi (0-100)
    /// Kullanılır: SRS interval calculation, user profile page
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// SRS sistemi tarafından hesaplanan sonraki review zamanı
    /// WHY: Optimal tekrar zamanlaması - forgetting curve'ün tepesinde tekrar
    /// HOW: CalculateNextReview() metodunda hesaplanır
    /// </summary>
    public DateTime NextReviewAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Word Word { get; set; } = null!;
}
```

---

## 3. Repository & Data Access Comments

### 3.1 Repository Method

```csharp
/// <summary>
/// Belirli bir user ve word'ün progress'ini getirir.
/// 
/// WHY (Neden bu method):
///   - Update işlemleri öncesinde mevcut state'i bilmek gerekir
///   - Database kaynakları optimize etmek (lazy loading yerine explicit include)
///
/// HOW (Nasıl çalışır):
///   1. Database'de FirstOrDefaultAsync ile tek record arama
///   2. Include ile ilişkili entities'yi eager load et
///   3. Null ise EntityNotFoundException throw et
///
/// BEST PRACTICE (Best Practice):
///   - Async/await: Non-blocking I/O
///   - CancellationToken: Graceful shutdown
///   - Include(): Optimize queries (N+1 problem prevention)
///   - FirstOrDefaultAsync(): Single record query
///   REFERENCE: TECHNICAL_SPECIFICATIONS.md § 5.1 (Repository Pattern)
///
/// EXAMPLE QUERY PLAN (Veritabanında hangi sorgu çalışır):
///   SELECT up.*, u.*, w.*
///   FROM UserProgress up
///   INNER JOIN Users u ON up.UserId = u.Id
///   INNER JOIN Words w ON up.WordId = w.Id
///   WHERE up.UserId = @userId AND up.WordId = @wordId
///   AND up.IsDeleted = 0  ← Soft delete filter
/// </summary>
public async Task<UserProgress?> GetByUserAndWordAsync(
    int userId,
    int wordId,
    CancellationToken cancellationToken = default)
{
    // Query: UserProgress table'den, user ve word'e göre filtrele
    // Include: User ve Word entities'yi eager load et (N+1 sorunu engelle)
    // FirstOrDefaultAsync: Async/await pattern kullanarak non-blocking query
    return await _dbSet
        .Include(up => up.User)      // Include user details
        .Include(up => up.Word)      // Include word details
        .FirstOrDefaultAsync(        // Async single record query
            up => up.UserId == userId 
                  && up.WordId == wordId,
            cancellationToken);
}
```

---

## 4. API Controller Comments

### 4.1 Controller Method

```csharp
/// <summary>
/// [POST] /api/v1/words/{wordId}/progress
/// 
/// PURPOSE (Amaç):
///   Kullanıcının bir kelimeyi öğrenirken verdiği cevapı işler.
///   Progress günceller, SRS sonraki review zamanını hesaplar, XP ekler.
///
/// REQUEST (Gelen istek):
///   POST /api/v1/words/5/progress
///   Authorization: Bearer <JWT>
///   Content-Type: application/json
///   
///   {
///       "isCorrect": true,
///       "timeSpentSeconds": 8
///   }
///
/// RESPONSE (Dönen cevap):
///   200 OK
///   {
///       "success": true,
///       "data": {
///           "xpEarned": 10,
///           "nextReviewAt": "2024-01-18T10:00:00Z",
///           "currentLevel": 2
///       }
///   }
///
/// AUTH (Kimlik doğrulama):
///   - Requires JWT token in Authorization header
///   - User identity extracted from JWT claims
///   - REFERENCE: SECURITY.md § 2 (JWT auth)
///
/// BUSINESS LOGIC (İş mantığı):
///   1. Request validate et (isCorrect boolean mi, timeSpent positive mi)
///   2. Current user identity'yi JWT'den al
///   3. UserProgressService'i çağır (async)
///   4. Response DTO'ya map et
///   5. 200 OK döndür
///
/// ERROR HANDLING (Hata Yönetimi):
///   - 400: Invalid input (isCorrect missing, vb.)
///   - 401: Unauthorized (no token)
///   - 404: Word not found
///   - 409: User progress not found (user kelimeyi başlatmamış)
///   - 500: Server error (logging and response)
///   REFERENCE: API_ENDPOINTS.md § 8 (Error responses)
///
/// WHY ASYNC (Neden Async):
///   - Database operation slow (I/O bound)
///   - Async = thread pool'da thread release et
///   - Binlerce concurrent user'ları handle edebiliriz
///
/// WHY MUTATION (POST neden GET değil):
///   - State değişiyor (progress güncelleme)
///   - REST principle: GET (safe, idempotent), POST (unsafe)
///   - Caching implications: POST responses cached değildir
///
/// REFERENCE (Referanslar):
///   - ARCHITECTURE.md § 5 (API flow)
///   - TECHNICAL_SPECIFICATIONS.md § 6 (MediatR commands)
/// </summary>
[Authorize]
[HttpPost("words/{wordId:int}/progress")]
public async Task<IActionResult> SubmitAnswer(
    int wordId,
    [FromBody] ProgressSubmitRequest request,
    CancellationToken cancellationToken)
{
    // STEP 1: Validate input
    // WHY: Garbage in, garbage out
    if (request?.IsCorrect == null)
        return BadRequest(new { error = "isCorrect required" });

    // STEP 2: Extract user from JWT token
    // WHY: Know WHO did this (audit trail, user-specific data)
    // HOW: ClaimsPrincipal contains JWT claims
    // REFERENCE: SECURITY.md § 2.1 (JWT claims)
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdClaim?.Value, out int userId))
        return Unauthorized();

    // STEP 3: Create command for MediatR
    // WHY: CQRS pattern - separation of concerns
    // HOW: MediatR finds handler and executes
    // REFERENCE: TECHNICAL_SPECIFICATIONS.md § 6 (MediatR)
    var command = new SubmitAnswerCommand
    {
        UserId = userId,
        WordId = wordId,
        IsCorrect = request.IsCorrect,
        TimeSpentSeconds = request.TimeSpentSeconds
    };

    // STEP 4: Execute command (async)
    // WHY: Business logic doesn't belong in controller
    // HOW: MediatR handles async execution
    var result = await _mediator.Send(command, cancellationToken);

    // STEP 5: Return response
    // WHY: API contract - Frontend expects this structure
    // HOW: JSON serialization (automatic)
    // RESPONSE FORMAT: { success: true, data: { ... } }
    return Ok(new ApiResponse<ProgressUpdateResponseDto>(
        success: true,
        data: result,
        message: "Progress recorded"));
}
```

---

## 5. Validation Comments

### 5.1 FluentValidation Rules

```csharp
/// <summary>
/// ProgressSubmitRequestValidator
/// 
/// PURPOSE (Amaç):
///   ProgressSubmitRequest'in business rules'a uygun olduğunu doğrula
///   BEFORE it reaches business logic. (Defensive programming)
///
/// WHY VALIDATE (Neden validate):
///   1. Security: Malicious input'u erken yakala
///   2. Performance: Invalid data'ya resource harcama
///   3. UX: User'a hemen feedback ver
///   4. Consistency: Business rules tek yerde tanımlanır
///
/// REFERENCE (Referans):
///   - TECHNICAL_SPECIFICATIONS.md § 4 (Input validation)
///   - SECURITY.md § 4.1 (Server-side validation)
/// </summary>
public class ProgressSubmitRequestValidator : AbstractValidator<ProgressSubmitRequest>
{
    public ProgressSubmitRequestValidator()
    {
        // Rule 1: isCorrect required
        // WHY: Must know if answer correct or incorrect
        // HOW: NotNull() = null check, NotEmpty() = value check
        RuleFor(x => x.IsCorrect)
            .NotNull()
            .WithMessage("isCorrect is required");

        // Rule 2: TimeSpentSeconds must be positive
        // WHY: Can't have negative time spent learning
        // HOW: GreaterThan(0) = must be > 0
        // RANGE: Realistic (5-600 seconds = 5 min - 10 min)
        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThan(0)
            .WithMessage("Time spent must be positive")
            .LessThanOrEqualTo(600)
            .WithMessage("Time spent max 10 minutes");

        // Rule 3: Difficulty should be 1-5
        // WHY: User perception of difficulty helps adjust content
        // HOW: InclusiveBetween(1, 5) = 1 <= value <= 5
        RuleFor(x => x.Difficulty)
            .InclusiveBetween(1, 5)
            .WithMessage("Difficulty must be 1-5")
            .When(x => x.Difficulty.HasValue); // Optional field
    }
}
```

---

## 6. Database Context Comments

### 6.1 DbContext Configuration

```csharp
/// <summary>
/// WordLearnerDbContext
/// 
/// PURPOSE (Amaç):
///   Entity Framework Core'un veritabanı ile iletişimi
///   Data access layer'ın giriş noktası
///
/// WHY DbContext (Entity Framework neden):
///   1. ORM: Objects ↔ SQL mapping
///   2. LINQ queries: Type-safe, compile-time checking
///   3. Migrations: Database version control
///   4. Change tracking: Automatic dirty checking
///   5. Relationship management: FK handling
///
/// CONFIGURATION (Yapılandırma):
///   - OnConfiguring: Database provider, connection string
///   - OnModelCreating: Entity relationships, constraints, indexes
///
/// REFERENCE (Referanslar):
///   - TECHNICAL_SPECIFICATIONS.md § 5 (Repository pattern)
///   - DATABASE_SCHEMA.md § 2 (Entity definitions)
/// </summary>
public class WordLearnerDbContext : DbContext
{
    public WordLearnerDbContext(DbContextOptions<WordLearnerDbContext> options)
        : base(options)
    {
    }

    // DbSets: Entity collections = database tables
    // WHY: Entry point for LINQ queries
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Word> Words { get; set; } = null!;
    public DbSet<UserProgress> UserProgresses { get; set; } = null!;
    public DbSet<LearningHistory> LearningHistories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        // WHY: Define relationships, constraints, indexes
        // HOW: Fluent API (alternative to Data Annotations)
        // REFERENCE: DATABASE_SCHEMA.md § 2 (Schema)

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Unique constraint on Email
            // WHY: No duplicate emails in system
            // HOW: CreateIndex with IsUnique = true
            entity.HasIndex(e => e.Email)
                .IsUnique();

            // Column mappings & constraints
            entity.Property(e => e.Email)
                .HasMaxLength(254)          // RFC 5321
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(60)           // bcrypt hash size
                .IsRequired();

            // Soft delete filter
            // WHY: Deleted users data stays, but hidden by default
            // HOW: Global query filter
            // REFERENCE: TECHNICAL_SPECIFICATIONS.md § 2.1
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        // Word entity configuration
        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.GermanWord);
            entity.HasIndex(e => e.DifficultyLevel);

            // 1:1 relationship with WordDetail
            // WHY: Almanca-specific data in separate table (normalization)
            // HOW: HasOne().WithOne()
            entity.HasOne(w => w.WordDetail)
                .WithOne(wd => wd.Word)
                .HasForeignKey<WordDetail>(wd => wd.WordId)
                .OnDelete(DeleteBehavior.Cascade); // Delete word = delete detail

            // M:N relationship with Category
            // WHY: One word = many categories, one category = many words
            entity.HasMany(w => w.WordCategories)
                .WithOne(wc => wc.Word)
                .HasForeignKey(wc => wc.WordId);
        });

        // Apply configurations
        // WHY: Organize entity configs into separate files
        // HOW: IEntityTypeConfiguration<T> pattern
        // REFERENCE: TECHNICAL_SPECIFICATIONS.md § 5.2
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WordLearnerDbContext).Assembly);
    }
}
```

---

## 7. DTO & Mapping Comments

### 7.1 DTO Definition

```csharp
/// <summary>
/// ProgressUpdateResponseDto
/// 
/// PURPOSE (Amaç):
///   User progress update sonrasında API response'unda dönecek veriler
///   Entity ↔ DTO mapping için.
///
/// WHY DTO (Neden Entity değil):
///   1. Security: Sensitive fields expose etmiyorum (e.g., CreatedBy, DeletedAt)
///   2. API Contract: Frontend'in ihtiyacı olan veriler TÜMÜnü include et
///   3. Decoupling: Entity changes = API break değil
///   4. Performance: Network bandwidth optimize et (sadece gerekli fields)
///   5. Validation: DTO'da validation rules
///
/// EXAMPLE (Örnek):
///   Entity UserProgress:
///     - Id ✓ (döndür)
///     - UserId ✗ (gizle - user zaten biliyor)
///     - CreatedAt ✗ (internal, gizle)
///     - IsDeleted ✗ (security, gizle)
///
///   Response DTO:
///     - Id ✓
///     - CurrentLevel ✓
///     - SuccessRate ✓
///     - XpEarned ✓ (calculated)
///
/// REFERENCE (Referanslar):
///   - TECHNICAL_SPECIFICATIONS.md § 5.2 (DTO Mapping)
///   - API_ENDPOINTS.md § 5.2 (Response format)
/// </summary>
public class ProgressUpdateResponseDto
{
    /// <summary>
    /// Progress record ID
    /// WHY: Client-side tracking için
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Şu anki mastery level (0-5)
    /// 0 = Never seen
    /// 5 = Automatic recall
    /// WHY: User'ın progress'ini göster (visual indicator)
    /// </summary>
    public int CurrentLevel { get; set; }

    /// <summary>
    /// Doğru cevap yüzdesi
    /// WHY: Progress bar'ında göster
    /// </summary>
    public decimal Mastery { get; set; }

    /// <summary>
    /// Bu attempt'ta kazanılan XP
    /// WHY: User immediate feedback ver
    /// </summary>
    public int XpEarned { get; set; }

    /// <summary>
    /// SRS tarafından hesaplanan sonraki review zamanı
    /// WHY: User'ı inform et - ne zaman tekrar görecek?
    /// </summary>
    public DateTime NextReviewAt { get; set; }

    /// <summary>
    /// Human-readable feedback message
    /// WHY: User motivation + immediate feedback
    /// EXAMPLE: "✅ Correct! Great job!" or "❌ Try again"
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
```

### 7.2 AutoMapper Profile

```csharp
/// <summary>
/// ProgressMappingProfile
/// 
/// PURPOSE (Amaç):
///   UserProgress Entity ↔ ProgressUpdateResponseDto arasında mapping tanımla
///
/// WHY AutoMapper (Neden manual map değil):
///   1. DRY Principle: Mapping rules tek yerde
///   2. Type Safety: Compile-time checking
///   3. Performance: Caching + optimization
///   4. Maintainability: Kolayca değiştirebilirim
///
/// HOW (Nasıl çalışır):
///   Mapper.Map<ProgressUpdateResponseDto>(entity)
///   Automatically copies properties: UserProgress.Id → DTO.Id
///
/// REFERENCE (Referanslar):
///   - TECHNICAL_SPECIFICATIONS.md § 6 (MediatR + AutoMapper)
/// </summary>
public class ProgressMappingProfile : Profile
{
    public ProgressMappingProfile()
    {
        // UserProgress → ProgressUpdateResponseDto
        // WHY: Database entity to API response
        CreateMap<UserProgress, ProgressUpdateResponseDto>()
            .ForMember(dest => dest.CurrentLevel,
                opt => opt.MapFrom(src => src.CurrentLevel))
            .ForMember(dest => dest.Mastery,
                opt => opt.MapFrom(src => src.SuccessRate))
            // Note: XpEarned calculated in service, not entity
            .ForMember(dest => dest.XpEarned,
                opt => opt.Ignore()); // Set in service layer

        // CreateWordRequest → Word Entity
        // WHY: API input to database entity
        CreateMap<CreateWordRequest, Word>()
            .ForMember(dest => dest.CreatedAt,
                opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => true)); // Default true
    }
}
```

---

## 8. Testing Comments

### 8.1 Unit Test

```csharp
/// <summary>
/// ProgressServiceTests
/// 
/// PURPOSE (Amaç):
///   UserProgressService'in business logic'ini verify et
///   SRS algorithm doğru çalışıyor mu?
///
/// WHY TESTS (Neden test):
///   1. Regression prevention: Eski bug'lar tekrar olmasın
///   2. Documentation: Test = code example
///   3. Confidence: Deploy'a güvenle gidebilirim
///   4. Refactoring: Code değiştirebilirim, tests biliyor expected behavior
///   5. Edge cases: Normal case + boundary conditions
///
/// TEST NAMING (Test adlandırması):
///   MethodName_Scenario_ExpectedResult
///   Example: UpdateProgress_CorrectAnswer_IncrementsLevelAndXp
///
/// AAA PATTERN (Test struktur):
///   1. Arrange: Setup test data
///   2. Act: Call method
///   3. Assert: Verify result
///
/// MOCKING (Mock'lar):
///   - Repository: Database calls olmadan test et
///   - Logger: Side effects'ı ignore et
///   - Faster, more isolated tests
///
/// REFERENCE (Referanslar):
///   - DEVELOPMENT_SETUP.md § 7 (Testing)
/// </summary>
public class ProgressServiceTests
{
    private readonly Mock<IUserProgressRepository> _mockRepository;
    private readonly Mock<ILogger<UserProgressService>> _mockLogger;
    private readonly UserProgressService _service;

    public ProgressServiceTests()
    {
        // Arrange: Setup mocks
        // WHY: Don't need real database for unit tests
        _mockRepository = new Mock<IUserProgressRepository>();
        _mockLogger = new Mock<ILogger<UserProgressService>>();
        
        // Create service with mocked dependencies
        _service = new UserProgressService(
            _mockRepository.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// UpdateProgress_WithCorrectAnswer_IncrementsMasteryLevel
    /// 
    /// SCENARIO: User answers correctly
    /// EXPECTED: CurrentLevel should increase by 1
    /// 
    /// TEST DATA:
    ///   - User 1, Word 5
    ///   - Current level: 1 (weak recall)
    ///   - Answer: correct
    /// 
    /// ASSERTION:
    ///   - CurrentLevel: 1 → 2 (good recall)
    ///   - SuccessRate: should increase
    ///   - NextReviewAt: should be extended (SRS)
    /// </summary>
    [Fact]
    public async Task UpdateProgress_CorrectAnswer_IncrementsMasteryLevel()
    {
        // Arrange: Setup test data
        var userId = 1;
        var wordId = 5;
        
        var existingProgress = new UserProgress
        {
            Id = 1,
            UserId = userId,
            WordId = wordId,
            CurrentLevel = 1,          // Weak recall
            SuccessRate = 50,          // 50% success rate
            TimesCorrect = 2,
            TimesIncorrect = 2,
            TotalAttempts = 4,
            NextReviewAt = DateTime.UtcNow.AddDays(1)
        };

        // Mock repository'e expect et
        // WHY: Repository.GetByUserAndWordAsync() çağrılacak
        // HOW: Moq.It.IsAny<>() = any value accept et
        _mockRepository
            .Setup(r => r.GetByUserAndWordAsync(userId, wordId, default))
            .ReturnsAsync(existingProgress);

        // Act: Call method with correct answer
        var result = await _service.UpdateProgressAsync(
            userId, wordId, isCorrect: true, CancellationToken.None);

        // Assert: Verify results
        // WHY: Doğru cevap mastery'yi arttırmalı
        Assert.NotNull(result);
        Assert.Equal(2, existingProgress.CurrentLevel); // 1 → 2
        Assert.True(existingProgress.SuccessRate > 50); // Success rate increased

        // Verify repository was called
        // WHY: Make sure business logic actually queried database
        _mockRepository.Verify(
            r => r.GetByUserAndWordAsync(userId, wordId, default),
            Times.Once); // Called exactly once
    }

    /// <summary>
    /// UpdateProgress_WithWrongAnswer_ResetsLevel
    /// 
    /// SCENARIO: User answers incorrectly
    /// EXPECTED: Level should reset to 0
    /// WHY: Ebbinghaus forgetting curve - baştan başlamalı
    /// </summary>
    [Fact]
    public async Task UpdateProgress_WrongAnswer_ResetsLevel()
    {
        // Arrange
        var userId = 1;
        var wordId = 5;
        
        var existingProgress = new UserProgress
        {
            Id = 1,
            UserId = userId,
            WordId = wordId,
            CurrentLevel = 3,          // Good recall - but about to forget
            SuccessRate = 80,
            NextReviewAt = DateTime.UtcNow.AddDays(7)
        };

        _mockRepository
            .Setup(r => r.GetByUserAndWordAsync(userId, wordId, default))
            .ReturnsAsync(existingProgress);

        // Act: Call with wrong answer
        var result = await _service.UpdateProgressAsync(
            userId, wordId, isCorrect: false, CancellationToken.None);

        // Assert: Level reset, review sooner
        Assert.Equal(0, existingProgress.CurrentLevel);
        Assert.True(existingProgress.NextReviewAt 
            < DateTime.UtcNow.AddDays(2)); // Next review: tomorrow
    }
}
```

---

## 9. Configuration Comments

### 9.1 Dependency Injection Setup

```csharp
/// <summary>
/// Program.cs - Service Registration
/// 
/// PURPOSE (Amaç):
///   Uygulamayı başlatmak, dependency injection yapılandırmak,
///   middleware'ları eklemek.
///
/// WHY DI (Neden Dependency Injection):
///   1. Testability: Mock implementations inject edebilirim
///   2. Loose coupling: Classes interface'lere depend ediyor
///   3. Flexibility: Production vs development different implementations
///   4. Maintainability: Dependencies açık şekilde görünür
///
/// REFERENCE (Referanslar):
///   - TECHNICAL_SPECIFICATIONS.md § 5 (Service registration)
/// </summary>

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services to DI container
builder.Services.AddControllers();

// Add database context
// WHY: EF Core connection string'le initialize et
// HOW: AddDbContext<WordLearnerDbContext>()
// CONNECTION STRING: Configuration'dan oku (secure)
builder.Services.AddDbContext<WordLearnerDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
// WHY: Service implementation'lar register et
// HOW: Interface → Implementation mapping
builder.Services.AddScoped<IUserProgressService, UserProgressService>();
builder.Services.AddScoped<IWordService, WordService>();
// Scope: Per HTTP request (good for database operations)
// Singleton: Application lifetime (for stateless services)
// Transient: Every injection (heavy objects)

// Add MediatR
// WHY: CQRS pattern - commands/queries dispatching
// HOW: Scan assembly for handlers
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add AutoMapper
// WHY: Entity ↔ DTO mapping
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add validators
// WHY: Fluent validation rules
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add CORS
// WHY: Allow mobile app requests
// HOW: Only specific origins, not *
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp", policy =>
    {
        policy.WithOrigins("https://wordlearner.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Build app
var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // API documentation
}

app.UseHttpsRedirection();     // Force HTTPS
app.UseCors("MobileApp");      // Apply CORS policy
app.UseAuthentication();       // JWT validation
app.UseAuthorization();        // Role-based access
app.MapControllers();          // Map controller routes

app.Run();
```

---

## 10. Language Requirements (DİL GEREKSİNİMLERİ)

### 10.1 TÜRKÇE YAZMA KURALLARI

```
🇹🇷 TEMELİ KURAL: Her kod açıklaması TÜRKÇE yazılmalıdır

✅ YAPILACAKLAR:
- [ ] Tüm yorumlar TÜRKÇEde
- [ ] Tüm açıklamalar TÜRKÇEde
- [ ] Tüm XML documentation TÜRKÇEde
- [ ] Tüm console output TÜRKÇEde
- [ ] Tüm log messages TÜRKÇEde

❌ YAPILMAYACAKLAR:
- [ ] English comments (sadece code keywords'lar İngilizce)
- [ ] English documentation
- [ ] Mixed Turkish-English
- [ ] Turkish spelling errors

ISTISNA: 
- Method names: İngilizce (C# convention)
- Class names: İngilizce (C# convention)
- Property names: İngilizce (C# convention)
- Database column names: İngilizce (SQL convention)
```

### 10.2 Türkçe Yazma Örnekleri

**✅ DOĞRU:**
```csharp
/// <summary>
/// Kullanıcının kelime öğrenme ilerlemesini güncelleyen metod.
/// 
/// AMAÇ:
///   Öğrenci bir kelimeyi öğrenirken verdiği cevaptan sonra 
///   ilerleme durumunu kaydetmek ve SRS hesaplaması yapmak.
///
/// NEDEN:
///   Spaced Repetition System'in çalışması için her attempt'ın
///   kaydedilmesi gerekir.
///
/// ÖRNEK:
///   var sonuç = await progressService.UpdateProgressAsync(
///       userId: 1,
///       wordId: 5,
///       isCorrect: true);
///   // Sonuç: XP +10, Sonraki review: 3 gün sonra
/// </summary>
public async Task<ProgressUpdateResponseDto> UpdateProgressAsync(
    int userId,
    int wordId,
    bool isCorrect)
{
    // ADIM 1: Girdi validasyonu
    // NEDEN: Geçersiz veri işlemeyiz
    if (userId <= 0 || wordId <= 0)
        throw new ArgumentException("ID'ler pozitif olmalıdır");

    // ADIM 2: Mevcut progres'i veritabanından çek
    // NEDEN: SRS hesaplaması için önceki durumu bilmek lazım
    var userProgress = await _progressRepository.GetByUserAndWordAsync(userId, wordId);
}
```

### 10.3 Türkçe Logger ve Exception Messages

```csharp
// ✅ DOĞRU
_logger.LogInformation(
    "Kullanıcı {UserId} başarıyla giriş yaptı. IP: {IpAddress}",
    userId, ipAddress);

throw new EntityNotFoundException(
    $"Kullanıcı {userId} bulunamadı. Lütfen kontrol edin.");

throw new ValidationException(
    $"Email '{email}' zaten kayıtlı. Lütfen başka bir email kullanın.");
```

---

## 11. Summary Checklist

```
✅ REQUIRED FOR EVERY CODE FILE:

File Level:
- [ ] Purpose (Amaç) - bu dosya ne işi yapar?
- [ ] Why (Neden) - neden bu dosyaya ihtiyaç var?
- [ ] Dependencies (Bağımlılıklar) - neye bağlı?
- [ ] References (Referanslar) - başka dokümantasyona link

Method/Function Level:
- [ ] Purpose (Amaç) - bu method ne yapar?
- [ ] WHY (Neden) - neden var?
- [ ] HOW (Nasıl) - adımlar
- [ ] EXAMPLE (Örnek) - kullanım örneği
- [ ] ALGORITHM (eğer complex logic)
- [ ] EDGE CASES (sınır durumları)

Complex Logic:
- [ ] STEP comments (Hangi adımda)
- [ ] WHY each step
- [ ] HOW implemented
- [ ] REFERENCE to documentation

✅ BEST PRACTICES THROUGHOUT:
- [ ] Async/await (non-blocking I/O)
- [ ] CancellationToken support
- [ ] Exception handling + logging
- [ ] Null checking + guards
- [ ] Type safety (no dynamic, var used correctly)
- [ ] SOLID principles followed
- [ ] DRY (Don't Repeat Yourself)
- [ ] KISS (Keep It Simple, Stupid)

✅ TÜRKÇE DİL GEREKSİNİMLERİ:
- [ ] Tüm yorumlar TÜRKÇEde
- [ ] Tüm XML documentation TÜRKÇEde
- [ ] Tüm logger messages TÜRKÇEde
- [ ] Tüm exception messages TÜRKÇEde
- [ ] Türkçe yazım kurallarına uyuldu
- [ ] Teknik terimler doğru kullanıldı
- [ ] C# keywords İngilizce kaldı
```

---

**SONUÇ**: Her kod satırı bir hikaye anlatmalıdır. Junior developer'ınız 6 ay sonra bu kodu okuyduğünde, ne yaptığını, neden yaptığını, nasıl çalıştığını **TÜRKÇEde** anlayabilmelidir.

