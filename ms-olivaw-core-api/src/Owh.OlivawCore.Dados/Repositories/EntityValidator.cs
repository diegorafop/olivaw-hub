using FluentValidation;
using Owh.OlivawCore.Domain;

namespace Owh.OlivawCore.Dados.Repositories;

public class EntityValidator<T>
    : AbstractValidator<T> where T : Entity
{
    public async Task NotFoundAsync
    (
        T entity,
        CancellationToken cancellationToken
    )
    {
        var instance = EntityValidator<T>.GetInstance(entity);

        var validator = new InlineValidator<T>();

        validator.RuleFor(item => item.Id)
            .Must(item => { return entity != null; })
            .WithMessage(NotFoundMessage(entity));

        await validator.ValidateAndThrowAsync
        (
            instance,
            cancellationToken
        );
    }

    public string NotFoundMessage
    (
        T entity
    )
    {
        var instance = EntityValidator<T>.GetInstance(entity);

        if (instance is null)
            return string.Empty;

        return $"{instance.GetType().Name} não encontrado(a).";
    }

    private static T GetInstance
    (
        T entity
    )
    {
        return entity ?? Activator.CreateInstance<T>();
    }
}
