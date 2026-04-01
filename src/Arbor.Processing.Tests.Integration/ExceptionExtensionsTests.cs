using System;
using System.Runtime.InteropServices;
using System.Threading;

using Arbor.Processing;
using AwesomeAssertions;
using Xunit;

namespace Arbor.Processing.UnitTests;

/// <summary>
/// Unit tests for the <see cref = "ExceptionExtensions"/> class.
/// </summary>
public class ExceptionExtensionsTests
{
    /// <summary>
    /// Tests that IsFatal returns true when the exception is one of the fatal exception types.
    /// </summary>
    /// <param name = "exception">The fatal exception to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(typeof(OutOfMemoryException))]
    [InlineData(typeof(AccessViolationException))]
    [InlineData(typeof(AppDomainUnloadedException))]
    [InlineData(typeof(StackOverflowException))]
    [InlineData(typeof(ThreadAbortException))]
    [InlineData(typeof(SEHException))]
    public void IsFatal_FatalExceptionTypes_ReturnsTrue(Type exceptionType)
    {
        Exception exception = (Exception)Activator.CreateInstance(exceptionType);
        bool result = exception.IsFatal();
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsFatal returns false when the exception is null.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsFatal_NullException_ReturnsFalse()
    {
        Exception exception = null;
        bool result = exception.IsFatal();
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsFatal returns false when the exception is not one of the fatal exception types.
    /// </summary>
    /// <param name = "exceptionType">The non-fatal exception type to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(typeof(Exception))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(ArgumentNullException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NullReferenceException))]
    [InlineData(typeof(NotSupportedException))]
    [InlineData(typeof(NotImplementedException))]
    [InlineData(typeof(IndexOutOfRangeException))]
    public void IsFatal_NonFatalExceptionTypes_ReturnsFalse(Type exceptionType)
    {
        Exception exception = (Exception)Activator.CreateInstance(exceptionType);
        bool result = exception.IsFatal();
        result.Should().BeFalse();
    }
}