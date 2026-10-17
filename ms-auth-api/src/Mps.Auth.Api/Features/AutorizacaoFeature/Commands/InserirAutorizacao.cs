using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutorizacaoFeature.Commands;

public class InserirAutorizacaoCommand
    : EntityCommand,
      IRequest<InserirAutorizacaoCommandResponse>
{
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Perfil { get; set; }

    public InserirAutorizacaoCommand() : base() { }
    public InserirAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class InserirAutorizacaoCommandResponse
{
    public Guid Id { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class InserirAutorizacaoCommandHandler
(
    IRepository<Autorizacao> repository,
    InserirAutorizacaoCommandValidator validator
) : IRequestHandler<InserirAutorizacaoCommand,
    InserirAutorizacaoCommandResponse>
{
    private readonly IRepository<Autorizacao> _repository = repository
    ?? throw new ArgumentNullException(nameof(repository));

    private readonly InserirAutorizacaoCommandValidator _validator = validator
            ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InserirAutorizacaoCommandResponse> Handle
    (
        InserirAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Autorizacao entity = ToDomain(request);
        await ExistsAsync(request, cancellationToken);
        await AddAsync(entity, cancellationToken);

        return entity.Map<InserirAutorizacaoCommandResponse>();
    }

    private static Autorizacao ToDomain
    (
        InserirAutorizacaoCommand request
    )
    {
        return EntityExtensions
            .ToDomain<InserirAutorizacaoCommand, 
                Autorizacao>(request);
    }

    private async Task AddAsync
    (
        Autorizacao entity,
        CancellationToken cancellationToken
    )
    {
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task ExistsAsync
    (
        InserirAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );

        await _validator.ValidateDataAsync(request, exists);
    }
}

public class InserirAutorizacaoCommandValidator
    : AbstractValidator<InserirAutorizacaoCommand>
{
    public InserirAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Tipo)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirAutorizacaoCommand, string?>
                (item => item.Tipo));

        RuleFor(item => item.Valor)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<InserirAutorizacaoCommand, string?>
                (item => item.Valor));
    }

    public async Task ValidateDataAsync
    (
        InserirAutorizacaoCommand request,
        bool exists
    )
    {
        RuleFor(item => item.Tipo)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Tipo));

        RuleFor(item => item.Valor)
            .Must(item => { return !exists; })
            .WithMessage(MessageHelper
                .DuplicateKeyFor(request, item => item.Valor));

        await this.ValidateAndThrowAsync(request);
    }
}