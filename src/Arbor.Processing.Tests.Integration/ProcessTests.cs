using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.Processing.Tests.Integration
{
    public class ProcessTests
    {
        private readonly ITestOutputHelper _output;

        public ProcessTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task InfiniteProcessShouldBeCleanedUpOnCancellation()
        {
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string[] args = { "127.0.0.1", "-t" };


            ExitCode? exitCode = null;

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
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
                        noWindow: false);
                }
            });

            Assert.Null(exitCode);
        }

        [Fact]
        public async Task InfiniteProcessShouldBeCleanedUpOnCancellationWithoutLogging()
        {
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string[] args = { "127.0.0.1", "-t" };


            ExitCode? exitCode = null;

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                        args,
                        cancellationToken: cts.Token,
                        noWindow: false);
                }
            });

            Assert.Null(exitCode);
        }


        [Fact]
        public async Task ProcessShouldBeCleanedUpOnExitSuccessful()
        {
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "system32",
                "ping.exe");

            string[] args = { "127.0.0.1" };

            for (int i = 0; i < 10; i++)
            {
                ExitCode? exitCode;

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    exitCode = await ProcessRunner.ExecuteProcessAsync(exePath,
                        args,
                        cancellationToken: cts.Token);
                }

                Assert.NotNull(exitCode);
                Assert.Equal(0, exitCode.Value);
            }
        }
    }
}
