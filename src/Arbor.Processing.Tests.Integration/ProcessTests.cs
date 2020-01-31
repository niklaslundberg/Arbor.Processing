using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.Processing.Tests.Integration
{
    public class ProcessTests
    {
        public ProcessTests(ITestOutputHelper output) => _output = output;

        private readonly ITestOutputHelper _output;

        [Fact]
        public async Task InfiniteProcessShouldBeCleanedUpOnCancellation()
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string[] args = {"127.0.0.1", "-t"};

            ExitCode? exitCode = null;

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                    args,
                    cancellationToken: cts.Token,
                    toolAction: (m, c) =>
                    {
                        if (!string.IsNullOrWhiteSpace(m))
                        {
                            _output.WriteLine(c + " [Tool]: " + m);
                        }
                    },
                    standardErrorAction: (m, c) => _output.WriteLine(c + " [StandardErrorOut]: " + m),
                    standardOutLog: (m, c) =>
                    {
                        if (!string.IsNullOrWhiteSpace(m))
                        {
                            _output.WriteLine(c + " [StandardOut]: " + m);
                        }
                    },
                    verboseAction: (m, c) => _output.WriteLine(c + " [Verbose]: " + m),
                    debugAction: (m, c) => _output.WriteLine(c + " [Debug]: " + m),
                    noWindow: false).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Assert.Null(exitCode);
        }

        [Fact]
        public async Task InfiniteProcessShouldBeCleanedUpOnCancellationWithoutLogging()
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string[] args = {"127.0.0.1", "-t"};


            ExitCode? exitCode = null;

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                    args,
                    cancellationToken: cts.Token,
                    noWindow: false).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Assert.Null(exitCode);
        }

        [Fact]
        public async Task ProcessPassingEnvironmentVariables()
        {
            string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string parameterName = "Ping_IP";

            var environmentVariables = new Dictionary<string, string> {[parameterName] = "127.0.0.1"};

            string[] args = {$"%{parameterName}%"};

            ExitCode? exitCode;

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                    args,
                    cancellationToken: cts.Token,
                    environmentVariables: environmentVariables,
                    standardOutLog: (message, category) => _output.WriteLine(message),
                    toolAction: (message, category) => _output.WriteLine(message)).ConfigureAwait(false);
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

            string[] args = {"127.0.0.1"};

            for (int i = 0; i < 10; i++)
            {
                ExitCode? exitCode;

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    exitCode = await ProcessRunner
                        .ExecuteProcessAsync(
                            exePath,
                            args,
                            cancellationToken: cts.Token)
                        .ConfigureAwait(false);
                }

                Assert.NotNull(exitCode);
                Assert.Equal(0, exitCode.Value);
            }
        }
    }
}