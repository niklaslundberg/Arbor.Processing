using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace Arbor.Processing.Tests.Integration;

public class OutTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Output()
    {
#if DEBUG
        const string configuration = "debug";
#else
            const string configuration = "release";
#endif

        string helperExe = Path.Combine(
            VcsTestPathHelper.FindVcsRootPath(),
            "src",
            "Arbor.Processing.Tests.OutputHelper",
            "bin",
            configuration,
            "net10.0",
            "Arbor.Processing.Tests.OutputHelper.exe");

        Assert.True(File.Exists(helperExe));

        var list = new List<ulong>();

        void Log(string message, string _)
        {
            if (ulong.TryParse(message, out ulong result))
            {
                list.Add(result);
            }
            else if (message?.Trim().Length != 1000)
            {
                testOutputHelper.WriteLine($"Unexpected line length {message?.Length}");
            }
        }

        await ProcessRunner.ExecuteProcessAsync(helperExe, standardOutLog: Log, cancellationToken: TestContext.Current.CancellationToken);

        list.Should().HaveCount(30_000);

        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0)
            {
                (list[i] > list[i - 1]).Should().BeTrue();
                (list[i] - list[i - 1] == 1).Should().BeTrue();
            }
        }
    }
}