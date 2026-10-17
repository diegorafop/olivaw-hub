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

namespace Mps.Auth.Api.Features.UsuarioAutorizacaoFeature.Commands;

public class InserirUsuarioAutorizacaoCommand
    : EntityCommand,
      IRequest<InserirUsuarioAutorizacaoCommandResponse>
{
    public Guid AutorizacaoId { get; set; }
    public Guid UsuarioId { get; set; }

    public InserirUsuarioAutorizacaoCommand() : base() { }
    public InserirUsuarioAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirUsuarioAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirUsuarioAutorizacaoCommandHandler
(
    IRepository<UsuarioAutorizacao> repository,
    IRepositoryUsuario repositoryUsuario,
    IRepository<Autorizacao> repositoryAutorizacao,
    InserirUsuarioAutorizacaoCommandValidator validator
) : IRequestHandler<InserirUsuarioAutorizacaoCommand,
    InserirUsuarioAutorizacaoCommandResponse>
{
    private readonly IRepository<UsuarioAutorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepositoryUsuario _repositoryUsuario = repositoryUsuario
        ?? throw new ArgumentNullException(nameof(repositoryUsuario));

    private readonly IRepository<Autorizacao> _repositoryAutorizacao = repositoryAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryAutorizacao));

    private readonly InserirUsuarioAutorizacaoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirUsuarioAutorizacaoCommandResponse> Handle
    (
        InserirUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsAsync(request, cancellationToken);
        UsuarioAutorizacao entity = await GetAsync(request, cancellationToken);
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirUsuarioAutorizacaoCommandResponse>();
    }

    private async Task AddAsync
    (
        UsuarioAutorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<UsuarioAutorizacao> GetAsync
    (
        InserirUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        Usuario usuario = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        Autorizacao autorizacao = await GetFirstAsync
        (
            request.AutorizacaoId,
            cancellationToken
        );

        return request.ToDomain
        (
            usuario,
            autorizacao
        );
    }

    private async Task<Autorizacao> GetFirstAsync
    (
        Guid autorizacaoId,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryAutorizacao.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(autorizacaoId),
            cancellationToken
        );
    }

    private async Task<Usuario> GetFirstAsync
    (
        InserirUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repositoryUsuario.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.UsuarioId),
            cancellationToken
        );
    }

    private async Task ExistsAsync
    (
        InserirUsuarioAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            SeatchPredicate(request),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }

    private static Expression<Func<UsuarioAutorizacao, bool>> SeatchPredicate
    (
        InserirUsuarioAutorizacaoCommand request
    )
    {
        return item =>
            item.Usuario.IdExterno.Equals(request.UsuarioId) &&
            item.Autorizacao.IdExterno.Equals(request.AutorizacaoId);
    }
}

public class InserirUsuarioAutorizacaoCommandValidator
    : AbstractValidator<InserirUsuarioAutorizacaoCommand>
{
    public InserirUsuarioAutorizacaoCommandValidator()
    {
        RuleFor(item => item.UsuarioId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioAutorizacaoCommand, Guid>
                (item => item.UsuarioId));

        RuleFor(item => item.AutorizacaoId)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioAutorizacaoCommand, Guid>
                (item => item.AutorizacaoId));
    }

    public async Task ValidateDataAsync
    (
        InserirUsuarioAutorizacaoCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.AutorizacaoId)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.AutorizacaoId));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}