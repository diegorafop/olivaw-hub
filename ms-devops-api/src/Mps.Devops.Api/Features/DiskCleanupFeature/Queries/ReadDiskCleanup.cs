using FluentValidation;
using MediatR;
using Mps.Devops.Services;
using Mps.OlivawCore.Api.Helpers;

namespace Mps.Devops.Api.Features.DiskCleanupFeature.Queries;

public sealed class ReadDiskCleanupQuery : IRequest<ReadDiskCleanupResponse>
{
    public Guid Id { get; set; }
}
public sealed class ReadDiskCleanupResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}
public sealed class ReadDiskCleanupHandler(IDeploymentService deployments) : IRequestHandler<ReadDiskCleanupQuery, ReadDiskCleanupResponse>
{
    public async Task<ReadDiskCleanupResponse> Handle(ReadDiskCleanupQuery request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var result = await deployments.GetAsync(request.Id, cancellationToken);
        return new ReadDiskCleanupResponse { Id = result.Id, Status = result.Status, Log = result.Log };
    }
}
public sealed class ReadDiskCleanupValidator : AbstractValidator<ReadDiskCleanupQuery>
{
    public ReadDiskCleanupValidator() { RuleFor(item => item.Id).NotEmpty(); }
}
