using Owh.OlivawCore.Api.Helpers;
using System.Linq.Expressions;
using Xunit;

namespace Owh.OlivawCore.Tests.HelperTest;

public class ReflectionHelperTest
{
    [Fact]
    public void GetName_should_accept_direct_and_boxed_properties()
    {
        Assert.Equal("Name", ReflectionHelper.GetName<Sample, string?>(item => item.Name));
        Assert.Equal("Count", ReflectionHelper.GetName<Sample, object>(item => item.Count));
        Assert.Null(ReflectionHelper.GetName<Sample, string>(null));
        Assert.Null(ReflectionHelper.GetName<Sample, int>(item => 42));
        Assert.Null(ReflectionHelper.GetMemberExpression<Sample, int>(item => -item.Count));
        Assert.Null(ReflectionHelper.GetMemberExpression<Sample, object>(item => (object)42));
    }

    [Fact]
    public void GetValue_should_read_direct_properties_and_preserve_null()
    {
        var sample = new Sample { Name = "OlivawCore", Count = 7 };
        Assert.Equal("OlivawCore", ReflectionHelper.GetValue(sample, item => item.Name));
        Assert.Equal("7", ReflectionHelper.GetValue(sample, item => (object)item.Count));
        Assert.Null(ReflectionHelper.GetValue<Sample, string?>(null, item => item.Name));
        Assert.Null(ReflectionHelper.GetValue(sample, (Expression<Func<Sample, string>>?)null));
        Assert.Null(ReflectionHelper.GetValue(new Sample(), item => item.Name));
    }

    [Fact]
    public void GetValue_should_ignore_fields_nested_properties_and_computed_expressions()
    {
        var sample = new Sample { Child = new Sample { Name = "child" } };
        Assert.Null(ReflectionHelper.GetValue(sample, item => item.Field));
        Assert.Null(ReflectionHelper.GetValue(sample, item => item.Child!.Name));
        Assert.Null(ReflectionHelper.GetValue(sample, item => item.Name!.Length));
        Assert.Null(ReflectionHelper.GetValue(sample, item => item.Count + 1));
        Assert.Null(ReflectionHelper.GetValue(sample, item => item[0]));
        Assert.Null(ReflectionHelper.GetValue(sample, item => DateTime.UtcNow));
    }

    public class Sample
    {
        public string? Name { get; set; }
        public int Count { get; set; }
        public Sample? Child { get; set; }
        public string Field = "field";
        public string this[int index] => index.ToString();
    }
}
