using FluentValidation;
using MediatR;
using Owh.Devops.Services;
using Owh.OlivawCore.Api.Helpers;
using System.Text.RegularExpressions;

namespace Owh.Devops.Api.Features.DeploymentFeature.Commands;

public sealed class CreateDeploymentCommand : IRequest<CreateDeploymentResponse>
{
    public string Folder { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

public sealed class CreateDeploymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}

public sealed class CreateDeploymentCommandHandler(IDeploymentService deployments, IDeploymentAccessService access)
    : IRequestHandler<CreateDeploymentCommand, CreateDeploymentResponse>
{
    public async Task<CreateDeploymentResponse> Handle(CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        ValidationHelper.ThrowIfNull(request);
        access.EnsureCanDeploy(request.Folder);
        var status = await deployments.SubmitAsync(new DeploymentRequest
        {
            Folder = request.Folder, Branch = request.Branch, Environment = request.Environment
        }, cancellationToken);
        return new CreateDeploymentResponse { Id = status.Id, Status = status.Status, Log = status.Log };
    }
}

public sealed class CreateDeploymentCommandValidator : AbstractValidator<CreateDeploymentCommand>
{
    public CreateDeploymentCommandValidator()
    {
        RuleFor(item => item.Folder).Cascade(CascadeMode.Stop).NotEmpty()
            .WithMessage(MessageHelper.EmptyFor<CreateDeploymentCommand, string>(item => item.Folder))
            .Must(IsSafeFolder).WithMessage("Folder deve ser um caminho relativo válido, sem caracteres especiais ou segmentos de navegação.");
        RuleFor(item => item.Branch).Cascade(CascadeMode.Stop).NotEmpty()
            .WithMessage(MessageHelper.EmptyFor<CreateDeploymentCommand, string>(item => item.Branch))
            .Must(IsSafeBranch).WithMessage("Branch deve ser um nome Git válido, sem opções ou caracteres especiais.");
        RuleFor(item => item.Environment).Cascade(CascadeMode.Stop).NotEmpty()
            .WithMessage(MessageHelper.EmptyFor<CreateDeploymentCommand, string>(item => item.Environment))
            .Must(IsSafeEnvironment).WithMessage("Environment deve ser dev, qas, stg, rls ou prd.");
    }

    public static bool IsSafeEnvironment(string value)
    {
        return value is "dev" or "qas" or "stg" or "rls" or "prd";
    }

    public static bool IsSafeFolder(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length > 200) return false;
        return Regex.IsMatch(value, @"\A[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*\z");
    }

    public static bool IsSafeBranch(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length > 200) return false;
        if (!Regex.IsMatch(value, @"\A[a-zA-Z0-9][a-zA-Z0-9._/-]*\z")) return false;
        if (value.Contains("..")) return false;
        if (value.Contains("//")) return false;
        if (value == "HEAD") return false;
        foreach (var part in value.Split('/'))
        {
            if (part.Length == 0) return false;
            if (part.StartsWith('.')) return false;
            if (part.EndsWith('.')) return false;
            if (part.EndsWith(".lock", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return true;
    }
}
