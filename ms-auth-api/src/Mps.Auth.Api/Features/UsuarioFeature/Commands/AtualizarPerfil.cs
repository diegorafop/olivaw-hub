using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AtualizarPerfilCommand
    : EntityCommand,
      IRequest<AtualizarPerfilCommandResponse>
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }

    public AtualizarPerfilCommand() : base() { }
    public AtualizarPerfilCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarPerfilCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarPerfilCommandHandler
(
    IRepositoryUsuario repository
) : IRequestHandler<AtualizarPerfilCommand,
    AtualizarPerfilCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<AtualizarPerfilCommandResponse> Handle
    (
        AtualizarPerfilCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);
        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<AtualizarPerfilCommandResponse>();
    }

    private async Task<Usuario> GetFirstAsync
    (
        AtualizarPerfilCommand request,
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

    private async Task UpdateAsync
    (
        AtualizarPerfilCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        request.UpdatePerfil(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarPerfilCommandValidator
    : AbstractValidator<AtualizarPerfilCommand>
{
    public AtualizarPerfilCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarPerfilCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.Login)
            .NotEmpty()
            .WithMessage(MessageHelper
                .NullFor<AtualizarPerfilCommand, string>
                (item => item.Login));

        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage(MessageHelper
                .NullFor<AtualizarPerfilCommand, string>
                (item => item.Email));
    }
}