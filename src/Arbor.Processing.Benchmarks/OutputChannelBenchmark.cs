using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Arbor.Processing;
using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace Arbor.Processing.Benchmarks;
[CPUUsageDiagnoser]
#pragma warning disable CA1515
public class OutputChannelBenchmark
#pragma warning restore CA1515
{
    private string _helperExe = string.Empty;
    [GlobalSetup]
    public void Setup()
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        while (!Directory.Exists(Path.Combine(dir, "Arbor.Processing.Tests.OutputHelper")))
        {
            string? parent = Path.GetDirectoryName(dir);
            if (parent is null || parent == dir)
            {
                throw new DirectoryNotFoundException("Could not locate Arbor.Processing.Tests.OutputHelper");
            }
            dir = parent;
        }

        _helperExe = Path.Combine(dir, "Arbor.Processing.Tests.OutputHelper", "bin", "release", "net10.0", "Arbor.Processing.Tests.OutputHelper.exe");
        if (!File.Exists(_helperExe))
        {
            throw new FileNotFoundException($"OutputHelper not found at: {_helperExe}");
        }
    }

    [Benchmark(Baseline = true)]
    public async Task StdoutChannelOnly() =>
        await ProcessRunner.ExecuteProcessAsync(_helperExe, standardOutLog: static (_, _) => { }).ConfigureAwait(false);

    [Benchmark]
    public async Task StdoutAndStderrChannels() =>
        await ProcessRunner.ExecuteProcessAsync(_helperExe,
            standardOutLog: static (_, _) => { },
            standardErrorAction: static (_, _) => { }).ConfigureAwait(false);
}