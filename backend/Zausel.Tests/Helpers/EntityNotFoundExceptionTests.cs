using FluentAssertions;
using Zausel.Domain.Exceptions;

namespace Zausel.Tests.Helpers;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void Constructor_MessageProvided_SetsMessageAndCode()
    {
        // ARRANGE + ACT
        var exception = new EntityNotFoundException("Word not found: Id=5");

        // ASSERT — Message İngilizce ve yalnızca log içindir, Code istemciye giden ErrorMessages çözümünün anahtarıdır
        exception.Message.Should().Be("Word not found: Id=5");
        exception.Code.Should().Be("ENTITY_NOT_FOUND");
    }
}
