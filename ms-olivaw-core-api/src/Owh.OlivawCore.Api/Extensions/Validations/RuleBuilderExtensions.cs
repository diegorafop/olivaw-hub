using FluentValidation;
using Owh.OlivawCore.Api.Extensions.Validations;
using Owh.OlivawCore.Api.Helpers;
using System.Linq.Expressions;

namespace Owh.OlivawCore.Api.Extensions.Validations;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, TProperty>
        NotEmpty<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(MessageHelper.EmptyFor(expression));
    }

    public static IRuleBuilderOptions<T, TProperty> NotEmptyWhenNotNull<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression
    )
    {
        return ruleBuilder
            .NotEmpty()
            .When((T item) =>
            {
                var value = expression.Compile()(item);
                return value is not null;
            })
            .WithMessage(MessageHelper.EmptyFor(expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        IsInEnum<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression
    )
    {
        return ruleBuilder
            .IsInEnum()
            .WithMessage(MessageHelper.InvalidFor(expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        NotNull<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression
    )
    {
        return ruleBuilder
            .NotNull()
            .WithMessage(MessageHelper.NullFor(expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        NotFoundFor<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper.NotFoundFor(request, expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        DuplicateKeyFor<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper.DuplicateKeyFor(request, expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        NotExpired<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper.ExpiredFor(request, expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        NotConfirmed<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper.ConfirmedFor(request, expression));
    }

    public static IRuleBuilderOptions<T, DateTime>
        GreaterThanMinimum<T>
    (
        this IRuleBuilder<T, DateTime> ruleBuilder,
        Expression<Func<T, DateTime>> expression,
        Func<DateTime, bool> mustFunction,
        DateTime minimum
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper.LowerThanMinimum(expression, minimum));
    }

    public static IRuleBuilderOptions<T, string>
        EmailAddress<T>
    (
        this IRuleBuilder<T, string> ruleBuilder,
        Expression<Func<T, string>> expression
    )
    {
        return ruleBuilder
            .NotEmpty(expression)
            .EmailAddress()
            .WithMessage(MessageHelper.EmailFor(expression));
    }

    public static IRuleBuilderOptions<T, TProperty>
        Equal<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expressionToCompare,
        Expression<Func<T, TProperty>> expressionTarget)
    {
        return ruleBuilder
            .Equal(expressionTarget)
            .WithMessage(MessageHelper
                .EqualFor(expressionToCompare, expressionTarget));
    }

    public static IRuleBuilderOptions<T, TProperty>
        EqualsToExpected<T, TProperty>
    (
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction
    )
    {
        return ruleBuilder
            .Must(mustFunction)
            .WithMessage(MessageHelper
                .DifferentThanExpectedFor(request, expression));
    }
}