using System;

using Arbor;
using Arbor.Processing;
using AwesomeAssertions;
using Xunit;

namespace Arbor.Processing.Tests.Integration;




public class ExitCodeTests
{
    [Fact(Timeout = 10_000)]
    public void ConstructorWithZeroCreatesSuccessExitCode()
    {
        var exitCode = new ExitCode(0);

        exitCode.Code.Should().Be(0);
        exitCode.IsSuccess.Should().BeTrue();
    }

    [Fact(Timeout = 10_000)]
    public void ConstructorWithPositiveNonZeroCreatesFailureExitCode()
    {
        var exitCode = new ExitCode(1);

        exitCode.Code.Should().Be(1);
        exitCode.IsSuccess.Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void ConstructorWithNegativeValueCreatesFailureExitCode()
    {
        var exitCode = new ExitCode(-1);

        exitCode.Code.Should().Be(-1);
        exitCode.IsSuccess.Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void SuccessHasCodeZero() =>
        ExitCode.Success.Code.Should().Be(0);

    [Fact(Timeout = 10_000)]
    public void SuccessIsSuccess() =>
        ExitCode.Success.IsSuccess.Should().BeTrue();

    [Fact(Timeout = 10_000)]
    public void SuccessReturnsSameValueOnEveryCall() =>
        (ExitCode.Success == ExitCode.Success).Should().BeTrue();

    [Fact(Timeout = 10_000)]
    public void FailureHasCodeOne() =>
        ExitCode.Failure.Code.Should().Be(1);

    [Fact(Timeout = 10_000)]
    public void FailureIsNotSuccess() =>
        ExitCode.Failure.IsSuccess.Should().BeFalse();

    [Fact(Timeout = 10_000)]
    public void FailureReturnsSameValueOnEveryCall() =>
        (ExitCode.Failure == ExitCode.Failure).Should().BeTrue();

    [Fact(Timeout = 10_000)]
    public void FailedWithNonZeroReturnsExitCodeWithThatValue()
    {
        var failed = ExitCode.Failed(42);

        failed.Code.Should().Be(42);
        failed.IsSuccess.Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void FailedWithNegativeReturnsExitCodeWithThatValue()
    {
        var failed = ExitCode.Failed(-5);

        failed.Code.Should().Be(-5);
        failed.IsSuccess.Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void FailedWithZeroThrowsArgumentOutOfRangeException()
    {
        Action act = () => ExitCode.Failed(0);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*Exit code cannot be 0 when failed*");
    }

    [Fact(Timeout = 10_000)]
    public void EqualsSameCodeReturnsTrue()
    {
        var a = new ExitCode(42);
        var b = new ExitCode(42);

        a.Equals(b).Should().BeTrue();
    }

    [Fact(Timeout = 10_000)]
    public void EqualsDifferentCodeReturnsFalse()
    {
        var a = new ExitCode(0);
        var b = new ExitCode(1);

        a.Equals(b).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void EqualsBoxedSameCodeReturnsTrue()
    {
        var exitCode = new ExitCode(42);
        object boxed = new ExitCode(42);

        exitCode.Equals(boxed).Should().BeTrue();
    }

    [Fact(Timeout = 10_000)]
    public void EqualsBoxedDifferentCodeReturnsFalse()
    {
        var exitCode = new ExitCode(42);
        object boxed = new ExitCode(99);

        exitCode.Equals(boxed).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void EqualsNullReturnsFalse()
    {
        var exitCode = new ExitCode(0);

        exitCode.Equals(null).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void EqualsDifferentTypeReturnsFalse()
    {
        var exitCode = new ExitCode(0);

        exitCode.Equals("not an ExitCode").Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void EqualityOperatorSameCodeReturnsTrue()
    {
        var a = new ExitCode(7);
        var b = new ExitCode(7);

        (a == b).Should().BeTrue();
    }

    [Fact(Timeout = 10_000)]
    public void EqualityOperatorDifferentCodeReturnsFalse()
    {
        var a = new ExitCode(0);
        var b = new ExitCode(1);

        (a == b).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void InequalityOperatorDifferentCodeReturnsTrue()
    {
        var a = new ExitCode(0);
        var b = new ExitCode(1);

        (a != b).Should().BeTrue();
    }

    [Fact(Timeout = 10_000)]
    public void InequalityOperatorSameCodeReturnsFalse()
    {
        var a = new ExitCode(42);
        var b = new ExitCode(42);

        (a != b).Should().BeFalse();
    }

    [Fact(Timeout = 10_000)]
    public void GetHashCodeSameCodeReturnsSameValue() =>
        new ExitCode(42).GetHashCode().Should().Be(new ExitCode(42).GetHashCode());

    [Fact(Timeout = 10_000)]
    public void GetHashCodeEqualsCodeValue() =>
        new ExitCode(7).GetHashCode().Should().Be(7);

    [Fact(Timeout = 10_000)]
    public void ImplicitConversionToIntReturnsCode()
    {
        var exitCode = new ExitCode(42);

        int value = exitCode;

        value.Should().Be(42);
    }

    [Fact(Timeout = 10_000)]
    public void ToInt32ReturnsCode() =>
        new ExitCode(42).ToInt32().Should().Be(42);

    [Fact(Timeout = 10_000)]
    public void ToStringZeroReturnsSuccessMessage() =>
        new ExitCode(0).ToString().Should().Be("EXIT CODE [0] Success");

    [Fact(Timeout = 10_000)]
    public void ToStringOneReturnsFailureMessage() =>
        new ExitCode(1).ToString().Should().Be("EXIT CODE [1] Failure");

    [Fact(Timeout = 10_000)]
    public void ToStringNegativeReturnsFailureMessage() =>
        new ExitCode(-1).ToString().Should().Be("EXIT CODE [-1] Failure");

    [Fact(Timeout = 10_000)]
    public void ToStringArbitraryNonZeroReturnsFailureMessage() =>
        new ExitCode(42).ToString().Should().Be("EXIT CODE [42] Failure");

    /// <summary>
    /// Tests that Equals returns true when both ExitCode instances have the minimum integer value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_BothCodesAreMinValue_ReturnsTrue()
    {
        var a = new ExitCode(int.MinValue);
        var b = new ExitCode(int.MinValue);

        a.Equals(b).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns true when both ExitCode instances have the maximum integer value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_BothCodesAreMaxValue_ReturnsTrue()
    {
        var a = new ExitCode(int.MaxValue);
        var b = new ExitCode(int.MaxValue);

        a.Equals(b).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns true when both ExitCode instances have zero value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_BothCodesAreZero_ReturnsTrue()
    {
        var a = new ExitCode(0);
        var b = new ExitCode(0);

        a.Equals(b).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns true when both ExitCode instances have the same negative value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_BothCodesAreSameNegative_ReturnsTrue()
    {
        var a = new ExitCode(-42);
        var b = new ExitCode(-42);

        a.Equals(b).Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing minimum and maximum integer values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_MinValueAndMaxValue_ReturnsFalse()
    {
        var a = new ExitCode(int.MinValue);
        var b = new ExitCode(int.MaxValue);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing a negative value with a positive value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_NegativeAndPositive_ReturnsFalse()
    {
        var a = new ExitCode(-1);
        var b = new ExitCode(1);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing different negative values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_DifferentNegativeCodes_ReturnsFalse()
    {
        var a = new ExitCode(-100);
        var b = new ExitCode(-200);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing zero with a non-zero value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ZeroAndNonZero_ReturnsFalse()
    {
        var a = new ExitCode(0);
        var b = new ExitCode(100);

        a.Equals(b).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that GetHashCode returns the correct value for boundary and edge case integer values.
    /// Tests int.MinValue, int.MaxValue, zero, negative, and positive values.
    /// </summary>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    public void GetHashCode_WithEdgeCaseValues_ReturnsCodeValue(int code) =>
        new ExitCode(code).GetHashCode().Should().Be(code);

    /// <summary>
    /// Verifies that GetHashCode is consistent for extreme integer boundary values.
    /// Two ExitCode instances with the same extreme code should return the same hash code.
    /// </summary>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void GetHashCode_WithBoundaryValues_ReturnsConsistentHashCode(int code) =>
        new ExitCode(code).GetHashCode().Should().Be(new ExitCode(code).GetHashCode());

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is null.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_NullObject_ReturnsFalse()
    {
        var exitCode = new ExitCode(42);

        bool result = exitCode.Equals((object)null);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns true when the parameter is a boxed ExitCode with the same code.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Equals_BoxedExitCodeWithSameCode_ReturnsTrue(int code)
    {
        var exitCode = new ExitCode(code);
        object boxed = new ExitCode(code);

        bool result = exitCode.Equals(boxed);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is a boxed ExitCode with a different code.
    /// </summary>
    /// <param name="code1">The first exit code value.</param>
    /// <param name="code2">The second exit code value.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(42, 43)]
    [InlineData(-1, 1)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(int.MinValue, int.MaxValue)]
    [InlineData(0, int.MaxValue)]
    [InlineData(0, int.MinValue)]
    public void Equals_BoxedExitCodeWithDifferentCode_ReturnsFalse(int code1, int code2)
    {
        var exitCode = new ExitCode(code1);
        object boxed = new ExitCode(code2);

        bool result = exitCode.Equals(boxed);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is a different type.
    /// </summary>
    /// <param name="obj">The object of a different type to compare.</param>
    [Theory(Timeout = 10_000)]
    [InlineData("string")]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(true)]
    [InlineData(3.14)]
    public void Equals_DifferentType_ReturnsFalse(object obj)
    {
        var exitCode = new ExitCode(42);

        bool result = exitCode.Equals(obj);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is an empty object.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_EmptyObject_ReturnsFalse()
    {
        var exitCode = new ExitCode(0);
        object emptyObject = new object();

        bool result = exitCode.Equals(emptyObject);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ToInt32 returns the exact code value for boundary values and edge cases.
    /// Validates behavior with int.MinValue, int.MaxValue, zero, and various positive and negative values.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(-2147483647)]
    [InlineData(2147483646)]
    [InlineData(-100)]
    [InlineData(100)]
    public void ToInt32_VariousCodeValues_ReturnsExactCodeValue(int code)
    {
        // Arrange
        var exitCode = new ExitCode(code);

        // Act
        int result = exitCode.ToInt32();

        // Assert
        result.Should().Be(code);
    }

    /// <summary>
    /// Tests that Failed method with zero exit code throws ArgumentOutOfRangeException
    /// with the expected error message.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Action act = () => ExitCode.Failed(0);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*Exit code cannot be 0 when failed*");
    }

    /// <summary>
    /// Tests that Failed method with a positive non-zero value returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithPositiveNonZero_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(42);

        // Assert
        failed.Code.Should().Be(42);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failed method with a negative value returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithNegative_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(-5);

        // Assert
        failed.Code.Should().Be(-5);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failed method with int.MaxValue returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithIntMaxValue_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(int.MaxValue);

        // Assert
        failed.Code.Should().Be(int.MaxValue);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failed method with int.MinValue returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithIntMinValue_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(int.MinValue);

        // Assert
        failed.Code.Should().Be(int.MinValue);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failed method with value 1 returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithOne_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(1);

        // Assert
        failed.Code.Should().Be(1);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failed method with value -1 returns an ExitCode
    /// with that code value and IsSuccess set to false.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithNegativeOne_ReturnsExitCodeWithThatValue()
    {
        // Act
        var failed = ExitCode.Failed(-1);

        // Assert
        failed.Code.Should().Be(-1);
        failed.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Success property equals a newly constructed ExitCode with code 0,
    /// verifying value-based equality.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Success_EqualsNewExitCodeWithZero_ReturnsTrue() =>
        ExitCode.Success.Equals(new ExitCode(0)).Should().BeTrue();

    /// <summary>
    /// Tests that the Success property does not equal the Failure property,
    /// verifying that Success and Failure are distinct values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Success_NotEqualsFailure_ReturnsTrue() =>
        (ExitCode.Success != ExitCode.Failure).Should().BeTrue();

    /// <summary>
    /// Tests that the Success property's GetHashCode returns 0,
    /// matching the Code value of the Success exit code.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Success_GetHashCode_ReturnsZero() =>
        ExitCode.Success.GetHashCode().Should().Be(0);

    /// <summary>
    /// Tests that the Success property can be implicitly converted to int
    /// and equals 0, verifying the implicit conversion operator.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Success_ImplicitConversionToInt_ReturnsZero()
    {
        // Arrange
        ExitCode success = ExitCode.Success;

        // Act
        int result = success;

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that calling ToInt32 on the Success property returns 0,
    /// verifying the explicit conversion method.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Success_ToInt32_ReturnsZero() =>
        ExitCode.Success.ToInt32().Should().Be(0);

    /// <summary>
    /// Tests that ToString returns "EXIT CODE [0] Success" when the exit code is 0.
    /// Verifies the success case formatting.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithCodeZero_ReturnsSuccessMessage() =>
        new ExitCode(0).ToString().Should().Be("EXIT CODE [0] Success");

    /// <summary>
    /// Tests that ToString returns "EXIT CODE [1] Failure" when the exit code is 1.
    /// Verifies the failure case formatting with a positive non-zero value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithCodeOne_ReturnsFailureMessage() =>
        new ExitCode(1).ToString().Should().Be("EXIT CODE [1] Failure");

    /// <summary>
    /// Tests that ToString returns "EXIT CODE [-1] Failure" when the exit code is -1.
    /// Verifies the failure case formatting with a negative value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithCodeNegativeOne_ReturnsFailureMessage() =>
        new ExitCode(-1).ToString().Should().Be("EXIT CODE [-1] Failure");

    /// <summary>
    /// Tests that ToString returns the correct failure message when the exit code is int.MaxValue.
    /// Verifies boundary value handling for maximum integer value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithCodeMaxValue_ReturnsFailureMessage() =>
        new ExitCode(int.MaxValue).ToString().Should().Be("EXIT CODE [2147483647] Failure");

    /// <summary>
    /// Tests that ToString returns the correct failure message when the exit code is int.MinValue.
    /// Verifies boundary value handling for minimum integer value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithCodeMinValue_ReturnsFailureMessage() =>
        new ExitCode(int.MinValue).ToString().Should().Be("EXIT CODE [-2147483648] Failure");

    /// <summary>
    /// Tests that ToString returns the correct failure message for an arbitrary non-zero positive value.
    /// Verifies that any positive non-zero code is treated as a failure.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithArbitraryPositiveCode_ReturnsFailureMessage() =>
        new ExitCode(42).ToString().Should().Be("EXIT CODE [42] Failure");

    /// <summary>
    /// Tests that ToString returns the correct failure message for an arbitrary negative value.
    /// Verifies that any negative code is treated as a failure.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_WithArbitraryNegativeCode_ReturnsFailureMessage() =>
        new ExitCode(-999).ToString().Should().Be("EXIT CODE [-999] Failure");

    /// <summary>
    /// Verifies that the Failure property returns an ExitCode that equals a manually created ExitCode with code 1.
    /// This ensures the Failure property returns a logically equivalent value.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_EqualsExitCodeWithCodeOne_ReturnsTrue()
    {
        // Arrange
        var manualFailure = new ExitCode(1);

        // Act
        var result = ExitCode.Failure;

        // Assert
        result.Should().Be(manualFailure);
    }

    /// <summary>
    /// Verifies that the Failure property returns an ExitCode that is not equal to Success.
    /// This ensures proper inequality between failure and success states.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_ComparedToSuccess_AreNotEqual()
    {
        // Act
        var failure = ExitCode.Failure;
        var success = ExitCode.Success;

        // Assert
        (failure != success).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the Failure property's GetHashCode returns 1, which should equal the code value.
    /// This ensures hash code consistency for the Failure singleton.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_GetHashCode_ReturnsOne()
    {
        // Act
        var hashCode = ExitCode.Failure.GetHashCode();

        // Assert
        hashCode.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the Failure property can be implicitly converted to int and returns 1.
    /// This ensures the implicit conversion operator works correctly for the Failure singleton.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_ImplicitConversionToInt_ReturnsOne()
    {
        // Act
        int code = ExitCode.Failure;

        // Assert
        code.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the Failure property's ToInt32 method returns 1.
    /// This ensures explicit integer conversion works correctly for the Failure singleton.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_ToInt32_ReturnsOne()
    {
        // Act
        var code = ExitCode.Failure.ToInt32();

        // Assert
        code.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the Failure property's ToString returns the expected failure message format.
    /// This ensures string representation is correct for the Failure singleton.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_ToString_ReturnsFailureMessage()
    {
        // Act
        var result = ExitCode.Failure.ToString();

        // Assert
        result.Should().Be("EXIT CODE [1] Failure");
    }

    /// <summary>
    /// Tests that calling ToInt32 on the Success property returns 0,
    /// verifying the explicit conversion method.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToInt32_Success_ReturnsZero()
    {
        // Arrange
        var success = ExitCode.Success;

        // Act
        int result = success.ToInt32();

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that calling ToInt32 on the Failure property returns 1,
    /// verifying the explicit conversion method.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToInt32_Failure_ReturnsOne()
    {
        // Arrange
        var failure = ExitCode.Failure;

        // Act
        int result = failure.ToInt32();

        // Assert
        result.Should().Be(1);
    }

    /// <summary>
    /// Tests that Failed method throws ArgumentOutOfRangeException when exitCode is zero.
    /// Zero is not a valid failure code, so the method should reject it with an appropriate exception message.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_WithZeroExitCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => ExitCode.Failed(0);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*Exit code cannot be 0 when failed*")
            .WithParameterName("exitCode");
    }

    /// <summary>
    /// Tests that Failed method returns an ExitCode with the provided non-zero code value.
    /// Validates that the returned ExitCode has the correct Code property and IsSuccess is false.
    /// Tests various edge cases including int.MinValue, int.MaxValue, -1, 1, and arbitrary values.
    /// </summary>
    /// <param name="exitCode">The non-zero exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(-100)]
    [InlineData(42)]
    [InlineData(-2147483647)]
    [InlineData(2147483646)]
    [InlineData(255)]
    [InlineData(-255)]
    public void Failed_WithNonZeroExitCode_ReturnsExitCodeWithCorrectCodeAndIsNotSuccess(int exitCode)
    {
        // Arrange & Act
        ExitCode result = ExitCode.Failed(exitCode);

        // Assert
        result.Code.Should().Be(exitCode);
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsSuccess returns true when the exit code is zero.
    /// Zero is the only value that represents a successful exit code.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void IsSuccess_WithCodeZero_ReturnsTrue()
    {
        // Arrange
        var exitCode = new ExitCode(0);

        // Act
        bool result = exitCode.IsSuccess;

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsSuccess returns false when the exit code is non-zero.
    /// Validates behavior with various edge cases including int.MinValue, int.MaxValue,
    /// positive values, negative values, and other boundary conditions.
    /// </summary>
    /// <param name="code">The non-zero exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(2147483646)]
    [InlineData(-2147483647)]
    public void IsSuccess_WithNonZeroCode_ReturnsFalse(int code)
    {
        // Arrange
        var exitCode = new ExitCode(code);

        // Act
        bool result = exitCode.IsSuccess;

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ToString returns the correct message format for various exit code values.
    /// Validates that the message is "Success" only for code 0, and "Failure" for all other codes.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    /// <param name="expectedMessage">The expected message portion (Success or Failure).</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0, "Success")]
    [InlineData(1, "Failure")]
    [InlineData(-1, "Failure")]
    [InlineData(42, "Failure")]
    [InlineData(-42, "Failure")]
    [InlineData(100, "Failure")]
    [InlineData(-100, "Failure")]
    [InlineData(2147483647, "Failure")]
    [InlineData(-2147483648, "Failure")]
    public void ToString_WithVariousCodeValues_ReturnsCorrectFormat(int code, string expectedMessage)
    {
        // Arrange
        var exitCode = new ExitCode(code);
        var expected = $"EXIT CODE [{code}] {expectedMessage}";

        // Act
        var result = exitCode.ToString();

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetHashCode returns the Code value for various edge cases including
    /// int.MinValue, int.MaxValue, zero, negative, and positive values.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(-2147483647)]
    [InlineData(2147483646)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(100)]
    [InlineData(-100)]
    public void GetHashCode_WithVariousCodeValues_ReturnsCodeValue(int code) =>
        new ExitCode(code).GetHashCode().Should().Be(code);

    /// <summary>
    /// Verifies that GetHashCode returns consistent hash codes for two ExitCode instances
    /// with the same code value, testing various edge case values.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(-42)]
    public void GetHashCode_SameCodeValue_ReturnsSameHashCode(int code)
    {
        // Arrange
        ExitCode exitCode1 = new ExitCode(code);
        ExitCode exitCode2 = new ExitCode(code);

        // Act
        int hash1 = exitCode1.GetHashCode();
        int hash2 = exitCode2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    /// <summary>
    /// Verifies that GetHashCode returns different hash codes for ExitCode instances
    /// with different code values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void GetHashCode_DifferentCodeValues_ReturnsDifferentHashCodes()
    {
        // Arrange
        ExitCode exitCode1 = new ExitCode(0);
        ExitCode exitCode2 = new ExitCode(1);

        // Act
        int hash1 = exitCode1.GetHashCode();
        int hash2 = exitCode2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Verifies that GetHashCode returns zero when the code value is zero.
    /// This is important for the Success exit code scenario.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void GetHashCode_WithCodeZero_ReturnsZero() =>
        new ExitCode(0).GetHashCode().Should().Be(0);

    /// <summary>
    /// Verifies that GetHashCode returns int.MinValue when the code is int.MinValue.
    /// This tests the lower boundary value handling.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void GetHashCode_WithMinValue_ReturnsMinValue() =>
        new ExitCode(int.MinValue).GetHashCode().Should().Be(int.MinValue);

    /// <summary>
    /// Verifies that GetHashCode returns int.MaxValue when the code is int.MaxValue.
    /// This tests the upper boundary value handling.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void GetHashCode_WithMaxValue_ReturnsMaxValue() =>
        new ExitCode(int.MaxValue).GetHashCode().Should().Be(int.MaxValue);

    /// <summary>
    /// Tests that the Failed method throws ArgumentOutOfRangeException when exitCode is zero.
    /// Zero is not a valid failure code, so the method should reject it with the expected exception message.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failed_ZeroExitCode_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const int exitCode = 0;

        // Act
        Action act = () => ExitCode.Failed(exitCode);

        // Assert
        act.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithMessage("*Exit code cannot be 0 when failed*")
            .WithParameterName("exitCode");
    }

    /// <summary>
    /// Tests that the Failed method returns an ExitCode with the correct code value and IsSuccess set to false
    /// for all non-zero exit code values. Tests boundary values (int.MinValue, int.MaxValue), common values,
    /// and arbitrary positive and negative values.
    /// </summary>
    /// <param name="exitCode">The non-zero exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(-2147483647)]
    [InlineData(2147483646)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(-100)]
    [InlineData(100)]
    [InlineData(-255)]
    [InlineData(255)]
    [InlineData(42)]
    [InlineData(-42)]
    public void Failed_NonZeroExitCode_ReturnsExitCodeWithCorrectCodeAndIsNotSuccess(int exitCode)
    {
        // Arrange & Act
        ExitCode result = ExitCode.Failed(exitCode);

        // Assert
        result.Code.Should().Be(exitCode);
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failure property uses the equality operator correctly with a manually created ExitCode(1).
    /// Verifies that the equality operator returns true when comparing Failure to an equivalent ExitCode.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_EqualityOperatorWithCodeOne_ReturnsTrue()
    {
        // Arrange
        var manualFailure = new ExitCode(1);

        // Act
        var result = ExitCode.Failure == manualFailure;

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Failure property uses the inequality operator correctly with ExitCode instances having different codes.
    /// Verifies that the inequality operator returns true when comparing Failure to ExitCode with different values.
    /// </summary>
    /// <param name="code">The exit code value to compare against Failure.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Failure_InequalityOperatorWithDifferentCode_ReturnsTrue(int code)
    {
        // Arrange
        var otherExitCode = new ExitCode(code);

        // Act
        var result = ExitCode.Failure != otherExitCode;

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Failure property's Equals method returns true when compared to a boxed ExitCode with code 1.
    /// Verifies proper equality checking with boxed value types.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_EqualsBoxedExitCodeWithCodeOne_ReturnsTrue()
    {
        // Arrange
        object boxedExitCode = new ExitCode(1);

        // Act
        var result = ExitCode.Failure.Equals(boxedExitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Failure property's Equals method returns false when compared to boxed ExitCode instances with different codes.
    /// Verifies proper inequality checking with boxed value types.
    /// </summary>
    /// <param name="code">The exit code value to box and compare against Failure.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Failure_EqualsBoxedExitCodeWithDifferentCode_ReturnsFalse(int code)
    {
        // Arrange
        object boxedExitCode = new ExitCode(code);

        // Act
        var result = ExitCode.Failure.Equals(boxedExitCode);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failure property's Equals method returns false when compared to null.
    /// Verifies proper null handling in equality comparisons.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_EqualsNull_ReturnsFalse()
    {
        // Act
        var result = ExitCode.Failure.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Failure property's Equals method returns false when compared to objects of different types.
    /// Verifies proper type checking in equality comparisons.
    /// </summary>
    /// <param name="obj">The object of a different type to compare.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1)]
    [InlineData("1")]
    [InlineData(true)]
    public void Failure_EqualsDifferentType_ReturnsFalse(object obj)
    {
        // Act
        var result = ExitCode.Failure.Equals(obj);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Failure property does not equal ExitCode instances with various different code values.
    /// Verifies that Failure only equals ExitCode instances with code 1.
    /// </summary>
    /// <param name="code">The exit code value to compare against Failure.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Failure_NotEqualsExitCodeWithDifferentCode_ReturnsTrue(int code)
    {
        // Arrange
        var otherExitCode = new ExitCode(code);

        // Act
        var areEqual = ExitCode.Failure.Equals(otherExitCode);

        // Assert
        areEqual.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Failure property Code value is exactly 1, not just non-zero.
    /// Verifies the specific code value for the Failure singleton.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_Code_IsExactlyOne()
    {
        // Act
        var code = ExitCode.Failure.Code;

        // Assert
        code.Should().Be(1);
    }

    /// <summary>
    /// Tests that accessing the Failure property multiple times from different call sites
    /// returns structurally equal values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Failure_MultipleAccessesFromDifferentCallSites_ReturnEqualValues()
    {
        // Act
        var first = GetFailure();
        var second = GetFailure();

        // Assert
        first.Should().Be(second);

        static ExitCode GetFailure() => ExitCode.Failure;
    }

    /// <summary>
    /// Tests that ToString returns "EXIT CODE [0] Success" when the code is 0.
    /// This is the only case where the message should indicate success.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void ToString_CodeZero_ReturnsSuccessMessage() =>
        new ExitCode(0).ToString().Should().Be("EXIT CODE [0] Success");

    /// <summary>
    /// Tests that ToString returns the correct failure message format for various non-zero exit codes.
    /// All non-zero codes (positive or negative) should result in a "Failure" message.
    /// Validates boundary values (int.MinValue, int.MaxValue) and typical failure codes.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(255)]
    [InlineData(-255)]
    public void ToString_NonZeroCode_ReturnsFailureMessage(int code)
    {
        // Arrange
        var exitCode = new ExitCode(code);
        var expected = $"EXIT CODE [{code}] Failure";

        // Act
        var result = exitCode.ToString();

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that GetHashCode returns the exact Code value for all integer boundary cases.
    /// Tests int.MinValue, int.MaxValue, zero, and typical positive/negative values.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(2147483646)]
    [InlineData(-2147483647)]
    public void GetHashCode_VariousCodes_ReturnsCodeValue(int code) =>
        new ExitCode(code).GetHashCode().Should().Be(code);

    /// <summary>
    /// Verifies that GetHashCode returns consistent values when called multiple times on the same instance.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(42)]
    public void GetHashCode_CalledMultipleTimes_ReturnsSameValue(int code)
    {
        // Arrange
        var exitCode = new ExitCode(code);

        // Act
        var hash1 = exitCode.GetHashCode();
        var hash2 = exitCode.GetHashCode();
        var hash3 = exitCode.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
        hash1.Should().Be(code);
    }

    /// <summary>
    /// Verifies that two ExitCode instances with the same Code value return the same hash code.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(42)]
    [InlineData(-42)]
    public void GetHashCode_TwoInstancesWithSameCode_ReturnSameHashCode(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        var exitCode2 = new ExitCode(code);

        // Act & Assert
        exitCode1.GetHashCode().Should().Be(exitCode2.GetHashCode());
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is null.
    /// Validates that null checking is properly handled in the Equals method.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ObjectOverload_WithNull_ReturnsFalse()
    {
        // Arrange
        var exitCode = new ExitCode(42);

        // Act
        bool result = exitCode.Equals((object)null);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns true when the parameter is a boxed ExitCode with the same code.
    /// Validates boundary and edge case values including int.MinValue, int.MaxValue, zero, and various positive/negative values.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(2147483646)]
    [InlineData(-2147483647)]
    public void Equals_ObjectOverload_WithBoxedSameCode_ReturnsTrue(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        object exitCode2 = new ExitCode(code);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is a boxed ExitCode with a different code.
    /// Validates various combinations of different code values including boundary values.
    /// </summary>
    /// <param name="code1">The first exit code value.</param>
    /// <param name="code2">The second exit code value.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(0, -1)]
    [InlineData(42, 43)]
    [InlineData(-1, 1)]
    [InlineData(100, -100)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(int.MinValue, int.MaxValue)]
    [InlineData(0, int.MaxValue)]
    [InlineData(0, int.MinValue)]
    [InlineData(int.MaxValue, 0)]
    [InlineData(int.MinValue, 0)]
    public void Equals_ObjectOverload_WithBoxedDifferentCode_ReturnsFalse(int code1, int code2)
    {
        // Arrange
        var exitCode1 = new ExitCode(code1);
        object exitCode2 = new ExitCode(code2);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when the parameter is an object of a different type.
    /// Validates behavior with various types including string, int, bool, double, and DateTime.
    /// </summary>
    /// <param name="obj">The object of a different type to compare.</param>
    [Theory(Timeout = 10_000)]
    [InlineData("string")]
    [InlineData("0")]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(3.14)]
    [InlineData(0.0)]
    [InlineData(-42.5)]
    public void Equals_ObjectOverload_WithDifferentType_ReturnsFalse(object obj)
    {
        // Arrange
        var exitCode = new ExitCode(0);

        // Act
        bool result = exitCode.Equals(obj);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when comparing with edge case values for the ExitCode.
    /// Ensures that boundary values like int.MinValue and int.MaxValue work correctly with null and different types.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ObjectOverload_WithMinValueAndNull_ReturnsFalse()
    {
        // Arrange
        var exitCode = new ExitCode(int.MinValue);

        // Act
        bool result = exitCode.Equals((object)null);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when comparing with edge case values for the ExitCode.
    /// Ensures that boundary values like int.MaxValue work correctly with null.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ObjectOverload_WithMaxValueAndNull_ReturnsFalse()
    {
        // Arrange
        var exitCode = new ExitCode(int.MaxValue);

        // Act
        bool result = exitCode.Equals((object)null);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when comparing the Success exit code with different types.
    /// Validates that predefined instances like Success properly reject non-ExitCode objects.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ObjectOverload_SuccessWithString_ReturnsFalse()
    {
        // Arrange
        var success = ExitCode.Success;

        // Act
        bool result = success.Equals((object)"0");

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals(object) returns false when comparing the Failure exit code with different types.
    /// Validates that predefined instances like Failure properly reject non-ExitCode objects.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ObjectOverload_FailureWithInteger_ReturnsFalse()
    {
        // Arrange
        var failure = ExitCode.Failure;

        // Act
        bool result = failure.Equals((object)1);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals satisfies the reflexive property: an ExitCode instance equals itself.
    /// Validates with various code values including boundary cases.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(100)]
    [InlineData(-100)]
    [InlineData(2147483646)]
    [InlineData(-2147483647)]
    public void Equals_ReflexiveProperty_InstanceEqualsItself(int code)
    {
        // Arrange
        var exitCode = new ExitCode(code);

        // Act
        bool result = exitCode.Equals(exitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals satisfies the symmetric property: if x.Equals(y) is true, then y.Equals(x) is also true.
    /// Validates with various code values including boundary cases.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(100)]
    [InlineData(-100)]
    public void Equals_SymmetricProperty_BothDirectionsReturnTrue(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        var exitCode2 = new ExitCode(code);

        // Act
        bool result1 = exitCode1.Equals(exitCode2);
        bool result2 = exitCode2.Equals(exitCode1);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals satisfies the transitive property: if x.Equals(y) and y.Equals(z), then x.Equals(z).
    /// Validates with various code values including boundary cases.
    /// </summary>
    /// <param name="code">The exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Equals_TransitiveProperty_ThreeInstancesWithSameCodeAllEqual(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        var exitCode2 = new ExitCode(code);
        var exitCode3 = new ExitCode(code);

        // Act
        bool result1And2 = exitCode1.Equals(exitCode2);
        bool result2And3 = exitCode2.Equals(exitCode3);
        bool result1And3 = exitCode1.Equals(exitCode3);

        // Assert
        result1And2.Should().BeTrue();
        result2And3.Should().BeTrue();
        result1And3.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals is consistent: multiple calls with the same values return the same result.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_ConsistencyProperty_MultipleCallsReturnSameResult()
    {
        // Arrange
        var exitCode1 = new ExitCode(42);
        var exitCode2 = new ExitCode(42);

        // Act
        bool result1 = exitCode1.Equals(exitCode2);
        bool result2 = exitCode1.Equals(exitCode2);
        bool result3 = exitCode1.Equals(exitCode2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals with default ExitCode (which has Code = 0) returns true when compared with another default or zero-code ExitCode.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_DefaultExitCodeWithZeroCode_ReturnsTrue()
    {
        // Arrange
        var defaultExitCode = default(ExitCode);
        var zeroExitCode = new ExitCode(0);

        // Act
        bool result = defaultExitCode.Equals(zeroExitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals with default ExitCode returns true when compared with itself.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_DefaultExitCodeWithItself_ReturnsTrue()
    {
        // Arrange
        var defaultExitCode = default(ExitCode);

        // Act
        bool result = defaultExitCode.Equals(defaultExitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals with default ExitCode returns false when compared with non-zero ExitCode.
    /// </summary>
    /// <param name="code">The non-zero exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Equals_DefaultExitCodeWithNonZero_ReturnsFalse(int code)
    {
        // Arrange
        var defaultExitCode = default(ExitCode);
        var nonZeroExitCode = new ExitCode(code);

        // Act
        bool result = defaultExitCode.Equals(nonZeroExitCode);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns true when comparing Success property with a newly created ExitCode with code 0.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_SuccessWithZeroCode_ReturnsTrue()
    {
        // Arrange
        var success = ExitCode.Success;
        var zeroExitCode = new ExitCode(0);

        // Act
        bool result = success.Equals(zeroExitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing Success property with Failure property.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_SuccessWithFailure_ReturnsFalse()
    {
        // Arrange
        var success = ExitCode.Success;
        var failure = ExitCode.Failure;

        // Act
        bool result = success.Equals(failure);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns true when comparing Failure property with a newly created ExitCode with code 1.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_FailureWithOneCode_ReturnsTrue()
    {
        // Arrange
        var failure = ExitCode.Failure;
        var oneExitCode = new ExitCode(1);

        // Act
        bool result = failure.Equals(oneExitCode);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns true for two ExitCode instances with the same positive code values.
    /// Validates various positive values including boundary cases.
    /// </summary>
    /// <param name="code">The positive exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    [InlineData(255)]
    [InlineData(1000)]
    [InlineData(2147483646)]
    [InlineData(int.MaxValue)]
    public void Equals_SamePositiveCodes_ReturnsTrue(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        var exitCode2 = new ExitCode(code);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns true for two ExitCode instances with the same negative code values.
    /// Validates various negative values including boundary cases.
    /// </summary>
    /// <param name="code">The negative exit code value to test.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(-1)]
    [InlineData(-42)]
    [InlineData(-100)]
    [InlineData(-255)]
    [InlineData(-1000)]
    [InlineData(-2147483647)]
    [InlineData(int.MinValue)]
    public void Equals_SameNegativeCodes_ReturnsTrue(int code)
    {
        // Arrange
        var exitCode1 = new ExitCode(code);
        var exitCode2 = new ExitCode(code);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals returns false for two ExitCode instances with different positive code values.
    /// </summary>
    /// <param name="code1">The first exit code value.</param>
    /// <param name="code2">The second exit code value.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1, 2)]
    [InlineData(42, 43)]
    [InlineData(100, 200)]
    [InlineData(1, int.MaxValue)]
    [InlineData(int.MaxValue, 1)]
    [InlineData(2147483646, int.MaxValue)]
    public void Equals_DifferentPositiveCodes_ReturnsFalse(int code1, int code2)
    {
        // Arrange
        var exitCode1 = new ExitCode(code1);
        var exitCode2 = new ExitCode(code2);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing positive and negative code values.
    /// </summary>
    /// <param name="positiveCode">The positive exit code value.</param>
    /// <param name="negativeCode">The negative exit code value.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(1, -1)]
    [InlineData(42, -42)]
    [InlineData(100, -100)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(1, int.MinValue)]
    [InlineData(int.MaxValue, -1)]
    public void Equals_PositiveAndNegativeCodes_ReturnsFalse(int positiveCode, int negativeCode)
    {
        // Arrange
        var positiveExitCode = new ExitCode(positiveCode);
        var negativeExitCode = new ExitCode(negativeCode);

        // Act
        bool result = positiveExitCode.Equals(negativeExitCode);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing boundary values with adjacent values.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_MaxValueAndMaxValueMinusOne_ReturnsFalse()
    {
        // Arrange
        var maxExitCode = new ExitCode(int.MaxValue);
        var maxMinusOneExitCode = new ExitCode(int.MaxValue - 1);

        // Act
        bool result = maxExitCode.Equals(maxMinusOneExitCode);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns false when comparing minimum value with minimum value plus one.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_MinValueAndMinValuePlusOne_ReturnsFalse()
    {
        // Arrange
        var minExitCode = new ExitCode(int.MinValue);
        var minPlusOneExitCode = new ExitCode(int.MinValue + 1);

        // Act
        bool result = minExitCode.Equals(minPlusOneExitCode);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Equals returns true when both ExitCode instances are created with zero value.
    /// Verifies that zero code (Success) is properly handled in equality comparison.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void Equals_TwoZeroCodes_ReturnsTrue()
    {
        // Arrange
        var exitCode1 = new ExitCode(0);
        var exitCode2 = new ExitCode(0);

        // Act
        bool result = exitCode1.Equals(exitCode2);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Equals is symmetric for instances with different codes (both directions return false).
    /// </summary>
    /// <param name="code1">The first exit code value.</param>
    /// <param name="code2">The second exit code value.</param>
    [Theory(Timeout = 10_000)]
    [InlineData(0, 1)]
    [InlineData(1, -1)]
    [InlineData(42, 43)]
    [InlineData(int.MaxValue, int.MinValue)]
    public void Equals_SymmetricPropertyWithDifferentCodes_BothDirectionsReturnFalse(int code1, int code2)
    {
        // Arrange
        var exitCode1 = new ExitCode(code1);
        var exitCode2 = new ExitCode(code2);

        // Act
        bool result1 = exitCode1.Equals(exitCode2);
        bool result2 = exitCode2.Equals(exitCode1);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }
}