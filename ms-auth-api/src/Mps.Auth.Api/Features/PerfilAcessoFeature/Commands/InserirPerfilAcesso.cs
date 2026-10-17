using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.PerfilAcessoFeature.Commands;

public class InserirPerfilAcessoCommand
    : EntityCommand,
      IRequest<InserirPerfilAcessoCommandResponse>
{
    public string Nome { get; set; }
    public string? Descricao { get; set; }

    public InserirPerfilAcessoCommand() : base() { }
    public InserirPerfilAcessoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirPerfilAcessoCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirPerfilAcessoCommandHandler
(
    IRepository<PerfilAcesso> repository,
    InserirPerfilAcessoCommandValidator validator
) : IRequestHandler<InserirPerfilAcessoCommand,
    InserirPerfilAcessoCommandResponse>
{
    private readonly IRepository<PerfilAcesso> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly InserirPerfilAcessoCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirPerfilAcessoCommandResponse> Handle
    (
        InserirPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        PerfilAcesso entity = EntityExtensions
            .ToDomain<InserirPerfilAcessoCommand,
                PerfilAcesso>(request);

        await ExistsAsync(request, cancellationToken);
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirPerfilAcessoCommandResponse>();
    }

    private async Task AddAsync
    (
        PerfilAcesso entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ExistsAsync
    (
        InserirPerfilAcessoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            item => item
                .Nome
                .Equals(request.Nome),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }
}

public class InserirPerfilAcessoCommandValidator
    : AbstractValidator<InserirPerfilAcessoCommand>
{
    public InserirPerfilAcessoCommandValidator()
    {
        RuleFor(item => item.Nome)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirPerfilAcessoCommand, string>
                (item => item.Nome));
    }

    public async Task ValidateDataAsync
    (
        InserirPerfilAcessoCommand request,
        bool exists,
        CancellationToken cancellationToken
    )
    {
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