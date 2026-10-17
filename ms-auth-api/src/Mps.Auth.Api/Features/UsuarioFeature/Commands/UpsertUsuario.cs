using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;

using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.Auth.Services;
using Mps.HolosCore.Api.Extensions.Collections;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class UpsertUsuarioCommand
    : EntityCommand,
      IRequest<IEnumerable<UpsertUsuarioCommandResponse>>
{
    public IEnumerable<UpsertUsuarioItemCommand> Itens { get; set; }

    public UpsertUsuarioCommand() { }
    public UpsertUsuarioCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class UpsertUsuarioItemCommand
{
    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }
}

public class UpsertUsuarioCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }

    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }
}

public class UpsertUsuarioCommandHandler
(
    IRepositoryUsuario repository,
    IServiceNotificacao service,
    IMediator mediator
) : IRequestHandler<UpsertUsuarioCommand,
    IEnumerable<UpsertUsuarioCommandResponse>>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IServiceNotificacao _service = service
        ?? throw new ArgumentNullException(nameof(service));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    public async Task<IEnumerable<UpsertUsuarioCommandResponse>> Handle
    (
        UpsertUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entityMany = await GetAsync
        (
            request,
            cancellationToken
        );

        var upsertedEntityMany = await UpsertAsync
        (
            request,
            entityMany,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        await TryNotificarUsuarioAsync
        (
            request,
            upsertedEntityMany,
            cancellationToken
        );

        return upsertedEntityMany.Select(item => item.Map<UpsertUsuarioCommandResponse>());
    }

    private async Task TryNotificarUsuarioAsync
    (
        UpsertUsuarioCommand request,
        IList<Usuario> entities,
        CancellationToken cancellationToken
    )
    {
        if (entities.Any())
        {
            var itens = entities
                .Select(item => item.ToEnfileirarUsuarioMessageCommand())
                .ToList();

            if (_service.Configured)
                await _service.EnfileirarUsuarioMessageLoteAsync
                (
                    itens,
                    cancellationToken
                );
        }
    }

    private async Task<Usuario> InserirUsuarioAsync
    (
        UpsertUsuarioCommand request,
        UpsertUsuarioItemCommand item,
        CancellationToken cancellationToken
    )
    {
        var command = request.ToInserirUsuarioInternalCommand(item);

        var response = await _mediator.Send(command, cancellationToken);

        return response.Usuario;
    }

    private async Task<IList<Usuario>> UpsertAsync
    (
        UpsertUsuarioCommand request,
        IEnumerable<Usuario> usuarioMany,
        CancellationToken cancellationToken
    )
    {
        var upsertedEntities = new List<Usuario>();

        foreach (var item in request.Itens)
        {
            var usuario = usuarioMany.FirstOrDefault(GetFirstOrDefaultSearchPredicate(item));

            if (usuario is null)
            {
                upsertedEntities.Add(await InserirUsuarioAsync(request, item, cancellationToken));
            }
            else if
            (
                !usuario.IsEqualsTo(item)
                && !usuario.EqualsPendenteProcessoAdmissao()
            )
            {
                await UpdateAsync
                (
                    request,
                    item,
                    usuario,
                    cancellationToken
                );

                upsertedEntities.Add(usuario);
            }
        }

        return upsertedEntities;
    }

    private async Task UpdateAsync
    (
        UpsertUsuarioCommand request,
        UpsertUsuarioItemCommand item,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        usuario.Update(item, request.UsuarioLogadoId);
        await _repository.UpdateAsync(usuario, cancellationToken);
    }

    private async Task<IEnumerable<Usuario>> GetAsync
    (
        UpsertUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        int batchSize = 1000;

        var predicates = request
            .Itens
            .GetManyByBatch(batchSize)
            .Select(item => SearchPredicate(item));

        var results = new List<Usuario>();

        foreach (var item in predicates)
            results.AddRange
            (
                await _repository.GetAsync
                (
                    item,
                    cancellationToken
                )
            );

        return results.DistinctBy(item => item.Id);
    }

    private static Func<Usuario, bool>
        GetFirstOrDefaultSearchPredicate
    (
        UpsertUsuarioItemCommand request
    )
    {
        return item =>
            item
                .Login
                .ToUpper()
                .Equals(request.Login.ToUpper()) ||
            item
                .Email
                .ToUpper()
                .Equals(request.Email.ToUpper()) ||
            (
                item.IntegracaoId != null &&
                request.IntegracaoId != null &&
                item
                    .IntegracaoId
                    .ToUpper()
                    .Equals(request.IntegracaoId)
            );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate
    (
        IEnumerable<UpsertUsuarioItemCommand> itens
    )
    {
        var logins = itens
            .Select(item => item.Login.ToUpper())
            .ToList();

        var emails = itens
            .Select(item => item.Email.ToUpper())
            .ToList();

        var integracaoIds = itens
            .Where(item => !string.IsNullOrEmpty(item.IntegracaoId))
            .Select(item => item.IntegracaoId!.ToUpper())
            .ToList();

        return item =>
            logins.Contains(item.Login.ToUpper()) ||
            emails.Contains(item.Email.ToUpper()) ||
            (
                item.IntegracaoId != null &&
                integracaoIds.Contains(item.IntegracaoId.ToUpper())
            );
    }
}

public class UpsertUsuarioCommandValidator
    : AbstractValidator<UpsertUsuarioCommand>
{
    public UpsertUsuarioCommandValidator()
    {
        RuleFor(item => item.Itens)
            .ForEach(item =>
            {
                item.ChildRules(item =>
                {
                    item
                        .RuleFor(item => item.Login)
                        .NotEmpty()
                        .WithMessage(MessageHelper
                            .EmptyFor<UpsertUsuarioItemCommand, string>
                            (item => item.Login));

                    item
                        .RuleFor(item => item.Nome)
                        .NotEmpty()
                        .WithMessage(MessageHelper
                            .EmptyFor<UpsertUsuarioItemCommand, string>
                            (item => item.Nome));

                    item
                        .RuleFor(item => item.Email)
                        .NotEmpty()
                        .WithMessage(MessageHelper
                            .EmptyFor<UpsertUsuarioItemCommand, string>
                            (item => item.Email));
                });
            });

        RuleFor(item => item.UsuarioLogadoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, Guid>
                (item => item.UsuarioLogadoId));
    }
}