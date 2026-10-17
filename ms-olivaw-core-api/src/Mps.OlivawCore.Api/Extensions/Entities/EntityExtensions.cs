using MediatR;
using Mps.OlivawCore.Contracts;
using Mps.OlivawCore.Domain;
using System.Collections;
using System.Reflection;

namespace Mps.OlivawCore.Api.Extensions.Entities;

public static class EntityExtensions
{
    private static readonly IList<string> _entityPropertyNames = [.. typeof(Entity)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(item => item.Name)];

    public static T Map<T>
    (
        this Entity entity
    )
        where T : class, new()
    {
        return Convert<Entity, T>(entity);
    }

    public static T Map<T>
    (
        this BaseContract contract
    )
        where T : class, new()
    {
        return Convert<BaseContract, T>(contract);
    }

    public static T ToDomain<T>
    (
        this IBaseRequest from
    )
        where T : Entity, new()
    {
        var entity = Convert<IBaseRequest, T>(from);

        entity.IdExterno = Guid.NewGuid();
        entity.DataCadastro = DateTime.UtcNow;
        entity.DataAtualizacao = null;

        return entity;
    }

    public static T ToDomain<F, T>
    (
        F from
    )
        where F : class
        where T : Entity, new()
    {
        var entity = Convert<F, T>(from);

        entity.IdExterno = Guid.NewGuid();
        entity.DataCadastro = DateTime.UtcNow;
        entity.DataAtualizacao = null;

        return entity;
    }

    public static T ToDomain<F, T>
    (
        F from,
        Guid usuarioLogadoId
    )
        where F : class
        where T : Entity, new()
    {
        var entity = Convert<F, T>(from);

        entity.IdExterno = Guid.NewGuid();
        entity.DataCadastro = DateTime.UtcNow;
        entity.DataAtualizacao = null;
        entity.UsuarioLogadoId = usuarioLogadoId;

        return entity;
    }

    public static T ToDomain<T>
    (
        Guid usuarioLogadoId
    )
        where T : Entity, new()
    {
        return new T()
        {
            IdExterno = Guid.NewGuid(),
            DataCadastro = DateTime.UtcNow,
            DataAtualizacao = null,
            UsuarioLogadoId = usuarioLogadoId
        };
    }

    public static T Replicate<T>
    (
        this Entity entity,
        Guid usuarioLogadoId
    )
        where T : Entity, new()
    {
        var clone = entity.Map<T>();

        clone.IdExterno = Guid.NewGuid();
        clone.DataCadastro = DateTime.UtcNow;
        clone.DataAtualizacao = null;
        clone.UsuarioLogadoId = usuarioLogadoId;

        return clone;
    }

    public static void Update
    (
        this Entity entity,
        Guid usuarioLogadoId
    )
    {
        entity.DataAtualizacao = DateTime.UtcNow;
        entity.UsuarioLogadoId = usuarioLogadoId;
    }

    public static void Update<T>
    (
        this T entity,
        IBaseRequest request
    )
        where T : Entity, new()
    {
        Copy(request, entity);

        entity.DataAtualizacao = DateTime.UtcNow;
    }

    public static void Update<T>
    (
        this T entity,
        object objeto,
        Guid usuarioLogadoId
    )
        where T : Entity, new()
    {
        Copy(objeto, entity);

        entity.DataAtualizacao = DateTime.UtcNow;
        entity.UsuarioLogadoId = usuarioLogadoId;
    }

    public static void Toggle
    (
        this Entity entity,
        bool toggle,
        Guid usuarioLogadoId
    )
    {
        var data = DateTime.UtcNow;

        entity.DataRemocao = toggle ? null : data;
        entity.DataAtualizacao = data;
        entity.UsuarioLogadoId = usuarioLogadoId;
    }

    public static void Toggle
    (
        this IEnumerable<Entity> entities,
        bool toggle,
        Guid usuarioLogadoId
    )
    {
        foreach (var entity in entities)
            entity.Toggle(toggle, usuarioLogadoId);
    }

    public static T Convert<F, T>
    (
        F from
    )
        where F : class
        where T : class, new()
    {
        var target = new T();

        Copy(from, target);

        SetIfContract(target);

        return target;
    }

    public static T Convert<T>
    (
        this IBaseRequest from
    ) where T : class, new()
    {
        var target = new T();

        Copy(from, target);

        SetIfContract(target);

        return target;
    }

    public static T Convert<T>
    (
        this IConversivel from
    ) where T : class, new()
    {
        var target = new T();

        Copy(from, target);

        SetIfContract(target);

        return target;
    }

    private static void Copy<F, T>
    (
        F from,
        T target
    )
        where T : class, new()
    {
        var targetProperties = target
            .GetType()
            .GetRuntimeProperties();

        foreach (PropertyInfo propertyTarget in targetProperties)
        {
            PropertyInfo propertyFrom = Get(from, MapName(propertyTarget.Name));
            if (IsValid(from, propertyFrom, propertyTarget))
                SetValue(from, target, propertyFrom, propertyTarget);
        }
    }

    private static bool TryGetTypeDefaultValue
    (
        Type? propertyType,
        out object? defaultValue
    )
    {
        if (propertyType == null)
        {
            defaultValue = null;
            return true;
        }

        if (IsNullable(propertyType))
        {
            defaultValue = null;
            return true;
        }

        if (propertyType == typeof(Guid))
        {
            defaultValue = Guid.Empty;
            return true;
        }

        defaultValue = null;
        return false;
    }

    private static void SetValue<F, T>
    (
        F from,
        T target,
        PropertyInfo propertyFrom,
        PropertyInfo propertyTarget
    )
    {
        object? value = propertyFrom.GetValue(from);

        Type propertyFromType = propertyFrom.PropertyType;
        Type propertyTargetType = propertyTarget.PropertyType;

        bool hasTypeDefaultValue = TryGetTypeDefaultValue
        (
            GetUnderlyingTypeIfNullableOrType(propertyFrom),
            out object? defaultValue
        );

        bool isTypeDefaultValue = hasTypeDefaultValue && (value?.Equals(defaultValue) ?? false);
        bool isNull = value is null;

        if (!isNull && !isTypeDefaultValue)
            propertyTarget.SetValue(target, propertyFrom.GetValue(from));
        else
            if (IsNullable(propertyFromType) && !IsNullable(propertyTargetType))
                propertyTarget.SetValue(target, isNull ? defaultValue : value);
            else if (!IsNullable(propertyFromType) && IsNullable(propertyTargetType))
                propertyTarget.SetValue(target, isTypeDefaultValue ? null : value);
            else
                propertyTarget.SetValue(target, value);
    }

    private static void SetIfContract<T>
    (
        T target
    ) where T : class
    {
        if (target is BaseContract contract)
            contract.TraceId = Guid.NewGuid();
    }

    private static bool IsValid<F>
    (
        F from,
        PropertyInfo propertyFrom,
        PropertyInfo propertyTarget
    )
    {
        return propertyFrom != null &&
            !IsCollection(propertyFrom) &&
            !(propertyFrom.PropertyType.BaseType == typeof(Entity)) &&
            IsMapped(from, propertyFrom, propertyTarget);
    }

    private static bool IsMapped<F>
    (
        F from,
        PropertyInfo propertyFrom,
        PropertyInfo propertyTarget
    )
    {
        Type propertyFromType = propertyFrom.PropertyType;
        Type propertyTargetType = propertyTarget.PropertyType;

        Type? propertyFromUnderlyingType = GetUnderlyingType(propertyFromType);
        Type? propertyTargetUnderlyingType = GetUnderlyingType(propertyTargetType);

        if (propertyFromType == propertyTargetType)
            return true;
        else if (IsNullable(propertyTargetType) && propertyFromType == propertyTargetUnderlyingType)
            return true;
        else if (IsNullable(propertyFromType) && propertyFromUnderlyingType == propertyTargetType)
            return
                propertyFrom.GetValue(from) is not null ||
                TryGetTypeDefaultValue(propertyFromUnderlyingType, out _);
        else
            return false;
    }

    public static bool IsEqualsTo
    (
        this Entity entity,
        object comparedObject
    )
    {
        var comparedObjectProperties = comparedObject
            .GetType()
            .GetRuntimeProperties();

        foreach (PropertyInfo comparedObjectProperty in comparedObjectProperties)
        {
            if (_entityPropertyNames.Contains(comparedObjectProperty.Name))
                continue;

            PropertyInfo entityProperty = Get(entity, comparedObjectProperty.Name);
            if (IsValid(comparedObject, entityProperty, comparedObjectProperty))
            {
                var entityValue = entityProperty.GetValue(entity);
                var comparedObjectValue = comparedObjectProperty.GetValue(comparedObject);

                if (!AreValuesEqual(entityValue, comparedObjectValue))
                    return false;
            }
        }

        return true;
    }

    private static bool AreValuesEqual
    (
        object? value1,
        object? value2
    )
    {
        if (value1 is null && value2 is null)
            return true;

        if (value1 is null || value2 is null)
            return false;

        return value1.Equals(value2);
    }

    private static Type? GetUnderlyingType
    (
        Type propertyType
    )
    {
        return Nullable.GetUnderlyingType(propertyType);
    }

    private static Type? GetUnderlyingTypeIfNullableOrType
    (
        PropertyInfo propertyInfo
    )
    {
        var type = propertyInfo.PropertyType;

        if (IsNullable(type))
            return Nullable.GetUnderlyingType(type);

        return type;
    }

    private static bool IsNullable(Type propertyType)
    {
        return Nullable.GetUnderlyingType(propertyType) is not null;
    }

    private static bool IsCollection
    (
        PropertyInfo property
    )
    {
        return property.PropertyType != typeof(string)
            && typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
    }

    private static string MapName
    (
        string name
    )
    {
        return name switch
        {
            "Id" => "IdExterno",
            _ => name,
        };
    }

    private static PropertyInfo Get<T>
    (
        T o,
        string name
    )
    {
        if (o is null)
            return null;

        if (string.IsNullOrWhiteSpace(name))
            return null;

        Type type = o.GetType();

        PropertyInfo? property = type.GetProperty
        (
            name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (property is null)
            return null;

        return property;
    }
}