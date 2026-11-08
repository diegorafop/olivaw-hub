using FluentValidation;
using MediatR;
using Owh.Devops.Services;
using Owh.OlivawCore.Api.Helpers;

namespace Owh.Devops.Api.Features.DiskCleanupFeature.Commands;

public sealed class CreateDiskCleanupCommand : IRequest<CreateDiskCleanupResponse>
{

}
public sealed class CreateDiskCleanupResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}
public sealed class CreateDiskCleanupHandler(IDiskCleanupService cleanups) : IRequestHandler<CreateDiskCleanupCommand, CreateDiskCleanupResponse>
{
    public async Task<CreateDiskCleanupResponse> Handle(CreateDiskCleanupCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        var result = await cleanups.CreateAsync(cancellationToken);
        return new CreateDiskCleanupResponse { Id = result.Id, Status = result.Status, Log = result.Log };
    }
}
public sealed class CreateDiskCleanupValidator : AbstractValidator<CreateDiskCleanupCommand>
{
    public CreateDiskCleanupValidator() {  }
}
