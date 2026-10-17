using System.Linq.Expressions;

namespace Mps.OlivawCore.Api.Extensions.Collections;

public static class LambdaExtensions
{
    private static Expression<T> Compose<T>
    (
        this Expression<T> leftExpression,
        Expression<T> rightExpression,
        Func<Expression, Expression, Expression> merge
    )
    {
        var map = leftExpression
            .Parameters
            .Select((left, i) => new
            {
                left,
                right = rightExpression.Parameters[i]
            })
            .ToDictionary(item => item.right, item => item.left);

        var rightBody = ExpressionRebinder.ReplacementExpression(map, rightExpression.Body);

        return Expression.Lambda<T>
        (
            merge(leftExpression.Body, rightBody),
            leftExpression.Parameters
        );
    }

    public static Expression<Func<T, bool>> And<T>
    (
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right
    )
    {
        return left.Compose(right, Expression.And);
    }

    public static Expression<Func<T, bool>> Or<T>
    (
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right
    )
    {
        return left.Compose(right, Expression.Or);
    }

    public static Expression<Func<TOuter, bool>>
        And<TOuter, TInner>
    (
        this Expression<Func<TOuter, bool>> left,
        Expression<Func<TInner, bool>> right,
        Expression<Func<TOuter, TInner>> propertyAccessExpression
    )
    {
        var newExpression = GetComposedExpression
        (
            right,
            propertyAccessExpression
        );

        return left.Compose(newExpression, Expression.And);
    }

    public static Expression<Func<TOuter, bool>>
        Or<TOuter, TInner>
    (
        this Expression<Func<TOuter, bool>> left,
        Expression<Func<TInner, bool>> right,
        Expression<Func<TOuter, TInner>> propertyAccessExpression
    )
    {
        var newExpression = GetComposedExpression
        (
            right,
            propertyAccessExpression
        );

        return left.Compose(newExpression, Expression.Or);
    }

    private static Expression<Func<TOuter, bool>>
        GetComposedExpression<TOuter, TInner>
    (
        Expression<Func<TInner, bool>> expression,
        Expression<Func<TOuter, TInner>> propertyAccessExpression
    )
    {
        var parameter = propertyAccessExpression.Parameters[0];

        var newInnerExpressionBody = new ReplaceParameterVisitor
            (
                expression.Parameters[0],
                propertyAccessExpression.Body
            )
            .Visit(expression.Body);

        var newExpression = Expression.Lambda<Func<TOuter, bool>>
        (
            newInnerExpressionBody,
            parameter
        );

        return newExpression;
    }
}

public class ExpressionRebinder
(
    Dictionary<ParameterExpression, ParameterExpression> map
) : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> _map = map
        ?? throw new ArgumentNullException(nameof(map));

    public static Expression ReplacementExpression
    (
        Dictionary<ParameterExpression, ParameterExpression> map,
        Expression exp
    )
    {
        return new ExpressionRebinder(map).Visit(exp);
    }

    protected override Expression VisitParameter
    (
        ParameterExpression node
    )
    {
        if (_map.TryGetValue(node, out ParameterExpression replacement))
            node = replacement;

        return base.VisitParameter(node);
    }
}

public class ReplaceParameterVisitor
(
    ParameterExpression oldParameter,
    Expression newExpression
) : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter = oldParameter
        ?? throw new ArgumentNullException(nameof(oldParameter));

    private readonly Expression _newExpression = newExpression
        ?? throw new ArgumentNullException(nameof(newExpression));

    protected override Expression VisitParameter
    (
        ParameterExpression node
    )
    {
        if (node == _oldParameter)
            return _newExpression;

        return node;
    }
}