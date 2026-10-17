using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using Mps.Devops.Api.Settings;
using Mps.Devops.Services;

namespace Mps.Devops.Api.Services;

public sealed class ProcessExecutionService(IOptions<DeploymentSettings> settings) : IProcessExecutionService
{
    public async Task<ProcessExecutionResult> ExecuteAsync(string payload, CancellationToken cancellationToken)
    {
        var start = CreateStartInfo(settings.Value);
        using var process = new Process { StartInfo = start };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(settings.Value.TimeoutSeconds));
        try
        {
            process.Start();
            var output = ReadOutputAsync(process.StandardOutput, timeout.Token);
            var error = DrainAsync(process.StandardError, timeout.Token);
            try
            {
                await process.StandardInput.WriteLineAsync(payload.AsMemory(), timeout.Token);
                process.StandardInput.Close();
                await process.WaitForExitAsync(timeout.Token);
                await error;
                return new ProcessExecutionResult { Success = process.ExitCode == 0, Output = await output };
            }
            finally
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { }
                await process.WaitForExitAsync(CancellationToken.None);
                try { await Task.WhenAll(output, error); }
                catch (OperationCanceledException) { }
                catch (IOException) { }
            }
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ProcessExecutionResult();
        }
        catch (IOException) { return new ProcessExecutionResult(); }
        catch (Win32Exception) { return new ProcessExecutionResult(); }
        catch (UnauthorizedAccessException) { return new ProcessExecutionResult(); }
    }

    public static ProcessStartInfo CreateStartInfo(DeploymentSettings settings)
    {
        if (!Regex.IsMatch(settings.Host, @"\A[a-zA-Z0-9][a-zA-Z0-9.-]*\z")) throw InvalidSettings();
        if (!Regex.IsMatch(settings.User, @"\A[a-zA-Z_][a-zA-Z0-9_-]*\z")) throw InvalidSettings();
        if (settings.Port < 1) throw InvalidSettings();
        if (settings.Port > 65535) throw InvalidSettings();
        if (settings.TimeoutSeconds < 1) throw InvalidSettings();
        if (settings.TimeoutSeconds > 120) throw InvalidSettings();
        if (!SafePath(settings.KeyFile)) throw InvalidSettings();
        if (!SafePath(settings.KnownHostsFile)) throw InvalidSettings();
        var start = new ProcessStartInfo("ssh")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var argument in new[]
        {
            "-F", "/dev/null", "-T", "-o", "BatchMode=yes", "-o", "IdentitiesOnly=yes",
            "-o", "StrictHostKeyChecking=yes", "-o", $"UserKnownHostsFile={settings.KnownHostsFile}",
            "-o", "GlobalKnownHostsFile=/dev/null", "-o", "ConnectTimeout=10",
            "-o", "ClearAllForwardings=yes", "-o", "IdentityAgent=none", "-o", "ServerAliveInterval=5",
            "-o", "ServerAliveCountMax=2", "-i", settings.KeyFile, "-p", settings.Port.ToString(),
            "-l", settings.User, settings.Host, "olivaw-deploy"
        }) start.ArgumentList.Add(argument);
        return start;
    }

    private static bool SafePath(string path) => Regex.IsMatch(path, @"\A/[a-zA-Z0-9_./-]+\z");

    private static ValidationException InvalidSettings() => new([new ValidationFailure("Deployment", "Configure a conexão SSH de implantação.")]);

    private static async Task<string> ReadOutputAsync(StreamReader reader, CancellationToken token)
    {
        var text = new StringBuilder();
        var buffer = new char[2048];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), token)) != 0)
        {
            if (text.Length + count > 32768) throw new IOException("Resposta do executor excedeu o limite.");
            text.Append(buffer, 0, count);
        }
        return text.ToString();
    }

    private static async Task DrainAsync(StreamReader reader, CancellationToken token)
    {
        var buffer = new char[2048];
        while (await reader.ReadAsync(buffer.AsMemory(), token) != 0) { }
    }
}
