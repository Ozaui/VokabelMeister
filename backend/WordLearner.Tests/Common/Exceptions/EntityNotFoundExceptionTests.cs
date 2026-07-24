using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Domain.Entities;

namespace WordLearner.Tests.Common.Exceptions;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void TypeAndKeyConstructor_ValidEntityTypeAndKey_ProducesStandardMessage()
    {
        // ARRANGE — BaseEntity türeyen bir tip ve örnek bir Id
        var entityType = typeof(BaseEntityStub);

        // ACT — Type+key overload ile exception oluştur
        var exception = new EntityNotFoundException(entityType, 5);

        // ASSERT — mesaj standart formatta olmalı
        exception.Message.Should().Be("BaseEntityStub not found: Id=5");
    }

    // AMAÇ: Test için gerçek bir feature entity gerekmeden Type.Name üretmeye yarayan minimal sınıf.
    // NEDEN: Projede A-02 aşamasında henüz hiçbir feature entity yok.
    private class BaseEntityStub : BaseEntity;
}
