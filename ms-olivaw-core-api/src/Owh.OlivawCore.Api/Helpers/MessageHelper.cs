using System.Linq.Expressions;
using System.Reflection;

namespace Owh.OlivawCore.Api.Helpers;

public static class MessageHelper
{
    public static string NotFoundFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' não encontrado.";
    }

    public static string NotFoundFor<T, TProperty>
    (
        T o,
        params Expression<Func<T, TProperty>>[] propertyMany
    )
    {
        var itemMany = new List<string>();

        foreach (var item in propertyMany)
            itemMany.Add($"{GetName(item)} = '{GetValue(o, item)}'");

        return $"{typeof(T).Name} ({string.Join("; ", itemMany)}) não encontrado.";
    }

    public static string ServiceErrorFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' inválido.";
    }

    public static string InvalidFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' inválido.";
    }

    public static string InvalidFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        string value = "'{PropertyValue}'";
        return $"{typeof(T).Name}.{GetName(property)}: {value} é inválido.";
    }

    public static string EmptyFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' está vazio.";
    }

    public static string InvalidCastForFile<T>()
    {
        return $"O tipo {typeof(T).Name} não herda de ConteudoArquivoQueryResponse";
    }

    public static string UnauthorizedFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' inautorizado.";
    }

    public static string DuplicateKeyFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' já cadastrado.";
    }

    public static string DuplicateKeyFor<T, TProperty>
    (
        T o,
        params Expression<Func<T, TProperty>>[] propertyMany
    )
    {
        var itemMany = new List<string>();

        foreach (var item in propertyMany)
            itemMany.Add($"{GetName(item)} = '{GetValue(o, item)}'");

        return $"{typeof(T).Name} ({string.Join("; ", itemMany)}) já cadastrado.";
    }

    public static string ExpiredFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' expirado.";
    }

    public static string ConfirmedFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' já foi confirmado.";
    }

    public static string InvalidJsonFormatFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' não possui formato Json válido.";
    }

    public static string NullFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é nulo.";
    }

    public static string NullFor<T>()
    {
        return $"'{nameof(T)}' é nulo.";
    }

    public static string OutOfRange<T, TProperty>
    (
        Expression<Func<T, TProperty>> property,
        int minimum,
        int maximum
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' está fora do intervalo [{minimum}, {maximum}].";
    }

    public static string LowerThanMinimum<T, TProperty>
    (
        Expression<Func<T, TProperty>> property,
        int minimum
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é menor que {minimum}.";
    }

    public static string LowerThanMinimum<T>
    (
        Expression<Func<T, DateTime>> property,
        DateTime minimum
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é menor que {minimum}.";
    }

    public static string GreaterThanMaximum<T, TProperty>
    (
        Expression<Func<T, TProperty>> property,
        int maximum
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é maior que {maximum}.";
    }

    public static string ForeignKeyFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property,
        string reference
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' é referenciado por {reference}";
    }

    public static string NotFoundFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property,
        object value
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{value}' não encontrado.";
    }

    public static string EqualFor<T, F, G>
    (
        Expression<Func<T, F>> property,
        Expression<Func<T, G>> propertyFrom
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é diferente do campo '{typeof(T).Name}.{GetName(propertyFrom)}'.";
    }

    public static string EmailFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        return $"Campo '{typeof(T).Name}.{GetName(property)}' é um e-mail inválido.";
    }

    public static string DifferentThanExpectedFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' é diferente do valor esperado.";
    }

    public static string? GetName<T, F>
    (
        Expression<Func<T, F>> property
    )
    {
        return ReflectionHelper.GetName(property);
    }

    public static string? GetValue<T, F>
    (
        T o,
        Expression<Func<T, F>> property
    )
    {
        return ReflectionHelper.GetValue(o, property);
    }

    public static string AuthenticationFor<T, F>
    (
        T o, Expression<Func<T, F>> property
    )
    {
        return $"{typeof(T).Name}.Login: '{GetValue(o, property)}' - Usuário e/ou senha são inválidos.";
    }

    public static string AuthenticationFor<T>()
    {
        return $"{typeof(T).Name}.Autorizações: Usuário sem nenhuma autorização.";
    }

    public static string DuplicateAuthentication<T, TProperty>
    (
        T o, Expression<Func<T, TProperty>> property
    )
    {
        return $"{typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' já cadastrado.";
    }

    public static string OnlyNumberFor<T, TProperty>
    (
        Expression<Func<T, TProperty>> property
    )
    {
        string value = "'{PropertyValue}'";
        return $"{typeof(T).Name}.{GetName(property)}: {value} deve conter apenas números.";
    }

    public static string OnlyNumberFor<T, TProperty>
    (
        T o,
        Expression<Func<T, TProperty>> property
    )
    {
        return $"Campo {typeof(T).Name}.{GetName(property)}: '{GetValue(o, property)}' deve conter apenas números.";
    }

    public static string InvalidSettingFor<T>()
    {
        var field = typeof(T)
            .GetField
            (
                "ConfigurationSection",
                BindingFlags.Public
                | BindingFlags.Static
                | BindingFlags.FlattenHierarchy
            );

        string sectionName = field
            ?.GetValue(null)
            ?.ToString()
            ?? "UnknownSection";

        return $"Configuration {typeof(T).Name}.ConfigurationSection = " +
            $"{sectionName} é inválido ou está faltando no appsettings.";
    }
}