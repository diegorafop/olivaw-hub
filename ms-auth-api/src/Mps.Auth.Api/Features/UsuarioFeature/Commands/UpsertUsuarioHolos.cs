using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class UpsertUsuarioHolosCommand
    : EntityCommand,
      IRequest<UpsertUsuarioHolosCommandResponse>
{
    public string Email { get; set; }
    public string Login { get; set; }
    public string Nome { get; set; }
    public IEnumerable<string> Autorizacoes { get; set; }
}

public class UpsertUsuarioHolosCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class UpsertUsuarioHolosCommandHandler
(
    IRepositoryUsuario repositoryUsuario,
    IRepository<Autorizacao> repositoryAutorizacao,
    IRepository<UsuarioAutorizacao> repositoryUsuarioAutorizacao,
    IMediator mediator
) : IRequestHandler<UpsertUsuarioHolosCommand,
    UpsertUsuarioHolosCommandResponse>
{
    private readonly IRepositoryUsuario _repositoryUsuario = repositoryUsuario
        ?? throw new ArgumentNullException(nameof(repositoryUsuario));

    private readonly IRepository<UsuarioAutorizacao> _repositoryUsuarioAutorizacao = repositoryUsuarioAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutorizacao));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    public async Task<UpsertUsuarioHolosCommandResponse> Handle
    (
        UpsertUsuarioHolosCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entity = await GetAsync
        (
            request,
            cancellationToken
        );

        var upsertedEntity = await UpsertAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repositoryUsuarioAutorizacao.SaveChangesAsync(cancellationToken);
        await _repositoryAutorizacao.SaveChangesAsync(cancellationToken);

        return upsertedEntity.Map<UpsertUsuarioHolosCommandResponse>();
    }

    private async Task<Usuario> InserirUsuarioAsync
    (
        UpsertUsuarioHolosCommand request,
        CancellationToken cancellationToken
    )
    {
        var command = request.ToInserirUsuarioInternalCommand();

        var response = await _mediator.Send(command, cancellationToken);

        await _repositoryUsuario.SaveChangesAsync(cancellationToken);

        await InserirAutorizacoesRequest
        (
            request,
            response.Usuario,
            cancellationToken
        );

        return response.Usuario;
    }

    private async Task InserirAutorizacoesRequest
    (
        UpsertUsuarioHolosCommand request,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        if (request.Autorizacoes != null && request.Autorizacoes.Any())
        {
            var predicate = request.SearchPredicateByList(request.Autorizacoes.ToList());
            var autorizacoes = await _repositoryAutorizacao.GetAsync(predicate, cancellationToken);

            foreach (var autorizacao in autorizacoes)
            {
                UsuarioAutorizacao usuarioAutorizacao = request.ToDomain(usuario, autorizacao);
                await _repositoryUsuarioAutorizacao.AddAsync(usuarioAutorizacao, cancellationToken);
            }
        }
    }

    private async Task<Usuario> UpsertAsync
    (
        UpsertUsuarioHolosCommand request,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        if (usuario is null)
            return await InserirUsuarioAsync(request, cancellationToken);

        else if (VerificarAutorizacoesRequest(request, usuario) || !usuario.IsEqualsTo(request))
        {
            await UpdateAsync
            (
                request,
                usuario,
                cancellationToken
            );
        }

        return usuario;
    }

    private async Task UpdateAsync
    (
        UpsertUsuarioHolosCommand request,
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        usuario.Update(request, request.UsuarioLogadoId);

        var rolesRequest = request.Autorizacoes ?? new List<string>();
        var rolesUsuario = usuario.Autorizacoes.ToList() ?? new List<UsuarioAutorizacao>();

        var vinculacoesParaRemover = rolesUsuario
            .Where(item => item.Autorizacao != null && !rolesRequest.Contains(item.Autorizacao.Valor))
            .ToList();

        foreach (var vinculacao in vinculacoesParaRemover)
            await _repositoryUsuarioAutorizacao.RemoveAsync
            (
                vinculacao,
                cancellationToken
            );

        var rolesAtuais = rolesUsuario
            .Where(item => item.Autorizacao != null)
            .Select(item => item.Autorizacao.Valor)
            .ToList();

        var rolesParaAdicionar = rolesRequest
            .Except(rolesAtuais)
            .ToList();

        if (rolesParaAdicionar.Any())
            await InserirAutorizacoesRequest
            (
                request,
                usuario,
                cancellationToken
            );

        await _repositoryUsuario.UpdateAsync
        (
            usuario,
            cancellationToken
        );

        await _repositoryUsuario.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> GetAsync
    (
        UpsertUsuarioHolosCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryUsuario.GetFirstOrDefaultAsync
        (
            SearchPredicate(request),
            cancellationToken,
            item => item.Autorizacoes
        );
    }

    private static Expression<Func<Usuario, bool>>
        SearchPredicate
    (
        UpsertUsuarioHolosCommand request
    )
    {
        return item => item
            .Login
            .Equals(request.Login);
    }

    private bool VerificarAutorizacoesRequest(UpsertUsuarioHolosCommand request, Usuario usuario)
    {
        var autorizacoesRequest = request.Autorizacoes ?? [];
        var autorizacoesAtuais = usuario.Autorizacoes.Select(item => item.Autorizacao.Nome).ToList() ?? [];

        if (autorizacoesRequest.Count() != autorizacoesAtuais.Count)
            return true;

        var listasDiferentes = autorizacoesRequest.Except(autorizacoesAtuais).Any() ||
                               autorizacoesAtuais.Except(autorizacoesRequest).Any();

        if (listasDiferentes)
            return true;

        return false;
    }
}

public class UpsertUsuarioHolosCommandValidator
    : AbstractValidator<UpsertUsuarioHolosCommand>
{
    public UpsertUsuarioHolosCommandValidator()
    {
        RuleFor(item => item.Login)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, string>
                (item => item.Login));

        RuleFor(item => item.Nome)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, string>
                (item => item.Nome));

        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, string>
                (item => item.Email));
    }
}