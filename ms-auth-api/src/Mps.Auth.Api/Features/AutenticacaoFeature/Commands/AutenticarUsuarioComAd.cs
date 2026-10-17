using FluentValidation;
using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Model;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class AutenticarUsuarioComAdCommand
    : EntityCommand,
      IRequest<AutenticarUsuarioComAdCommandResponse>
{
    internal string IdToken { get; set; }

    public string? Nome { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Matricula { get; set; }

    public AutenticarUsuarioComAdCommand() { }
    public AutenticarUsuarioComAdCommand
    (
        string idToken,
        Guid usuarioLogadoId
    )
        : base(usuarioLogadoId)
    {
        IdToken = idToken;
    }
}

public class AutenticarUsuarioComAdCommandResponse
{
    public string Token { get; set; }
    public string TokenRefresh { get; set; }
}

public class AutenticarUsuarioComAdCommandHandler
(
    IPublishEndpoint publisher,
    IConfiguration configuration,
    IMediator mediator,
    IRepositoryUsuario repository,
    IRepository<UsuarioAutenticacao> repositoryUsuarioAutenticacao,
    IRepository<UsuarioAutorizacao> repositoryUsuarioAutorizacao,
    AutenticarUsuarioComAdCommandValidator validator
) : IRequestHandler<AutenticarUsuarioComAdCommand,
    AutenticarUsuarioComAdCommandResponse>
{
    private readonly IPublishEndpoint _publisher = publisher
        ?? throw new ArgumentNullException(nameof(publisher));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IRepository<UsuarioAutenticacao> _repositoryUsuarioAutenticacao = repositoryUsuarioAutenticacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutenticacao));

    private readonly IRepository<UsuarioAutorizacao> _repositoryUsuarioAutorizacao = repositoryUsuarioAutorizacao
        ?? throw new ArgumentNullException(nameof(repositoryUsuarioAutorizacao));

    private readonly AutenticarUsuarioComAdCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AutenticarUsuarioComAdCommandResponse> Handle
    (
        AutenticarUsuarioComAdCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var usuarioModel = await ValidateDataAsync(request, cancellationToken);

        var entity = await GetUsuarioAsync
        (
            request,
            usuarioModel,
            cancellationToken
        );

        var usuarioAutenticacao = await UpsertAsync(request, entity, cancellationToken);

        return entity.ToAutenticarUsuarioComAdCommandResponse
        (
            usuarioAutenticacao.Token,
            _configuration
        );
    }

    private async Task<UsuarioModel> ValidateDataAsync
    (
        AutenticarUsuarioComAdCommand request,
        CancellationToken cancellationToken
    )
    {
        var usuarioModel = request.ToUsuarioModel(_configuration);

        await _validator.ValidateDataAsync
        (
            request,
            usuarioModel,
            cancellationToken
        );

        return usuarioModel;
    }

    private async Task<Usuario> GetUsuarioAsync
    (
        AutenticarUsuarioComAdCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        var usuario = await GetFirstOrDefaultAsync(usuarioModel, cancellationToken);

        if (usuario is not null)
            return usuario;

        var command = request.ToInserirUsuarioInternalCommand(_configuration);
        var response = await _mediator.Send(command, cancellationToken);

        var usuarioAutorizacaoMany = response
            .Usuario
            .ToManyUsuarioAutorizacao(request, _configuration);

        await _repositoryUsuarioAutorizacao.AddCollectionAsync
        (
            usuarioAutorizacaoMany,
            cancellationToken
        );

        return response.Usuario;
    }

    private async Task<UsuarioAutenticacao> UpsertAsync
    (
        AutenticarUsuarioComAdCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        UsuarioAutenticacao usuarioAutenticacao = await UpsertUsuarioAsync
        (
            request,
            entity,
            cancellationToken
        );

        await _repository.SaveChangesAsync(cancellationToken);

        return usuarioAutenticacao;
    }

    private async Task<Usuario?> GetFirstOrDefaultAsync
    (
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstOrDefaultAsync
        (
            item => item
                .Login
                .Equals(usuarioModel.Login),
            cancellationToken,
            item => item.Autorizacoes,
            item => item.Perfils
        );
    }

    public async Task<UsuarioAutenticacao> UpsertUsuarioAsync
    (
        AutenticarUsuarioComAdCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        var usuarioAutenticacao = entity.ToUsuarioAutenticacao();

        if (entity.Id != 0)
        {
            entity.Update(request, _configuration);
            await _repository.UpdateAsync(entity, cancellationToken);
        }
        else
            await EnfileirarUpsertDestinatarioAsync(entity, cancellationToken);

        await _repositoryUsuarioAutenticacao.AddAsync
        (
            usuarioAutenticacao,
            cancellationToken
        );

        return usuarioAutenticacao;
    }

    private async Task EnfileirarUpsertDestinatarioAsync
    (
        Usuario usuario,
        CancellationToken cancellationToken
    )
    {
        var contract = usuario.ToEnfileirarUpsertDestinatarioContract();
        await _publisher.Publish(contract, cancellationToken);
    }
}

public class AutenticarUsuarioComAdCommandValidator
    : AbstractValidator<AutenticarUsuarioComAdCommand>
{
    public AutenticarUsuarioComAdCommandValidator()
    {
        RuleFor(item => item.IdToken)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AutenticarUsuarioComAdCommand, string>
                (item => item.IdToken));
    }

    public async Task ValidateDataAsync
    (
        AutenticarUsuarioComAdCommand request,
        UsuarioModel usuarioModel,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Nome))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Nome));

        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Email))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Email));

        RuleFor(item => item.IdToken)
            .Must(item => !string.IsNullOrEmpty(usuarioModel.Login))
            .WithMessage(MessageHelper
                .EmptyFor<Usuario, string>
                (item => item.Login));

        await this.ValidateAndThrowAsync(request, cancellationToken);
    }
}