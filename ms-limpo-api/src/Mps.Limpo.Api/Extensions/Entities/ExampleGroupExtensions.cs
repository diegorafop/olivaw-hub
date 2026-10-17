using Mps.OlivawCore.Api.Extensions.Collections;
using Mps.OlivawCore.Api.Extensions.Entities;
using Mps.Limpo.Api.Features.ExampleFeature.Commands;
using Mps.Limpo.Api.Features.ExampleFeature.Queries;
using Mps.OlivawCore.Domain;
using System.Linq.Expressions;

namespace Mps.Limpo.Api.Extensions.Entities;

public static class ExampleExtensions
{
    public static Example ToDomain
    (
        this InserirExampleCommand request
    )
    {
        return EntityExtensions
            .ToDomain<InserirExampleCommand,
                Example>(request);
    }

    public static Expression<Func<Example, bool>> SearchPredicateId
    (
        this AtualizarExampleCommand request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }

    public static Expression<Func<Example, bool>> SearchPredicateByName
    (
        this AtualizarExampleCommand request
    )
    {
        return item => item
            .Name
            .ToUpper()
            .Equals(request
                .Name
                .ToUpper())
            && !item
                .IdExterno
                .Equals(request.Id);
    }

    public static Expression<Func<Example, bool>> SearchPredicate
    (
        this AlternarExampleCommand request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }

    public static void Update
    (
        this AtualizarExampleCommand request,
        Example entity
    )
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Permission = request.Permission;
        entity.UsuarioLogadoId = request.UsuarioLogadoId;

        entity.Update(request.UsuarioLogadoId);
    }

    public static Expression<Func<Example, bool>> SearchPredicate
    (
        this SelecionarExampleByIdQuery request
    )
    {
        return item => item
            .IdExterno
            .Equals(request.Id);
    }

    public static Expression<Func<Example, bool>> SearchPredicate
    (
        this SelecionarExampleByFilterQuery request
    )
    {
        Expression<Func<Example, bool>> predicate = item => true;

        if (!string.IsNullOrEmpty(request.Name))
            predicate = predicate.And(item => item
                .Name
                .ToUpper()
                .Contains(request
                    .Name
                    .ToUpper()));

        if (!string.IsNullOrEmpty(request.Description))
            predicate = predicate.And(item => item
                .Description
                .ToUpper()
                .Contains(request
                    .Description
                    .ToUpper()));

        predicate = predicate.And(item => item
                .Permission
                .Equals(request.Permission));

        return predicate;
    }
}
