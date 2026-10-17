using System.ComponentModel;
using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Options;
using Mps.Devops.Api.Features.ImplantacaoFeature.Commands;
using Mps.Devops.Api.Settings;
using Mps.Devops.Services;

namespace Mps.Devops.Api.Services;

public sealed class ProcessExecutionService(IOptions<DeploymentSettings> settings) : IProcessExecutionService
{
    private readonly DeploymentSettings _settings = settings.Value;

    public async Task<ProcessExecutionResult> ExecuteAsync(string folder, string branch, string environment, DeploymentStep step, CancellationToken cancellationToken)
    {
        await new CriarImplantacaoCommandValidator().ValidateAndThrowAsync(
            new CriarImplantacaoCommand { Folder = folder, Branch = branch, Environment = environment }, cancellationToken);
        try
        {
            var directory = ResolveDirectory(folder);
            var start = CreateStartInfo(directory, branch, environment, step);
            using var process = new Process { StartInfo = start };
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));
            process.Start();
            var output = DrainAsync(process.StandardOutput, timeout.Token);
            var error = DrainAsync(process.StandardError, timeout.Token);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                await Task.WhenAll(output, error);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
                try { await Task.WhenAll(output, error); }
                catch (OperationCanceledException) { }
                cancellationToken.ThrowIfCancellationRequested();
                return Failure();
            }
            return new ProcessExecutionResult
            {
                Success = process.ExitCode == 0,
                Log = $"{step}: código de saída {process.ExitCode}."
            };
        }
        catch (IOException) { return Failure(); }
        catch (UnauthorizedAccessException) { return Failure(); }
        catch (Win32Exception) { return Failure(); }
    }

    public string ResolveDirectory(string folder)
    {
        if (!CriarImplantacaoCommandValidator.IsSafeFolder(folder)) throw InvalidFolder();
        if (string.IsNullOrWhiteSpace(_settings.RootDirectory)) throw InvalidFolder();
        if (!Path.IsPathFullyQualified(_settings.RootDirectory)) throw InvalidFolder();
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(_settings.RootDirectory));
        var current = Path.GetPathRoot(root) ?? throw InvalidFolder();
        foreach (var part in root[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, part);
            RejectLink(current);
        }
        foreach (var part in folder.Split('/'))
        {
            current = Path.Combine(current, part);
            RejectLink(current);
        }
        if (!Directory.Exists(current)) throw InvalidFolder();
        var repository = current;
        while (true)
        {
            var git = Path.Combine(repository, ".git");
            if (Directory.Exists(git))
            {
                RejectLink(git);
                return current;
            }
            if (repository == root) throw InvalidFolder();
            repository = Path.GetDirectoryName(repository) ?? throw InvalidFolder();
        }
    }

    public static ProcessStartInfo CreateStartInfo(string directory, string branch, string environment, DeploymentStep step)
    {
        if (!CriarImplantacaoCommandValidator.IsSafeBranch(branch)) throw InvalidFolder();
        if (!CriarImplantacaoCommandValidator.IsSafeEnvironment(environment))
            throw new ValidationException([new FluentValidation.Results.ValidationFailure("Environment", "Environment inválido.")]);
        var start = new ProcessStartInfo
        {
            WorkingDirectory = directory, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        };
        string[] arguments;
        switch (step)
        {
            case DeploymentStep.Checkout:
                start.FileName = "git";
                arguments = ["checkout", branch];
                break;
            case DeploymentStep.Pull:
                start.FileName = "git";
                arguments = ["pull", "--ff-only"];
                break;
            case DeploymentStep.Compose:
                start.FileName = "docker";
                arguments = ["compose", "--env-file", $"docker/.env.{environment}",
                    "-f", "docker/docker-compose.yml", "-f", $"docker/docker-compose.{environment}.yml", "up", "-d"];
                break;
            default: throw new ArgumentOutOfRangeException(nameof(step));
        }
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["GIT_TERMINAL_PROMPT"] = "0";
        start.Environment["GCM_INTERACTIVE"] = "Never";
        return start;
    }

    private static async Task DrainAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var buffer = new char[4096];
        while (await reader.ReadAsync(buffer.AsMemory(), cancellationToken) != 0) { }
    }

    private static void RejectLink(string path)
    {
        if (new DirectoryInfo(path).LinkTarget is not null) throw InvalidFolder();
    }

    private static ValidationException InvalidFolder()
    {
        return new ValidationException([new FluentValidation.Results.ValidationFailure("Folder",
            "Folder indisponível ou fora da raiz de implantação permitida.")]);
    }

    private static ProcessExecutionResult Failure() => new() { Success = false };
}
