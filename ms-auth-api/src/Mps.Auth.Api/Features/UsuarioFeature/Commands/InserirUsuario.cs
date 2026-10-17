using FluentValidation;
using MassTransit;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Api.Features.UsuarioFeature.Internals;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class InserirUsuarioCommand
    : EntityCommand,
      IRequest<InserirUsuarioCommandResponse>
{
    public Guid? Id { get; set; }
    public string Login { get; set; }
    public string Senha { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public InserirUsuarioCommand() : base() { }
    public InserirUsuarioCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirUsuarioCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirUsuarioCommandHandler
(
    IRepositoryUsuario repository,
    IMediator mediator,
    IPublishEndpoint publisher,
    IConfiguration configuration
) : IRequestHandler<InserirUsuarioCommand,
    InserirUsuarioCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly IMediator _mediator = mediator
        ?? throw new ArgumentNullException(nameof(mediator));

    private readonly IPublishEndpoint _publisher = publisher
        ?? throw new ArgumentNullException(nameof(publisher));

    private readonly IConfiguration _configuration = configuration
        ?? throw new ArgumentNullException(nameof(configuration));

    public async Task<InserirUsuarioCommandResponse> Handle
    (
        InserirUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        var entity = await InserirUsuarioAsync(request, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        await PublishAsync(entity, cancellationToken);

        return entity.Map<InserirUsuarioCommandResponse>();
    }

    private async Task PublishAsync
    (
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        await _publisher.Publish
        (
            entity.ToEnfileirarUpsertDocumentoUsuarioPreferenciaContract(_configuration),
            cancellationToken
        );
    }

    private async Task<Usuario> InserirUsuarioAsync
    (
        InserirUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        var command = request.Convert<InserirUsuarioInternalCommand>();

        command.Id = request.Id;

        var response = await _mediator.Send(command, cancellationToken);

        return response.Usuario;
    }
}

public class InserirUsuarioCommandValidator
    : AbstractValidator<InserirUsuarioCommand>
{
    public InserirUsuarioCommandValidator()
    {
        RuleFor(item => item.Login)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, string>
                (item => item.Login));

        RuleFor(item => item.Senha)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirUsuarioCommand, string>
                (item => item.Senha));

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

    public async Task ValidateDataAsync
    (
        InserirUsuarioCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.Login)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Login));

        RuleFor(item => item.Nome)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Nome));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}