using FluentValidation;
using System.Linq.Expressions;

namespace Mps.OlivawCore.Api.Extensions.Validations;

public static class AbstractValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> NotEmpty<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression)
    {
        return validator
            .RuleFor(expression)
            .NotEmpty(expression);
    }

    public static IRuleBuilderOptions<T, TProperty> NotEmptyWhenNotNull<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression)
    {
        return validator
            .RuleFor(expression)
            .NotEmptyWhenNotNull(expression);
    }

    public static IRuleBuilderOptions<T, TProperty> IsInEnum<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression)
    {
        return validator
            .RuleFor(expression)
            .IsInEnum(expression);
    }

    public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression)
    {
        return validator
            .RuleFor(expression)
            .NotNull(expression);
    }

    public static IRuleBuilderOptions<T, TProperty> NotFoundFor<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction)
    {
        return validator
            .RuleFor(expression)
            .NotFoundFor(expression, request, mustFunction);
    }

    public static IRuleBuilderOptions<T, TProperty> DuplicateKeyFor<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction)
    {
        return validator
            .RuleFor(expression)
            .DuplicateKeyFor(expression, request, mustFunction);
    }

    public static IRuleBuilderOptions<T, TProperty> NotExpired<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction)
    {
        return validator
            .RuleFor(expression)
            .NotExpired(expression, request, mustFunction);
    }

    public static IRuleBuilderOptions<T, TProperty> NotConfirmed<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction)
    {
        return validator
            .RuleFor(expression)
            .NotConfirmed(expression, request, mustFunction);
    }

    public static IRuleBuilderOptions<T, DateTime> GreaterThanMinimum<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, DateTime>> expression,
        Func<DateTime, bool> mustFunction,
        DateTime minimum)
    {
        return validator
            .RuleFor(expression)
            .GreaterThanMinimum(expression, mustFunction, minimum);
    }

    public static IRuleBuilderOptions<T, string> EmailAddress<T>(
        this AbstractValidator<T> validator,
        Expression<Func<T, string>> expression)
    {
        return validator
            .RuleFor(expression)
            .EmailAddress(expression);
    }

    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expressionToCompare,
        Expression<Func<T, TProperty>> expressionTarget)
    {
        return validator
            .RuleFor(expressionToCompare)
            .Equal(expressionToCompare, expressionTarget);
    }

    public static IRuleBuilderOptions<T, TProperty> EqualsToExpected<T, TProperty>(
        this AbstractValidator<T> validator,
        Expression<Func<T, TProperty>> expression,
        T request,
        Func<TProperty, bool> mustFunction)
    {
        return validator
            .RuleFor(expression)
            .EqualsToExpected(expression, request, mustFunction);
    }
}