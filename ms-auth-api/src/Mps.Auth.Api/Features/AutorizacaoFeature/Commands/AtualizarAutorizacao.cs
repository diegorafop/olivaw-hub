using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutorizacaoFeature.Commands;

public class AtualizarAutorizacaoCommand
    : EntityCommand,
      IRequest<AtualizarAutorizacaoCommandResponse>
{
    public Guid Id { get; set; }
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Perfil { get; set; }

    public AtualizarAutorizacaoCommand() : base() { }
    public AtualizarAutorizacaoCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarAutorizacaoCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarAutorizacaoCommandHandler
(
    IRepository<Autorizacao> repository
) : IRequestHandler<AtualizarAutorizacaoCommand,
    AtualizarAutorizacaoCommandResponse>
{
    private readonly IRepository<Autorizacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<AtualizarAutorizacaoCommandResponse> Handle
    (
        AtualizarAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Autorizacao entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await UpdateAsync
        (
            request, 
            entity, 
            cancellationToken
        );

        return entity.Map<AtualizarAutorizacaoCommandResponse>();
    }

    private async Task UpdateAsync
    (
        AtualizarAutorizacaoCommand request, 
        Autorizacao entity, 
        CancellationToken cancellationToken
    )
    {
        request.Update(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Autorizacao> GetFirstAsync
    (
        AtualizarAutorizacaoCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );
    }
}

public class AtualizarAutorizacaoCommandValidator
    : AbstractValidator<AtualizarAutorizacaoCommand>
{
    public AtualizarAutorizacaoCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarAutorizacaoCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.Tipo)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarAutorizacaoCommand, string>
                (item => item.Tipo));

        RuleFor(item => item.Valor)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarAutorizacaoCommand, string>
                (item => item.Valor));

        RuleFor(item => item.Perfil)
            .NotNull()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarAutorizacaoCommand, bool>
                (item => item.Perfil));
    }
}