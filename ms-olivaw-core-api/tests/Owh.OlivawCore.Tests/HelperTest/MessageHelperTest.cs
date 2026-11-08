using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Domain;
using Xunit;

namespace Owh.OlivawCore.Tests.HelperTest;

public class MessageHelperTest
{
    private sealed class MessageEntity : Entity { }
    private readonly MessageEntity _instance;

    public MessageHelperTest()
    {
        _instance = new MessageEntity
        {
            Id = 1,
            ExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            DeletedAt = null
        };
    }

    [Fact]
    public void NotFoundFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "não encontrado.",
            MessageHelper.NotFoundFor(entity, item => item.ExternalId)
        );
    }

    [Fact]
    public void InvalidFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "inválido.",
            MessageHelper.InvalidFor(entity, item => item.ExternalId)
        );
    }

    [Fact]
    public void DuplicateKeyFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "já cadastrado.",
            MessageHelper.DuplicateKeyFor(entity, item => item.ExternalId)
        );
    }

    [Fact]
    public void EmptyFor_should_not_have_error()
    {
        Assert.Contains
        (
            "está vazio.",
            MessageHelper.EmptyFor<MessageEntity, Guid>(item => item.ExternalId)
        );
    }

    [Fact]
    public void NullFor_should_not_have_error()
    {
        Assert.Contains
        (
            "é nulo.",
            MessageHelper.NullFor<MessageEntity, Guid>(item => item.ExternalId)
        );
    }

    [Fact]
    public void NullFor_parameterless_should_not_have_error()
    {
        Assert.Contains
        (
            "é nulo.",
            MessageHelper.NullFor<MessageEntity, int>(item => item.Id)
        );
    }

    [Fact]
    public void OutOfRange_should_not_have_error()
    {
        Assert.Contains
        (
            "está fora do intervalo",
            MessageHelper.OutOfRange<MessageEntity, int>(item => item.Id, 1, 2)
        );
    }

    [Fact]
    public void NotFoundFor_should_have_error_when_property_value_is_null()
    {
        var entity = _instance;

        var message = MessageHelper.NotFoundFor
        (
            entity,
            property: item => item.Id
        );

        Assert.Contains
        (
            $"'{entity.Id}' não encontrado.",
            message
        );
    }
}