using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Arbor.Processing.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PublicApiBenchmarks
{
    private readonly string _executePath;
    private readonly IReadOnlyList<string> _executeArguments;

    public PublicApiBenchmarks()
    {
        if (OperatingSystem.IsWindows())
        {
            _executePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
            _executeArguments = ["/c", "exit", "0"];
        }
        else
        {
            _executePath = "/usr/bin/env";
            _executeArguments = ["true"];
        }
    }

    [Benchmark]
    public ExitCode ExitCodeSuccess() => ExitCode.Success;

    [Benchmark]
    public ExitCode ExitCodeFailure() => ExitCode.Failure;

    [Benchmark]
    public string ExitCodeToStringSuccess() => ExitCode.Success.ToString();

    [Benchmark]
    public Task<ExitCode> ExecuteProcessWithoutLogging() =>
        ProcessRunner.ExecuteProcessAsync(_executePath, _executeArguments, formatArgs: false);
}
