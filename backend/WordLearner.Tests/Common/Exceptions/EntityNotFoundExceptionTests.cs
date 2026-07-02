// ─────────────────────────────────────────────────────────────────────────────
// EntityNotFoundExceptionTests.cs
//
// AMAÇ: EntityNotFoundException'ın Type+key overload'ının standart mesaj formatını
//       ürettiğini doğrulamak.
// NEDEN: Repository<T> ve ileride yazılacak feature servisleri bu overload'a
//        güveniyor; format burada bozulursa hata mesajları tutarsızlaşır.
// BAĞIMLILIKLAR: xUnit, FluentAssertions, WordLearner.Application.Common.Exceptions.EntityNotFoundException.
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using WordLearner.Application.Common.Exceptions;
using WordLearner.Domain.Entities;

namespace WordLearner.Tests.Common.Exceptions;

public class EntityNotFoundExceptionTests
{
    /// <summary>
    /// TypeAndKeyConstructor_ValidEntityTypeAndKey_ProducesStandardMessage
    ///
    /// AMAÇ: Type+key constructor'ının "{EntityAdı} bulunamadı: Id={key}" formatını ürettiğini doğrulamak.
    /// NEDEN: Repository<T>.SoftDeleteAsync bu overload'ı kullanıyor; format burada
    ///        değişirse loglama/hata standartlaşması bozulur.
    /// </summary>
    [Fact]
    public void TypeAndKeyConstructor_ValidEntityTypeAndKey_ProducesStandardMessage()
    {
        // ARRANGE — BaseEntity türeyen bir tip ve örnek bir Id
        var entityType = typeof(BaseEntityStub);

        // ACT — Type+key overload ile exception oluştur
        var exception = new EntityNotFoundException(entityType, 5);

        // ASSERT — mesaj standart formatta olmalı
        exception.Message.Should().Be("BaseEntityStub bulunamadı: Id=5");
    }

    // AMAÇ: Test için gerçek bir feature entity gerekmeden Type.Name üretmeye yarayan minimal sınıf.
    // NEDEN: Projede A-02 aşamasında henüz hiçbir feature entity yok.
    private class BaseEntityStub : BaseEntity;
}
