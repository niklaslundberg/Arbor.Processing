using Arbor.Processing;
using AwesomeAssertions;
using System;
using System.Diagnostics;
using Xunit;

namespace Arbor.Processing.UnitTests;
/// <summary>
/// Unit tests for the ProcessExtensions class.
/// </summary>
public partial class ProcessExtensionsTests
{
    /// <summary>
    /// Tests that IsWin64 returns a valid boolean value when called on the current process.
    /// Verifies that the method executes without throwing exceptions and returns one of the expected values (null, true, or false).
    /// Expected result: Returns null on non-Windows platforms, or true/false on Windows platforms.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWin64_WithCurrentProcess_ReturnsValidBooleanValue()
    {
        // Arrange
        Process currentProcess = Process.GetCurrentProcess();
        // Act
        bool? result = currentProcess.IsWin64();
        // Assert
        // Result should be null (non-Windows) or a boolean value (Windows)
        // We cannot assert a specific value since it depends on the platform and process architecture
        _ = result; // Verify no exception was thrown
    }

    /// <summary>
    /// Tests that IsWin64 returns null when running on a non-Windows platform.
    /// This test will be skipped on Windows platforms.
    /// Expected result: Returns null on non-Windows platforms.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWin64_OnNonWindowsPlatform_ReturnsNull()
    {
        // Arrange
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Skip this test on Windows
            return;
        }

        Process currentProcess = Process.GetCurrentProcess();
        // Act
        bool? result = currentProcess.IsWin64();
        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that IsWin64 returns a non-null boolean value when running on a Windows platform.
    /// This test will be skipped on non-Windows platforms.
    /// Expected result: Returns true or false on Windows platforms.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWin64_OnWindowsPlatform_ReturnsNonNullBoolean()
    {
        // Arrange
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Skip this test on non-Windows platforms
            return;
        }

        Process currentProcess = Process.GetCurrentProcess();
        // Act
        bool? result = currentProcess.IsWin64();
        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that IsWin64 handles a process that has already exited gracefully.
    /// The method should catch non-fatal exceptions and return false.
    /// Expected result: Returns false when the process handle cannot be retrieved due to process termination.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWin64_WithExitedProcess_HandleGracefully()
    {
        // Arrange
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Skip this test on non-Windows platforms
            return;
        }

        // Start a process and let it exit immediately
        Process process = Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/c exit", CreateNoWindow = true, UseShellExecute = false });
        // Wait for the process to exit
        process.WaitForExit();
        // Act
        bool? result = process.IsWin64();
        // Assert
        // The result should be false because accessing the handle of an exited process
        // should throw a non-fatal exception which is caught and returns false
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsWin64 works correctly with multiple process instances.
    /// Verifies consistent behavior across different process objects referring to the same process.
    /// Expected result: Returns consistent results for the same process.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWin64_WithMultipleProcessInstances_ReturnsConsistentResults()
    {
        // Arrange
        Process currentProcess1 = Process.GetCurrentProcess();
        Process currentProcess2 = Process.GetProcessById(currentProcess1.Id);
        // Act
        bool? result1 = currentProcess1.IsWin64();
        bool? result2 = currentProcess2.IsWin64();
        // Assert
        result1.Should().Be(result2);
    }
}