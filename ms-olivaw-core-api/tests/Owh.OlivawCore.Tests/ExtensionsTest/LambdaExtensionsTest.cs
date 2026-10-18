using Owh.OlivawCore.Api.Extensions.Collections;
using System.Linq.Expressions;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class LambdaExtensionsTest
{
    public LambdaExtensionsTest() { }

    [Fact]
    public void Constructor_should_have_error()
    {
        Dictionary<ParameterExpression, ParameterExpression> map = null;

        var exception = Assert.Throws<ArgumentNullException>
        (
            () => new ExpressionRebinder(map)
        );

        Assert.Equal
        (
            new ArgumentNullException(nameof(map)).Message,
            exception.Message
        );
    }
}