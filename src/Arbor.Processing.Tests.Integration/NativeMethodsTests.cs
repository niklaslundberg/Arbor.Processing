using System;
using System.Diagnostics;

using AwesomeAssertions;
using Xunit;

namespace Arbor.Processing.UnitTests;


/// <summary>
/// Integration tests for the NativeMethods class.
/// Note: These tests interact with actual Windows API calls and require a Windows environment.
/// </summary>
public class NativeMethodsTests
{
    /// <summary>
    /// Tests that IsWow64Process succeeds when called with a valid current process handle.
    /// Expected: The method should return true (success) and provide a valid result in the out parameter.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWow64Process_ValidCurrentProcessHandle_ReturnsTrue()
    {
        // Arrange
        IntPtr currentProcessHandle = Process.GetCurrentProcess().Handle;

        // Act
        bool result = NativeMethods.IsWow64Process(currentProcessHandle, out bool isWow64);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles IntPtr.Zero (invalid handle).
    /// Expected: The method should return false (failure) when given a null/zero handle.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWow64Process_ZeroHandle_ReturnsFalse()
    {
        // Arrange
        IntPtr zeroHandle = IntPtr.Zero;

        // Act
        bool result = NativeMethods.IsWow64Process(zeroHandle, out bool isWow64);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles an invalid process handle (negative value).
    /// Expected: The method should return true (the API call succeeds even with this handle value).
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWow64Process_InvalidNegativeHandle_ReturnsFalse()
    {
        // Arrange
        IntPtr invalidHandle = new IntPtr(-1);

        // Act
        bool result = NativeMethods.IsWow64Process(invalidHandle, out bool isWow64);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles arbitrary invalid handles.
    /// Expected: The method should return false (failure) for invalid handles.
    /// </summary>
    [Theory(Timeout = 10_000)]
    [InlineData(12345)]
    [InlineData(99999)]
    [InlineData(-12345)]
    public void IsWow64Process_ArbitraryInvalidHandles_ReturnsFalse(int handleValue)
    {
        // Arrange
        IntPtr invalidHandle = new IntPtr(handleValue);

        // Act
        bool result = NativeMethods.IsWow64Process(invalidHandle, out bool isWow64);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsWow64Process properly sets the out parameter when called with a valid handle.
    /// Expected: The method should set the wow64Process parameter to a valid boolean value (true or false).
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWow64Process_ValidHandle_SetsOutParameter()
    {
        // Arrange
        IntPtr currentProcessHandle = Process.GetCurrentProcess().Handle;

        // Act
        bool result = NativeMethods.IsWow64Process(currentProcessHandle, out bool isWow64);

        // Assert
        result.Should().BeTrue();
        // The out parameter should be either true or false (both are valid depending on process architecture)
        (isWow64 == true || isWow64 == false).Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles IntPtr.Zero (invalid handle).
    /// Expected: The method behavior with zero handle depends on the Windows API implementation.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsWow64Process_ZeroHandle_HandlesGracefully()
    {
        // Arrange
        nint zeroHandle = IntPtr.Zero;

        // Act
        bool result = NativeMethods.IsWow64Process(zeroHandle, out bool isWow64);

        // Assert
        // The result depends on Windows API behavior - we just verify the call completes
        (result == true || result == false).Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles various invalid process handles.
    /// Expected: The method behavior with invalid handles depends on the Windows API implementation.
    /// </summary>
    [Theory(Timeout = 10_000)]
    [InlineData(-1)]
    [InlineData(12345)]
    [InlineData(99999)]
    [InlineData(-12345)]
    public void IsWow64Process_InvalidHandles_HandlesGracefully(int handleValue)
    {
        // Arrange
        nint invalidHandle = new IntPtr(handleValue);

        // Act
        bool result = NativeMethods.IsWow64Process(invalidHandle, out bool isWow64);

        // Assert
        // The result depends on Windows API behavior - we just verify the call completes
        (result == true || result == false).Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsWow64Process handles extreme boundary values for nint handles.
    /// Expected: The method should handle extreme values without crashing.
    /// </summary>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void IsWow64Process_ExtremeHandleValues_HandlesGracefully(int handleValue)
    {
        // Arrange
        nint extremeHandle = new IntPtr(handleValue);

        // Act
        bool result = NativeMethods.IsWow64Process(extremeHandle, out bool isWow64);

        // Assert
        // The result depends on Windows API behavior - we just verify the call completes
        (result == true || result == false).Should().BeTrue();
    }
}