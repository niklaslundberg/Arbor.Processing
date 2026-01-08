using System;
using System.Threading;

namespace Arbor.Processing.Tests.OutputHelper;

internal static class Program
{
    private static void Main(string[] args)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        Print(cts.Token);
    }

    public static void Print(CancellationToken cancellationToken)
    {
        ulong counter = 1;
        while (!cancellationToken.IsCancellationRequested && counter <= 30000)
        {
            Console.WriteLine(counter);
            counter++;

            Console.WriteLine(new string('*', 1000));
        }
    }
}