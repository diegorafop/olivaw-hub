using Owh.OlivawCore.Api.Extensions.Collections;
using System.Linq.Expressions;
using Xunit;

namespace Owh.OlivawCore.Tests.ExtensionsTest;

public class LambdaCompositionTest
{
    [Theory]
    [InlineData(1, false, true)]
    [InlineData(5, true, true)]
    [InlineData(10, false, true)]
    public void Compose_should_bind_both_predicates_to_the_same_parameter(int value, bool and, bool or)
    {
        Expression<Func<int, bool>> lower = item => item > 2;
        Expression<Func<int, bool>> upper = item => item < 8;
        Assert.Equal(and, lower.And(upper).Compile()(value));
        Assert.Equal(or, lower.Or(upper).Compile()(value));
    }

    [Theory]
    [InlineData(false, 0, false, false)]
    [InlineData(false, 5, false, true)]
    [InlineData(true, 0, false, true)]
    [InlineData(true, 5, true, true)]
    public void Compose_should_support_predicates_on_nested_objects(bool active, int score, bool and, bool or)
    {
        Expression<Func<Parent, bool>> parent = item => item.Active;
        Expression<Func<Child, bool>> child = item => item.Score > 3;
        var value = new Parent { Active = active, Child = new Child { Score = score } };
        Assert.Equal(and, parent.And(child, item => item.Child).Compile()(value));
        Assert.Equal(or, parent.Or(child, item => item.Child).Compile()(value));
    }

    [Fact]
    public void Visitors_should_preserve_unmapped_parameters_and_replace_only_the_requested_one()
    {
        var oldParameter = Expression.Parameter(typeof(int), "old");
        var newParameter = Expression.Parameter(typeof(int), "replacement");
        var unrelated = Expression.Parameter(typeof(int), "unrelated");
        var visitor = new ReplaceParameterVisitor(oldParameter, newParameter);
        Assert.Same(newParameter, visitor.Visit(oldParameter));
        Assert.Same(unrelated, visitor.Visit(unrelated));
        Assert.Same(unrelated, ExpressionRebinder.ReplacementExpression([], unrelated));
        Assert.Throws<ArgumentNullException>(() => new ReplaceParameterVisitor(null!, newParameter));
        Assert.Throws<ArgumentNullException>(() => new ReplaceParameterVisitor(oldParameter, null!));
    }

    public class Parent
    {
        public bool Active { get; set; }
        public Child Child { get; set; } = new();
    }

    public class Child
    {
        public int Score { get; set; }
    }
}