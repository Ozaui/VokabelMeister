// ─────────────────────────────────────────────────────────────────────────────
// RepositoryTests.cs
//
// AMAÇ: Repository<T> generic taban sınıfının CRUD işlemlerini ve
//       WordLearnerDbContext'teki soft delete global filtresini doğrulamak.
// NEDEN: A-03'ten itibaren yazılacak tüm feature repository'ler (UserRepository,
//        WordRepository ...) bu sınıfı miras alacak; taban sınıf hatalıysa
//        hata her feature'a yayılır. Bu yüzden A-02'de, hiçbir feature entity'si
//        yokken bile, generic davranış test edilir (CODING_STANDARDS.md §7.4).
// BAĞIMLILIKLAR: xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory,
//                WordLearner.Infrastructure.Repositories.Repository<T>,
//                WordLearner.Infrastructure.Data.WordLearnerDbContext.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Domain.Entities;
using WordLearner.Infrastructure.Data;
using WordLearner.Infrastructure.Repositories;

namespace WordLearner.Tests.Repositories;

// AMAÇ: Repository<T> testleri için kullanılan, yalnızca bu test dosyasına özel minimal entity.
// NEDEN: Projede henüz (A-02 aşamasında) hiçbir feature entity'si yok; gerçek bir entity
//        yazmak yerine BaseEntity'den türeyen sahte bir sınıf kullanmak, testi gerçek
//        feature'lardan (A-03+) bağımsız ve DB'ye dokunmadan (in-memory) çalıştırmayı sağlar.
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

// AMAÇ: TestEntity'yi EF Core modeline dahil eden, yalnızca test projesinde kullanılan DbContext.
// NEDEN: WordLearnerDbContext'te TestEntity için bir DbSet property'si yok (ve olmamalı —
//        test entity'si production koduna sızmamalı); bu alt sınıf OnModelCreating'i override
//        ederek TestEntity'yi modele elle ekler. Böylece WordLearnerDbContext'in gerçek
//        (BaseEntity.IsDeleted'a bağlı) soft delete filtresi TestEntity üzerinde de çalışır.
public class TestDbContext : WordLearnerDbContext
{
    public TestDbContext(DbContextOptions<WordLearnerDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NEDEN: base.OnModelCreating soft delete filtresini modeldeki tüm BaseEntity
        //        türevlerine uygular; TestEntity bu döngüden önce modele eklenmeli.
        modelBuilder.Entity<TestEntity>();
        base.OnModelCreating(modelBuilder);
    }
}

public class RepositoryTests
{
    // AMAÇ: Her test için izole, benzersiz isimli bir in-memory veritabanı oluşturur.
    // NEDEN: Testler paralel/sırayla çalışırken aynı veritabanı adını paylaşırsa
    //        kayıtlar birbirine karışır; Guid ile her test kendi temiz DB'sinde çalışır.
    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WordLearnerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    /// <summary>
    /// AddAsync_ValidEntity_AssignsIdAndSaves
    ///
    /// AMAÇ: Yeni bir entity eklendiğinde DB'ye yazıldığını ve otomatik Id atandığını doğrulamak.
    /// NEDEN: Controller'lar 201 Created response'unda AddAsync'in döndürdüğü Id'yi kullanır;
    ///        Id atanmazsa tüm "oluştur" endpoint'leri hatalı response döner.
    /// </summary>
    [Fact]
    public async Task AddAsync_ValidEntity_AssignsIdAndSaves()
    {
        // ARRANGE — temiz context + repository + Id'siz yeni entity
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "Apfel" };

        // ACT — entity'yi ekle
        var sonuc = await repo.AddAsync(entity);

        // ASSERT — Id atanmış ve DB'de tek kayıt olmalı
        sonuc.Id.Should().BeGreaterThan(0);
        (await context.Set<TestEntity>().CountAsync()).Should().Be(1);
    }

    /// <summary>
    /// AddAsync_UserIdProvided_SetsCreatedByAndUpdatedByToSameUser
    ///
    /// AMAÇ: AddAsync'e userId geçildiğinde BaseEntity.CreatedByUserId ve UpdatedByUserId'nin
    ///       aynı kullanıcıya set edildiğini doğrulamak.
    /// NEDEN: "Kim oluşturdu" sorusu ActivityLog'a gitmeden doğrudan kayıttan cevaplanmalı;
    ///        Auth (A-03) tamamlanana kadar servis katmanı userId geçmeyecek, bu durumda
    ///        alan null kalmalı — bu yüzden hem dolu hem null senaryo test edilir.
    /// </summary>
    [Fact]
    public async Task AddAsync_UserIdProvided_SetsCreatedByAndUpdatedByToSameUser()
    {
        // ARRANGE — temiz context + repository
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);

        // ACT — userId ile ekle, userId'siz ekle
        var userIdIle = await repo.AddAsync(new TestEntity { Name = "Kartoffel" }, userId: 42);
        var userIdSiz = await repo.AddAsync(new TestEntity { Name = "Zwiebel" });

        // ASSERT — verilen userId her iki alana da yazılmalı; verilmezse null kalmalı
        userIdIle.CreatedByUserId.Should().Be(42);
        userIdIle.UpdatedByUserId.Should().Be(42);
        userIdSiz.CreatedByUserId.Should().BeNull();
        userIdSiz.UpdatedByUserId.Should().BeNull();
    }

    /// <summary>
    /// GetByIdAsync_RecordExists_ReturnsEntity
    ///
    /// AMAÇ: Var olan bir kaydın Id'sine göre doğru şekilde getirildiğini doğrulamak (mutlu yol).
    /// NEDEN: Servis katmanındaki tüm "detay getir" ve "güncelle" akışları bu metoda dayanır.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_RecordExists_ReturnsEntity()
    {
        // ARRANGE — bir entity ekle
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var eklenen = await repo.AddAsync(new TestEntity { Name = "Birne" });

        // ACT — eklenen kaydı Id'siyle sorgula
        var bulunan = await repo.GetByIdAsync(eklenen.Id);

        // ASSERT — aynı kayıt (aynı Name ile) dönmeli
        bulunan.Should().NotBeNull();
        bulunan!.Name.Should().Be("Birne");
    }

    /// <summary>
    /// GetByIdAsync_RecordNotFound_ReturnsNull
    ///
    /// AMAÇ: Olmayan bir Id sorgulandığında exception değil null döndüğünü doğrulamak.
    /// NEDEN: IRepository.cs'teki sözleşme gereği "bulunamadı" durumu null ile ifade edilir;
    ///        servis katmanı bu null'ı kontrol edip EntityNotFoundException fırlatır — repository
    ///        katmanının kendisi bu exception'ı GetByIdAsync'te fırlatmamalı.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_RecordNotFound_ReturnsNull()
    {
        // ARRANGE — boş context (hiç kayıt yok)
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);

        // ACT — var olmayan bir Id sorgula
        var sonuc = await repo.GetByIdAsync(999);

        // ASSERT — null dönmeli, exception fırlatılmamalı
        sonuc.Should().BeNull();
    }

    /// <summary>
    /// GetAllAsync_SoftDeleteFilterActive_ReturnsOnlyNonDeletedRecords
    ///
    /// AMAÇ: WordLearnerDbContext'teki global soft delete query filter'ının GetAllAsync
    ///       sonucundan silinmiş (IsDeleted=true) kayıtları otomatik çıkardığını doğrulamak.
    /// NEDEN: Bu, A-02'nin en kritik davranışıdır — filtre çalışmazsa silinmiş kullanıcı
    ///        kartları, kelimeler vb. tüm listelerde yanlışlıkla görünmeye devam eder.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_SoftDeleteFilterActive_ReturnsOnlyNonDeletedRecords()
    {
        // ARRANGE — iki kayıt ekle, birini soft-delete et
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var kalacak = await repo.AddAsync(new TestEntity { Name = "Kirsche" });
        var silinecek = await repo.AddAsync(new TestEntity { Name = "Pflaume" });
        await repo.SoftDeleteAsync(silinecek.Id);

        // ACT — tüm (filtrelenmiş) kayıtları getir
        var liste = await repo.GetAllAsync();

        // ASSERT — yalnızca silinmemiş kayıt dönmeli
        // NEDEN: Global query filter (WordLearnerDbContext.OnModelCreating) IsDeleted==true
        //        olan kayıtları sorgudan tamamen çıkarır; liste toplamı 1 olmalı.
        liste.Should().ContainSingle(e => e.Id == kalacak.Id);
        liste.Should().NotContain(e => e.Id == silinecek.Id);
    }

    /// <summary>
    /// UpdateAsync_ExistingEntity_AutoUpdatesUpdatedAt
    ///
    /// AMAÇ: WordLearnerDbContext.SaveChangesAsync override'ının, güncellenen entity'nin
    ///       UpdatedAt alanını elle set edilmeden otomatik olarak günceli tarihe çektiğini doğrulamak.
    /// NEDEN: Servis katmanında hiçbir yerde "entity.UpdatedAt = DateTime.UtcNow" yazılmayacak
    ///        (BaseEntity.md/WordLearnerDbContext.md kararı) — bu davranış merkezi override'a bağlı.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ExistingEntity_AutoUpdatesUpdatedAt()
    {
        // ARRANGE — kaydı ekle, UpdatedAt'i bilinçli olarak eskiye çek (dünkü zaman)
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var eklenen = await repo.AddAsync(new TestEntity { Name = "Traube" });
        eklenen.UpdatedAt = DateTime.UtcNow.AddDays(-1);
        eklenen.Name = "Traube (güncellendi)";

        // ACT — entity'yi güncelle
        await repo.UpdateAsync(eklenen);

        // ASSERT — UpdatedAt otomatik olarak "şimdi"ye yakın bir değere çekilmiş olmalı
        eklenen.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// UpdateAsync_UserIdProvided_UpdatesUpdatedByUserId
    ///
    /// AMAÇ: UpdateAsync'e userId geçildiğinde BaseEntity.UpdatedByUserId'nin o kullanıcıya
    ///       set edildiğini, önceki (farklı) değerin üzerine yazıldığını doğrulamak.
    /// NEDEN: "Son güncelleyen kim" sorusu ActivityLog'a gitmeden doğrudan kayıttan cevaplanmalı;
    ///        her UpdateAsync çağrısı en son işlemi yapan kullanıcıyı yansıtmalı.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UserIdProvided_UpdatesUpdatedByUserId()
    {
        // ARRANGE — kaydı kullanıcı 1 olarak ekle
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var eklenen = await repo.AddAsync(new TestEntity { Name = "Rettich" }, userId: 1);

        // ACT — kaydı kullanıcı 2 olarak güncelle
        eklenen.Name = "Rettich (güncellendi)";
        await repo.UpdateAsync(eklenen, userId: 2);

        // ASSERT — UpdatedByUserId son güncelleyeni (2) yansıtmalı, CreatedByUserId (1) değişmemeli
        eklenen.UpdatedByUserId.Should().Be(2);
        eklenen.CreatedByUserId.Should().Be(1);
    }

    /// <summary>
    /// SoftDeleteAsync_RecordExists_SetsIsDeletedTrueAndHidesFromQuery
    ///
    /// AMAÇ: SoftDeleteAsync'in kaydı fiziksel silmek yerine IsDeleted/DeletedAt alanlarını
    ///       set ettiğini ve bu sayede kaydın sonraki sorgulardan (filtre yüzünden) kaybolduğunu
    ///       doğrulamak.
    /// NEDEN: Fiziksel silme geri alınamaz ve audit trail'i bozar; soft delete kuralı
    ///        Repository.cs'in NEDEN yorumunda açıkça belirtilmiş kritik bir davranış.
    /// </summary>
    [Fact]
    public async Task SoftDeleteAsync_RecordExists_SetsIsDeletedTrueAndHidesFromQuery()
    {
        // ARRANGE — bir kayıt ekle
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);
        var eklenen = await repo.AddAsync(new TestEntity { Name = "Zwetschge" });

        // ACT — kaydı soft-delete et
        await repo.SoftDeleteAsync(eklenen.Id);

        // ASSERT — normal sorguda görünmemeli, ama IgnoreQueryFilters ile ham veri hâlâ DB'de olmalı
        // NEDEN: "Görünmüyor" ile "fiziksel olarak silindi" farklı şeylerdir; ikisini de doğrulamak gerekir.
        (await repo.GetByIdAsync(eklenen.Id))
            .Should()
            .BeNull();

        var hamKayit = await context
            .Set<TestEntity>()
            .IgnoreQueryFilters()
            .FirstAsync(e => e.Id == eklenen.Id);
        hamKayit.IsDeleted.Should().BeTrue();
        hamKayit.DeletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// SoftDeleteAsync_RecordNotFound_ThrowsEntityNotFoundException
    ///
    /// AMAÇ: Olmayan bir Id ile SoftDeleteAsync çağrıldığında EntityNotFoundException
    ///       fırlatıldığını doğrulamak (bulunamadı durumu — CODING_STANDARDS.md §7.5 zorunlu senaryo).
    /// NEDEN: Global exception middleware'i (A-02'nin sonraki bir adımı) bu exception tipini
    ///        yakalayıp 404 döndürecek; yanlış exception tipi fırlatılırsa 500 döner.
    /// </summary>
    [Fact]
    public async Task SoftDeleteAsync_RecordNotFound_ThrowsEntityNotFoundException()
    {
        // ARRANGE — boş context (hiç kayıt yok)
        await using var context = CreateContext();
        var repo = new Repository<TestEntity>(context);

        // ACT — var olmayan bir Id'yi silmeyi dene
        var act = () => repo.SoftDeleteAsync(999);

        // ASSERT — EntityNotFoundException fırlatılmalı
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
