using Owh.OlivawCore.Api.Helpers;
using Owh.OlivawCore.Api.Settings;
using System.Linq.Expressions;
using Xunit;

namespace Owh.OlivawCore.Tests.HelperTest;

public class MessageTemplatesTest
{
    [Fact]
    public void Composite_messages_should_include_all_property_names_and_values_in_order()
    {
        var sample = new Sample { Name = "value", Number = 7 };
        Expression<Func<Sample, object>>[] properties = [item => item.Name, item => item.Number];
        Assert.Equal("Sample (Name = 'value'; Number = '7') não encontrado.", MessageHelper.NotFoundFor(sample, properties));
        Assert.Equal("Sample (Name = 'value'; Number = '7') já cadastrado.", MessageHelper.DuplicateKeyFor(sample, properties));
        Assert.Equal("Sample.Name: 'external' não encontrado.", MessageHelper.NotFoundFor<Sample, string>(item => item.Name, "external"));
    }

    [Fact]
    public void Field_messages_should_preserve_templates_and_validation_placeholders()
    {
        Assert.Equal("Sample.Name: '{PropertyValue}' é inválido.", MessageHelper.InvalidFor<Sample, string>(item => item.Name));
        Assert.Equal("Campo 'Sample.Name' não possui formato Json válido.", MessageHelper.InvalidJsonFormatFor<Sample, string>(item => item.Name));
        Assert.Equal("Campo 'Sample.Number' é menor que 1.", MessageHelper.LowerThanMinimum<Sample, int>(item => item.Number, 1));
        Assert.Equal("Campo 'Sample.Number' é maior que 10.", MessageHelper.GreaterThanMaximum<Sample, int>(item => item.Number, 10));
        Assert.Equal("Sample.Name: '{PropertyValue}' deve conter apenas números.", MessageHelper.OnlyNumberFor<Sample, string>(item => item.Name));
        Assert.Equal("Campo 'Sample.Name' é um e-mail inválido.", MessageHelper.EmailFor<Sample, string>(item => item.Name));
        Assert.Equal("Campo 'Sample.Name' é diferente do campo 'Sample.Number'.", MessageHelper.EqualFor<Sample, string, int>(item => item.Name, item => item.Number));
    }

    [Fact]
    public void Instance_messages_should_preserve_type_property_and_value()
    {
        var sample = new Sample { Name = "value" };
        Assert.Equal("Sample.Name: 'value' inautorizado.", MessageHelper.UnauthorizedFor(sample, item => item.Name));
        Assert.Equal("Sample.Name: 'value' é referenciado por Parent", MessageHelper.ForeignKeyFor(sample, item => item.Name, "Parent"));
        Assert.Equal("Campo Sample.Name: 'value' deve conter apenas números.", MessageHelper.OnlyNumberFor(sample, item => item.Name));
        Assert.Equal("Sample.Name: 'value' já cadastrado.", MessageHelper.DuplicateAuthentication(sample, item => item.Name));
        Assert.Equal("Sample.Login: 'value' - Usuário e/ou senha são inválidos.", MessageHelper.AuthenticationFor(sample, item => item.Name));
        Assert.Equal("Sample.Autorizações: Usuário sem nenhuma autorização.", MessageHelper.AuthenticationFor<Sample>());
        Assert.Equal("O tipo Sample não herda de ConteudoArquivoQueryResponse", MessageHelper.InvalidCastForFile<Sample>());
    }

    [Fact]
    public void InvalidSettingFor_should_use_section_constant_or_unknown_fallback()
    {
        Assert.Contains("services:provider-authentication", MessageHelper.InvalidSettingFor<SettingProviderAuthentication>());
        Assert.Contains("UnknownSection", MessageHelper.InvalidSettingFor<Sample>());
    }

    public class Sample
    {
        public string Name { get; set; } = "";
        public int Number { get; set; }
    }
}
