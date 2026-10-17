using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AlterarPerfilSenhaCommand
    : EntityCommand,
      IRequest<AlterarPerfilSenhaCommandResponse>
{
    public Guid Id { get; set; }
    public string Senha { get; set; }

    public AlterarPerfilSenhaCommand() : base() { }
    public AlterarPerfilSenhaCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AlterarPerfilSenhaCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AlterarPerfilSenhaCommandHandler
(
    IRepositoryUsuario repository
) : IRequestHandler<AlterarPerfilSenhaCommand,
    AlterarPerfilSenhaCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<AlterarPerfilSenhaCommandResponse> Handle
    (
        AlterarPerfilSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );

        await UpdateAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity.Map<AlterarPerfilSenhaCommandResponse>();
    }

    private async Task UpdateAsync
    (
        AlterarPerfilSenhaCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        request.UpdatePerfilSenha(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AlterarPerfilSenhaCommandValidator
    : AbstractValidator<AlterarPerfilSenhaCommand>
{
    public AlterarPerfilSenhaCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarPerfilSenhaCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.Senha)
            .NotEmpty()
            .WithMessage(MessageHelper
                .NullFor<AlterarPerfilSenhaCommand, string>
                (item => item.Senha));
    }
}