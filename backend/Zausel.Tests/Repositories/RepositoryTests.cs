using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Zausel.Domain.Exceptions;
using Zausel.Infrastructure.Repositories;

namespace Zausel.Tests.Repositories;

public class RepositoryTests
{
    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task AddAsync_ValidEntity_SetsCreatedByUserIdAndPersists()
    {
        // ARRANGE
        await using var context = CreateContext();
        var repository = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "kelime" };

        // ACT
        await repository.AddAsync(entity, userId: 7);
        await repository.SaveChangesAsync();

        // ASSERT — "kim yaptı" alanı Repository'de, CreatedAt DbContext.SaveChangesAsync override'ında dolduruldu
        entity.CreatedByUserId.Should().Be(7);
        entity.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetByIdAsync_SoftDeletedEntity_ReturnsNull()
    {
        // ARRANGE — bir kayıt ekleyip soft delete uygula
        await using var context = CreateContext();
        var repository = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "silinecek" };
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        // ACT
        await repository.SoftDeleteAsync(entity.Id);
        await repository.SaveChangesAsync();

        // ASSERT — global query filter IsDeleted=true kaydı GetByIdAsync'ten gizler
        var result = await repository.GetByIdAsync(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_EntityDoesNotExist_ThrowsEntityNotFoundException()
    {
        // ARRANGE
        await using var context = CreateContext();
        var repository = new Repository<TestEntity>(context);

        // ACT
        var act = async () => await repository.SoftDeleteAsync(id: 999);

        // ASSERT
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("TestEntity not found: Id=999");
    }
}
