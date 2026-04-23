using System;
using Xunit;

namespace Arbor.Processing.Tests.Integration;

public class ExitCodeTests
{
    [Fact]
    public void SuccessShouldHaveZeroCode()
    {
        ExitCode exitCode = ExitCode.Success;

        Assert.True(exitCode.IsSuccess);
        Assert.Equal(0, exitCode.Code);
        Assert.Equal("EXIT CODE [0] Success", exitCode.ToString());
    }

    [Fact]
    public void FailureShouldHaveOneCode()
    {
        ExitCode exitCode = ExitCode.Failure;

        Assert.False(exitCode.IsSuccess);
        Assert.Equal(1, exitCode.Code);
        Assert.Equal("EXIT CODE [1] Failure", exitCode.ToString());
    }

    [Fact]
    public void FailedZeroShouldThrow()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => ExitCode.Failed(0));

        Assert.Equal("exitCode", exception.ParamName);
    }
}
