using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Domain;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using Mps.HolosCore.Repositories;

namespace Mps.Auth.Api.Features.AutenticacaoFeature.Commands;

public class DeslogarUsuarioCommand
    : IRequest<DeslogarUsuarioCommandResponse>
{
    public string Token { get; set; }
    internal Guid UsuarioLogadoId { get; set; }

    public DeslogarUsuarioCommand() { }
    public DeslogarUsuarioCommand
    (
        Guid usuarioLogadoId
    )
    {
        UsuarioLogadoId = usuarioLogadoId;
    }
}

public class DeslogarUsuarioCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? DataRemocao { get; set; }
}

public class DeslogarUsuarioCommandHandler
(
    IRepository<UsuarioAutenticacao> repository
) : IRequestHandler<DeslogarUsuarioCommand,
    DeslogarUsuarioCommandResponse>
{
    private readonly IRepository<UsuarioAutenticacao> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<DeslogarUsuarioCommandResponse> Handle
    (
        DeslogarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        UsuarioAutenticacao entity = await GetFirstAsync
        (
            request,
            cancellationToken
        );

        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<DeslogarUsuarioCommandResponse>();
    }

    private async Task<UsuarioAutenticacao> GetFirstAsync
    (
        DeslogarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetFirstAsync
        (
            request.SearchPredicate(),
            cancellationToken
        );
    }

    private async Task UpdateAsync
    (
        DeslogarUsuarioCommand request,
        UsuarioAutenticacao entity,
        CancellationToken cancellationToken
    )
    {
        entity.DataRemocao = DateTime.UtcNow;
        entity.Update(request.UsuarioLogadoId);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class DeslogarUsuarioCommandValidator
    : AbstractValidator<DeslogarUsuarioCommand>
{
    public DeslogarUsuarioCommandValidator()
    {
        RuleFor(item => item.Token)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<DeslogarUsuarioCommand, string>
                (item => item.Token));
    }
}