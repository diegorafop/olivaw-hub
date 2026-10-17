using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;
using Mps.Devops.Services;

namespace Mps.Devops.Api.Services;

public sealed class SshDeploymentService(IProcessExecutionService processes) : IDeploymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<DeploymentStatus> SubmitAsync(DeploymentRequest request, CancellationToken cancellationToken)
    {
        await new CriarImplantacaoCommandValidator().ValidateAndThrowAsync(new CriarImplantacaoCommand
        {
            Folder = request.Folder, Branch = request.Branch, Environment = request.Environment
        }, cancellationToken);
        return await SendAsync(new { action = "submit", request.Folder, request.Branch, request.Environment }, cancellationToken);
    }

    public Task<DeploymentStatus> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return SendAsync(new { action = "status", id }, cancellationToken);
    }

    private async Task<DeploymentStatus> SendAsync(object payload, CancellationToken cancellationToken)
    {
        var result = await processes.ExecuteAsync(JsonSerializer.Serialize(payload, JsonOptions), cancellationToken);
        if (!result.Success) throw Failure("Deployment", "Executor SSH indisponível. Confira conexão, chave e host conhecido.");
        try
        {
            using var document = JsonDocument.Parse(result.Output);
            var root = document.RootElement;
            if (!root.GetProperty("success").GetBoolean())
            {
                var code = root.GetProperty("error").GetString();
                if (code == "not_found") throw Failure("Id", "Implantação não encontrado.");
                if (code == "invalid_request") throw Failure("Deployment", "Solicitação rejeitada pelo executor. Confira folder, branch e environment autorizados.");
                throw Failure("Deployment", "Executor não conseguiu iniciar ou consultar a implantação.");
            }
            var status = root.GetProperty("deployment").Deserialize<DeploymentStatus>(JsonOptions);
            if (status is null) throw new JsonException();
            if (status.Id == Guid.Empty) throw new JsonException();
            if (status.Status is not ("queued" or "running" or "succeeded" or "failed")) throw new JsonException();
            if (status.Log is null) throw new JsonException();
            return status;
        }
        catch (JsonException) { throw Failure("Deployment", "Resposta inválida do executor SSH."); }
        catch (KeyNotFoundException) { throw Failure("Deployment", "Resposta inválida do executor SSH."); }
        catch (InvalidOperationException) { throw Failure("Deployment", "Resposta inválida do executor SSH."); }
    }

    private static ValidationException Failure(string field, string message) => new([new ValidationFailure(field, message)]);
}
