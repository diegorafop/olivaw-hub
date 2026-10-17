using Mps.OlivawCore.Api.Helpers;
using Mps.OlivawCore.Domain;
using Xunit;

namespace Mps.OlivawCore.Tests.HelperTest;

public class MessageHelperTest
{
    private readonly Example _instance;

    public MessageHelperTest()
    {
        _instance = new Example
        {
            Name = "name",
            Description = "description",
            Permission = true,
            Id = 1,
            IdExterno = Guid.NewGuid(),
            DataCadastro = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            DataRemocao = null
        };
    }

    [Fact]
    public void NotFoundFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "não encontrado.",
            MessageHelper.NotFoundFor(entity, item => item.IdExterno)
        );
    }

    [Fact]
    public void InvalidFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "inválido.",
            MessageHelper.InvalidFor(entity, item => item.IdExterno)
        );
    }

    [Fact]
    public void DuplicateKeyFor_should_not_have_error()
    {
        var entity = _instance;

        Assert.Contains
        (
            "já cadastrado.",
            MessageHelper.DuplicateKeyFor(entity, item => item.IdExterno)
        );
    }

    [Fact]
    public void EmptyFor_should_not_have_error()
    {
        Assert.Contains
        (
            "está vazio.",
            MessageHelper.EmptyFor<Example, Guid>(item => item.IdExterno)
        );
    }

    [Fact]
    public void NullFor_should_not_have_error()
    {
        Assert.Contains
        (
            "é nulo.",
            MessageHelper.NullFor<Example, Guid>(item => item.IdExterno)
        );
    }

    [Fact]
    public void NullFor_parameterless_should_not_have_error()
    {
        Assert.Contains
        (
            "é nulo.",
            MessageHelper.NullFor<Example, int>(item => item.Id)
        );
    }

    [Fact]
    public void OutOfRange_should_not_have_error()
    {
        Assert.Contains
        (
            "está fora do intervalo",
            MessageHelper.OutOfRange<Example, int>(item => item.Id, 1, 2)
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