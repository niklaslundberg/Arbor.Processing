using BenchmarkDotNet.Running;

namespace Arbor.Processing.Benchmarks
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
