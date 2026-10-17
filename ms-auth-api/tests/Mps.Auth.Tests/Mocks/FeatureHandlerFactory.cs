using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Linq;

namespace Mps.Auth.Tests.Mocks;

internal static class FeatureHandlerFactory
{
    public static THandler Create<THandler>()
    {
        var handlerType = typeof(THandler);
        var constructor = handlerType.GetConstructors()[0];
        var arguments = constructor
            .GetParameters()
            .Select(item => CreateArgument(item.ParameterType))
            .ToArray();

        return (THandler)Activator.CreateInstance(handlerType, arguments)!;
    }

    private static object CreateArgument(Type parameterType)
    {
        if (parameterType == typeof(IConfiguration))
            return new FakerBuilder().Configuration;

        if (parameterType.IsInterface)
        {
            var mockType = typeof(Mock<>).MakeGenericType(parameterType);
            var mock = Activator.CreateInstance(mockType)!;

            foreach (var property in mockType.GetProperties())
            {
                if (property.Name != "Object")
                    continue;

                if (property.PropertyType != parameterType)
                    continue;

                return property.GetValue(mock)!;
            }

            throw new InvalidOperationException
            (
                $"Não foi possível obter o mock para {parameterType.Name}."
            );
        }

        return Activator.CreateInstance(parameterType)!;
    }
}
