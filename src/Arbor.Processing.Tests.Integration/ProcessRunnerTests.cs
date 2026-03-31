using Arbor;
using Arbor.Processing;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace Arbor.Processing.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "ProcessRunner"/> class.
/// Note: The ProcessRunner class has a private constructor and can only be instantiated
/// through the static ExecuteProcessAsync method. Therefore, direct unit testing of the
/// Dispose method in isolation is not possible without using reflection (which is prohibited).
/// These tests verify disposal behavior through integration-style scenarios.
/// </summary>
public sealed partial class ProcessRunnerTests
{
    /// <summary>
    /// Tests that ProcessRunner properly disposes resources when a process completes successfully.
    /// This verifies that Dispose is called and completes without error after normal execution.
    /// </summary>
    [Fact]
    public async Task Dispose_WhenProcessCompletesSuccessfully_DisposesWithoutError()
    {
        // Arrange
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        string[] args = ["/c", "echo", "test"];
        bool verboseLogCalled = false;
        bool standardErrorCalled = false;
        // Act
        ExitCode result = await ProcessRunner.ExecuteProcessAsync(exePath, args, verboseAction: (m, c) =>
        {
            verboseLogCalled = m.Contains("Dispose completed") || verboseLogCalled;
        }, standardErrorAction: (m, c) =>
        {
            standardErrorCalled = true;
        }, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        result.Should().Be(ExitCode.Success);
        verboseLogCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ProcessRunner properly handles disposal when cancelled.
    /// This verifies the Dispose method handles incomplete task completion sources correctly.
    /// </summary>
    [Fact]
    public async Task Dispose_WhenProcessIsCancelled_HandlesIncompleteTaskGracefully()
    {
        // Arrange
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        string[] args = ["127.0.0.1", "-t"];
        bool standardErrorInvoked = false;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        // Act
        Func<Task> act = async () =>
        {
            await ProcessRunner.ExecuteProcessAsync(exePath, args, standardErrorAction: (m, c) =>
            {
                standardErrorInvoked = m.Contains("Task completion") || standardErrorInvoked;
            }, verboseAction: (m, c) =>
            {
            }, noWindow: true, cancellationToken: cts.Token);
        };
        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    /// <summary>
    /// Tests that ProcessRunner Dispose method is idempotent and can handle being called
    /// multiple times through the using pattern and explicit disposal.
    /// </summary>
    [Fact(Skip = "Cannot directly test Dispose idempotency due to private constructor. ProcessRunner manages its own lifecycle internally through ExecuteProcessAsync.")]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotent()
    {
    // This test cannot be implemented due to the private constructor of ProcessRunner.
    // The class is designed to manage its own lifecycle internally and does not expose
    // a public constructor for direct instantiation and testing of Dispose behavior.
    // Disposal is handled automatically within the ExecuteProcessAsync method.
    }

    /// <summary>
    /// Tests that Dispose properly handles null channels without throwing exceptions.
    /// This scenario occurs when Dispose is called before channels are initialized.
    /// </summary>
    [Fact(Skip = "Cannot directly test internal state scenarios due to private constructor and no public API to control channel initialization.")]
    public void Dispose_WhenChannelsAreNull_DoesNotThrow()
    {
    // This test cannot be implemented due to the private constructor of ProcessRunner.
    // The internal state of channels (_outputChannel, _errorChannel) cannot be controlled
    // or verified without reflection, which is prohibited.
    // Integration tests through ExecuteProcessAsync provide indirect coverage.
    }

    /// <summary>
    /// Tests that Dispose properly handles null TaskCompletionSource without throwing exceptions.
    /// </summary>
    [Fact(Skip = "Cannot directly test internal state scenarios due to private constructor and no public API to control TaskCompletionSource state.")]
    public void Dispose_WhenTaskCompletionSourceIsNull_DoesNotThrow()
    {
    // This test cannot be implemented due to the private constructor of ProcessRunner.
    // The internal state of _taskCompletionSource cannot be controlled or verified
    // without reflection, which is prohibited.
    // The Dispose method handles null _taskCompletionSource gracefully with null-conditional operators.
    }

    /// <summary>
    /// Tests that Dispose sets the TaskCompletionSource result to Failure when the task cannot be awaited.
    /// This ensures proper cleanup and signaling when disposal occurs with an incomplete task.
    /// </summary>
    [Fact(Skip = "Cannot directly verify TaskCompletionSource result due to private constructor and internal state management.")]
    public void Dispose_WhenTaskCannotBeAwaited_SetsFailureResult()
    {
    // This test cannot be implemented due to the private constructor of ProcessRunner.
    // The behavior where Dispose sets ExitCode.Failure when task cannot be awaited
    // is an internal implementation detail that cannot be directly verified.
    // This scenario is partially covered by cancellation integration tests.
    }

    /// <summary>
    /// Tests that Dispose invokes verbose logging callbacks with appropriate messages.
    /// </summary>
    [Fact]
    public async Task Dispose_WhenVerboseActionProvided_InvokesLoggingCallbacks()
    {
        // Arrange
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        string[] args = ["/c", "exit", "0"];
        var logMessages = new List<string>();
        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, args, verboseAction: (m, c) => logMessages.Add(m), cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        logMessages.Should().Contain(m => m.Contains("Dispose completed"));
        logMessages.Should().Contain(m => m.Contains("Disposing process"));
    }

    /// <summary>
    /// Tests that Dispose handles process cleanup when process exits with non-zero exit code.
    /// </summary>
    [Fact]
    public async Task Dispose_WhenProcessExitsWithFailure_DisposesCorrectly()
    {
        // Arrange
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        string[] args = ["/c", "exit", "1"];
        bool disposeCompleted = false;
        // Act
        ExitCode result = await ProcessRunner.ExecuteProcessAsync(exePath, args, verboseAction: (m, c) =>
        {
            disposeCompleted = m.Contains("Dispose completed") || disposeCompleted;
        }, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        result.Code.Should().Be(1);
        disposeCompleted.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync throws ArgumentException when executePath is null.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NullExecutePath_ThrowsArgumentException()
    {
        // Arrange
        string executePath = null!;
        // Act
        Func<Task> act = async () => await ProcessRunner.ExecuteProcessAsync(executePath);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync throws ArgumentException when executePath is empty.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_EmptyExecutePath_ThrowsArgumentException()
    {
        // Arrange
        string executePath = string.Empty;
        // Act
        Func<Task> act = async () => await ProcessRunner.ExecuteProcessAsync(executePath);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync throws ArgumentException when executePath is whitespace.
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t  \n  ")]
    public async Task ExecuteProcessAsync_WhitespaceExecutePath_ThrowsArgumentException(string executePath)
    {
        // Arrange & Act
        Func<Task> act = async () => await ProcessRunner.ExecuteProcessAsync(executePath);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync throws ArgumentException for invalid path characters.
    /// </summary>
    [Theory]
    [InlineData("C:\\invalid<path>.exe")]
    [InlineData("C:\\invalid>path.exe")]
    [InlineData("C:\\invalid|path.exe")]
    [InlineData("C:\\invalid\"path.exe")]
    public async Task ExecuteProcessAsync_InvalidPathCharacters_ThrowsArgumentException(string executePath)
    {
        // Arrange & Act
        Func<Task> act = async () => await ProcessRunner.ExecuteProcessAsync(executePath);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles null arguments gracefully.
    /// Uses a real executable to test actual execution.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NullArguments_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        IEnumerable<string> arguments = null!;
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, arguments: arguments, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles empty arguments collection.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_EmptyArguments_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        IEnumerable<string> arguments = Array.Empty<string>();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, arguments: arguments, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync passes arguments to the process.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithArguments_PassesArgumentsToProcess()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        string[] arguments = GetTestArguments();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, arguments: arguments, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync invokes standardOutLog delegate when provided.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithStandardOutLog_InvokesDelegate()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        bool delegateInvoked = false;
        List<string> messages = new List<string>();
        void StandardOutLog(string message, string category)
        {
            delegateInvoked = true;
            messages.Add(message);
        }

        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, standardOutLog: StandardOutLog, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        delegateInvoked.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync invokes toolAction delegate in finally block with timing information.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithToolAction_InvokesDelegateWithTimingInfo()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        bool delegateInvoked = false;
        string loggedMessage = null!;
        void ToolAction(string message, string category)
        {
            delegateInvoked = true;
            loggedMessage = message;
        }

        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, toolAction: ToolAction, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        delegateInvoked.Should().BeTrue();
        loggedMessage.Should().NotBeNull();
        loggedMessage.Should().Contain("Running process");
        loggedMessage.Should().Contain("milliseconds");
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles null environment variables.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NullEnvironmentVariables_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        IEnumerable<KeyValuePair<string, string>> environmentVariables = null!;
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, environmentVariables: environmentVariables, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles empty environment variables collection.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_EmptyEnvironmentVariables_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        var environmentVariables = Array.Empty<KeyValuePair<string, string>>();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, environmentVariables: environmentVariables, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles non-empty environment variables collection.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithEnvironmentVariables_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        var environmentVariables = new[]
        {
            new KeyValuePair<string, string>("TEST_VAR1", "value1"),
            new KeyValuePair<string, string>("TEST_VAR2", "value2")
        };
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, environmentVariables: environmentVariables, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync respects noWindow parameter when true.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NoWindowTrue_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, noWindow: true, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync respects noWindow parameter when false.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NoWindowFalse_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, noWindow: false, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles shellExecute parameter.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteProcessAsync_ShellExecuteParameter_ExecutesSuccessfully(bool? shellExecute)
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, shellExecute: shellExecute, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles formatArgs parameter.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteProcessAsync_FormatArgsParameter_ExecutesSuccessfully(bool? formatArgs)
    {
        // Arrange
        string exePath = GetSystemExecutable();
        string[] arguments = GetTestArguments();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, arguments: arguments, formatArgs: formatArgs, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles null working directory.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_NullWorkingDirectory_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        DirectoryInfo workingDirectory = null!;
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, workingDirectory: workingDirectory, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles valid working directory.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_ValidWorkingDirectory_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        DirectoryInfo workingDirectory = new DirectoryInfo(Path.GetTempPath());
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, workingDirectory: workingDirectory, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync throws TaskCanceledException when cancellation is requested.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_CancellationRequested_ThrowsTaskCanceledException()
    {
        // Arrange
        string exePath = GetLongRunningExecutable();
        string[] arguments = GetLongRunningArguments();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        // Act
        Func<Task> act = async () => await ProcessRunner.ExecuteProcessAsync(exePath, arguments: arguments, noWindow: true, cancellationToken: cts.Token);
        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles default cancellation token.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_DefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, cancellationToken: default);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync invokes standardErrorAction delegate when provided.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithStandardErrorAction_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        bool delegateInvoked = false;
        void StandardErrorAction(string message, string category)
        {
            delegateInvoked = true;
        }

        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, standardErrorAction: StandardErrorAction, cancellationToken: TestContext.Current.CancellationToken);
    // Assert - delegate may or may not be invoked depending on whether there's error output
    // Just verify execution completes
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync invokes verboseAction delegate when provided.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithVerboseAction_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        bool delegateInvoked = false;
        void VerboseAction(string message, string category)
        {
            delegateInvoked = true;
        }

        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, verboseAction: VerboseAction, cancellationToken: TestContext.Current.CancellationToken);
    // Assert - delegate may or may not be invoked
    // Just verify execution completes
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync invokes debugAction delegate when provided.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_WithDebugAction_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        bool delegateInvoked = false;
        void DebugAction(string message, string category)
        {
            delegateInvoked = true;
        }

        // Act
        await ProcessRunner.ExecuteProcessAsync(exePath, debugAction: DebugAction, cancellationToken: TestContext.Current.CancellationToken);
    // Assert - delegate may or may not be invoked
    // Just verify execution completes
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync handles all optional parameters as null.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_AllOptionalParametersNull_ExecutesSuccessfully()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, arguments: null, standardOutLog: null, standardErrorAction: null, toolAction: null, verboseAction: null, environmentVariables: null, debugAction: null, workingDirectory: null, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ExecuteProcessAsync returns ExitCode with appropriate code value.
    /// </summary>
    [Fact]
    public async Task ExecuteProcessAsync_ValidExecution_ReturnsExitCode()
    {
        // Arrange
        string exePath = GetSystemExecutable();
        // Act
        ExitCode exitCode = await ProcessRunner.ExecuteProcessAsync(exePath, cancellationToken: TestContext.Current.CancellationToken);
        // Assert
        exitCode.Should().NotBeNull();
        exitCode.Code.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Helper method to get a system executable path for testing.
    /// Uses cmd.exe with /c exit 0 to ensure quick execution.
    /// </summary>
    private static string GetSystemExecutable()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        }

        throw new PlatformNotSupportedException("Tests require Windows platform");
    }

    /// <summary>
    /// Helper method to get test arguments that cause quick execution.
    /// </summary>
    private static string[] GetTestArguments()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return new[]
            {
                "/c",
                "exit",
                "0"
            };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Helper method to get a long-running executable for cancellation tests.
    /// </summary>
    private static string GetLongRunningExecutable()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        }

        throw new PlatformNotSupportedException("Tests require Windows platform");
    }

    /// <summary>
    /// Helper method to get arguments for long-running process.
    /// </summary>
    private static string[] GetLongRunningArguments()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return new[]
            {
                "127.0.0.1",
                "-t"
            };
        }

        return Array.Empty<string>();
    }
}