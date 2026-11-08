using FluentValidation;
using MediatR;
using Owh.Devops.Services;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Devops.Api.Features.DeploymentFeature.Queries;

public sealed class ReadDeploymentQuery : IRequest<ReadDeploymentResponse>
{
    public Guid Id { get; set; }
}

public sealed class ReadDeploymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}

public sealed class ReadDeploymentQueryHandler(IDeploymentService deployments, IDeploymentAccessService access) : IRequestHandler<ReadDeploymentQuery, ReadDeploymentResponse>
{
    public async Task<ReadDeploymentResponse> Handle(ReadDeploymentQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var status = await deployments.GetAsync(request.Id, cancellationToken);
        access.EnsureCanDeploy(status.Folder);
        return new ReadDeploymentResponse { Id = status.Id, Status = status.Status, Log = status.Log };
    }
}

public sealed class ReadDeploymentQueryValidator : AbstractValidator<ReadDeploymentQuery>
{
    public ReadDeploymentQueryValidator()
    {
        RuleFor(item => item.Id).NotEmpty().WithMessage(MessageHelper.EmptyFor<ReadDeploymentQuery, Guid>(item => item.Id));
    }
}
