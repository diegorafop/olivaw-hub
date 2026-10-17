using FluentValidation;
using MediatR;
using Mps.Devops.Services;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Devops.Api.Features.ImplantacaoFeature.Queries;

public sealed class GetDeploymentQuery : IRequest<GetDeploymentResponse>
{
    public Guid Id { get; set; }
}

public sealed class GetDeploymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}

public sealed class GetDeploymentQueryHandler(IDeploymentService deployments) : IRequestHandler<GetDeploymentQuery, GetDeploymentResponse>
{
    public async Task<GetDeploymentResponse> Handle(GetDeploymentQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var status = await deployments.GetAsync(request.Id, cancellationToken);
        return new GetDeploymentResponse { Id = status.Id, Status = status.Status, Log = status.Log };
    }
}

public sealed class GetDeploymentQueryValidator : AbstractValidator<GetDeploymentQuery>
{
    public GetDeploymentQueryValidator()
    {
        RuleFor(item => item.Id).NotEmpty().WithMessage(MessageHelper.EmptyFor<GetDeploymentQuery, Guid>(item => item.Id));
    }
}
