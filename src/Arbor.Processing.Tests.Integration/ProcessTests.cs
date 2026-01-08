using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Arbor.Processing.Tests.Integration;

public class ProcessTests(ITestOutputHelper output)
{
    [Fact]
    public async Task InfiniteProcessShouldBeCleanedUpOnCancellation()
    {
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "system32",
            "ping.exe");

        string[] args = ["127.0.0.1", "-t"];

        ExitCode? exitCode = null;

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                args,
                standardOutLog: (m, c) =>
                {
                    if (!string.IsNullOrWhiteSpace(m))
                    {
                        output.WriteLine($"{c} [StandardOut]: {m}");
                    }
                },
                standardErrorAction: (m, c) => output.WriteLine(c + " [StandardErrorOut]: " + m),
                toolAction: (m, c) =>
                {
                    if (!string.IsNullOrWhiteSpace(m))
                    {
                        output.WriteLine($"{c} [Tool]: {m}");
                    }
                },
                verboseAction: (m, c) => output.WriteLine(c + " [Verbose]: " + m),
                debugAction: (m, c) => output.WriteLine(c + " [Debug]: " + m),
                noWindow: false,
                cancellationToken: cts.Token);
        });

        Assert.Null(exitCode);
    }

    [Fact]
    public async Task InfiniteProcessShouldBeCleanedUpOnCancellationWithoutLogging()
    {
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "system32",
            "ping.exe");

        string[] args = ["127.0.0.1", "-t"];


        ExitCode? exitCode = null;

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                args,
                noWindow: false,
                cancellationToken: cts.Token);
        });

        Assert.Null(exitCode);
    }

    [Fact]
    public async Task ProcessPassingEnvironmentVariables()
    {
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "system32",
            "ping.exe");

        const string parameterName = "Ping_IP";

        var environmentVariables = new Dictionary<string, string> {[parameterName] = "127.0.0.1"};

        string[] args = [$"%{parameterName}%"];

        ExitCode? exitCode;

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        {
            exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                args,
                standardOutLog: (message, _) => output.WriteLine(message),
                toolAction: (message, _) => output.WriteLine(message),
                environmentVariables: environmentVariables,
                cancellationToken: cts.Token);
        }

        Assert.NotNull(exitCode);
        Assert.Equal(0, exitCode.Value);
    }


    [Fact]
    public async Task ProcessShouldBeCleanedUpOnExitSuccessful()
    {
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "system32",
            "ping.exe");

        string[] args = ["127.0.0.1"];

        for (int i = 0; i < 10; i++)
        {
            ExitCode? exitCode;

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                exitCode = await ProcessRunner.ExecuteProcessAsync(
                    exePath,
                    args,
                    cancellationToken: cts.Token);
            }

            Assert.NotNull(exitCode);
            Assert.Equal(0, exitCode.Value);
        }
    }
}