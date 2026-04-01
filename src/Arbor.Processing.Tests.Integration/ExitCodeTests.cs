using System;
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
}