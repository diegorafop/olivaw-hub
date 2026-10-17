using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Domain;
using Mps.Auth.Domain.Enum;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;
using System.Linq.Expressions;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AtualizarUsuarioCommand
    : EntityCommand,
      IRequest<AtualizarUsuarioCommandResponse>
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string? IntegracaoId { get; set; }
    public StatusIntegracaoEnum StatusIntegracaoEnum { get; set; }

    public AtualizarUsuarioCommand() : base() { }
    public AtualizarUsuarioCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AtualizarUsuarioCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AtualizarUsuarioCommandHandler
(
    IRepositoryUsuario repository,
    AtualizarUsuarioCommandValidator validator
) : IRequestHandler<AtualizarUsuarioCommand,
    AtualizarUsuarioCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly AtualizarUsuarioCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AtualizarUsuarioCommandResponse> Handle
    (
        AtualizarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        await ExistsAsync(request, cancellationToken);

        Usuario entity = await GetFirstAsync
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

        return entity.Map<AtualizarUsuarioCommandResponse>();
    }

    private async Task<Usuario> GetFirstAsync
    (
        AtualizarUsuarioCommand request,
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

    private async Task ExistsAsync
    (
        AtualizarUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        bool exists = await _repository.ExistsAsync
        (
            SearchPredicate(request),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            exists,
            cancellationToken
        );
    }

    private static Expression<Func<Usuario, bool>> SearchPredicate
    (
        AtualizarUsuarioCommand request
    )
    {
        return item => item
            .Nome
            .ToUpper()
            .Equals(request
                .Nome
                .ToUpper())
            && !item
                .IdExterno
                .Equals(request.Id);
    }

    private async Task UpdateAsync
    (
        AtualizarUsuarioCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        request.Update(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AtualizarUsuarioCommandValidator
    : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AtualizarUsuarioCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.Login)
            .NotEmpty()
            .WithMessage(MessageHelper
                .NullFor<AtualizarUsuarioCommand, string>
                (item => item.Login));

        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage(MessageHelper
                .NullFor<AtualizarUsuarioCommand, string>
                (item => item.Email));
    }

    public async Task ValidateDataAsync
    (
        AtualizarUsuarioCommand request,
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