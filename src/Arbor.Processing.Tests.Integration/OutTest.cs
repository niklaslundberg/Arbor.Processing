using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.Processing.Tests.Integration
{
    public class OutTest(ITestOutputHelper testOutputHelper)
    {
        [Fact]
        public async Task Output()
        {
            string helperExe = Path.Combine(
                VcsTestPathHelper.FindVcsRootPath(),
                "src",
                "Arbor.Processing.Tests.OutputHelper",
                "bin",
                "debug",
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

            await ProcessRunner.ExecuteProcessAsync(helperExe, standardOutLog: Log);

            Assert.Equal(30000, list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    Assert.True(list[i] > list[i - 1]);
                    Assert.True(list[i] - list[i - 1] == 1);
                }
            }
        }
    }
}