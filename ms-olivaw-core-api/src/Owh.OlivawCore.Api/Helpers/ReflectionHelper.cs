using System.Linq.Expressions;
using System.Reflection;

namespace Owh.OlivawCore.Api.Helpers;

public static class ReflectionHelper
{
    public static string? GetName<T, F>
    (
        Expression<Func<T, F>>? property
    )
    {
        var member = GetMemberExpression(property);

        if (member is null)
            return null;

        return member.Member.Name;
    }

    public static string? GetValue<T, F>
    (
        T? o,
        Expression<Func<T, F>>? property
    )
    {
        if (o is null)
            return null;

        var expr = GetMemberExpression(property);

        if (expr is null)
            return null;

        if (expr.Member is not PropertyInfo prop)
            return null;

        if (!prop.CanRead)
            return null;

        if (prop.GetIndexParameters().Length > 0)
            return null;

        if (prop.DeclaringType is null)
            return null;

        if (!prop.DeclaringType.IsInstanceOfType(o))
            return null;

        if (expr.Expression != property?.Parameters[0])
            return null;

        var value = prop.GetValue(o);

        if (value is null)
            return null;

        return value.ToString();
    }

    public static MemberExpression? GetMemberExpression<T, F>
    (
        Expression<Func<T, F>>? property
    )
    {
        if (property is null)
            return null;

        if (property.Body is MemberExpression member)
            return member;

        if (property.Body is not UnaryExpression unary)
            return null;

        if (unary.NodeType != ExpressionType.Convert)
            return null;

        if (unary.Operand is not MemberExpression convertedMember)
            return null;

        return convertedMember;
    }
}