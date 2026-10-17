using FluentValidation;
using MediatR;
using Mps.Auth.Api.Extensions.Entities;
using Mps.Auth.Api.Features.EntityFeature.Commands;
using Mps.Auth.Api.Helpers;
using Mps.Auth.Domain;
using Mps.Auth.Repositories;
using Mps.HolosCore.Api.Extensions.Entities;
using Mps.HolosCore.Api.Helpers;

namespace Mps.Auth.Api.Features.UsuarioFeature.Commands;

public class AlterarSenhaCommand
    : EntityCommand,
      IRequest<AlterarSenhaCommandResponse>
{
    public Guid Id { get; set; }
    public string SenhaAtual { get; set; }
    public string SenhaNova { get; set; }
    public string SenhaConfirmacao { get; set; }

    public AlterarSenhaCommand() : base() { }
    public AlterarSenhaCommand
    (
        Guid usuarioLogadoId
    ) : base(usuarioLogadoId) { }
}

public class AlterarSenhaCommandResponse
{
    public DateTime? DataAtualizacao { get; set; }
}

public class AlterarSenhaCommandHandler
(
    IRepositoryUsuario repository,
    AlterarSenhaCommandValidator validator
) : IRequestHandler<AlterarSenhaCommand,
    AlterarSenhaCommandResponse>
{
    private readonly IRepositoryUsuario _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    private readonly AlterarSenhaCommandValidator _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<AlterarSenhaCommandResponse> Handle
    (
        AlterarSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationHelper.ThrowIfNull(request);

        Usuario entity = await GetFirstAsync(request, cancellationToken);
        await UpdateAsync(request, entity, cancellationToken);

        return entity.Map<AlterarSenhaCommandResponse>();
    }

    private async Task<Usuario> GetFirstAsync
    (
        AlterarSenhaCommand request,
        CancellationToken cancellationToken
    )
    {
        var entity = await _repository.GetFirstAsync
        (
            item => item
                .IdExterno
                .Equals(request.Id),
            cancellationToken
        );

        await _validator.ValidateDataAsync
        (
            request,
            entity,
            cancellationToken
        );

        return entity;
    }

    private async Task UpdateAsync
    (
        AlterarSenhaCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        request.UpdateSenha(entity);
        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public class AlterarSenhaCommandValidator
    : AbstractValidator<AlterarSenhaCommand>
{
    public AlterarSenhaCommandValidator()
    {
        RuleFor(item => item.Id)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaCommand, Guid>
                (item => item.Id));

        RuleFor(item => item.SenhaAtual)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaCommand, string>
                (item => item.SenhaAtual));

        RuleFor(item => item.SenhaNova)
            .NotEmpty()
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaCommand, string>
                (item => item.SenhaNova));

        RuleFor(item => item.SenhaConfirmacao)
            .NotEmpty()
            .Equal(item => item.SenhaNova)
            .WithMessage(MessageHelper
                .EmptyFor<AlterarSenhaCommand, string>
                (item => item.SenhaConfirmacao));

    }

    public async Task ValidateDataAsync
    (
        AlterarSenhaCommand request,
        Usuario entity,
        CancellationToken cancellationToken
    )
    {
        RuleFor(item => item.SenhaAtual)
            .Must(item =>
            {
                return !string.IsNullOrEmpty(entity.Senha) &&
                CryptographyHelper.EnhancedVerify
                (
                    request.SenhaAtual,
                    entity.Senha
                );
            })
            .WithMessage(MessageHelper
                .AuthenticationFor(request, item => item.SenhaAtual));

        await this.ValidateAndThrowAsync
        (
            request,
            cancellationToken
        );
    }
}