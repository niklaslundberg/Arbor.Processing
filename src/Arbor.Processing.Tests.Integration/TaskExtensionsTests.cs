using System;
using System.Threading;
using System.Threading.Tasks;

using Arbor.Processing;
using AwesomeAssertions;
using Xunit;

namespace Arbor.Processing.UnitTests;



public class TaskExtensionsTests
{
    /// <summary>
    /// Tests that TimeoutTask returns a non-null Task when provided with a default cancellation token.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void TimeoutTask_WithDefaultCancellationToken_ReturnsNonNullTask()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var task = TaskExtensions.TimeoutTask(cancellationToken);

        // Assert
        task.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that TimeoutTask returns a task that does not complete when using CancellationToken.None.
    /// The task should remain incomplete as it uses infinite delay.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public async Task TimeoutTask_WithNoneCancellationToken_TaskDoesNotComplete()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var task = TaskExtensions.TimeoutTask(cancellationToken);
        var completedTask = await Task.WhenAny(task, Task.Delay(100));

        // Assert
        completedTask.Should().NotBe(task);
        task.IsCompleted.Should().BeFalse();
    }

    /// <summary>
    /// Tests that TimeoutTask returns a cancelled task when provided with an already cancelled token.
    /// The task should complete immediately in a cancelled state.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void TimeoutTask_WithAlreadyCancelledToken_ReturnsCancelledTask()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var task = TaskExtensions.TimeoutTask(cts.Token);

        // Assert
        task.IsCanceled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that TimeoutTask returns a task that becomes cancelled when the cancellation token is cancelled after task creation.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public async Task TimeoutTask_WhenTokenCancelledAfterCreation_TaskBecomesCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var task = TaskExtensions.TimeoutTask(cts.Token);

        // Act
        cts.Cancel();
        await Task.Delay(50);

        // Assert
        task.IsCanceled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that TimeoutTask throws TaskCanceledException when awaiting a task created with a pre-cancelled token.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public async Task TimeoutTask_WithCancelledToken_ThrowsTaskCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = TaskExtensions.TimeoutTask(cts.Token);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

    /// <summary>
    /// Tests that TimeoutTask throws TaskCanceledException when the token is cancelled after task creation and the task is awaited.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public async Task TimeoutTask_WhenTokenCancelledAfterCreationAndAwaited_ThrowsTaskCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var task = TaskExtensions.TimeoutTask(cts.Token);

        // Act
        cts.CancelAfter(50);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await task);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Tests that CanBeAwaited throws ArgumentNullException when task is null on .NET 6.0 or greater.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_NullTask_ThrowsArgumentNullException()
    {
        // Arrange
        Task<int> task = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => task.CanBeAwaited());
    }
#else
    /// <summary>
    /// Tests that CanBeAwaited throws ArgumentNullException when task is null on frameworks before .NET 6.0.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_NullTask_ThrowsArgumentNullException()
    {
        // Arrange
        Task<int> task = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => task.CanBeAwaited());
    }
#endif

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is completed successfully.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_CompletedTask_ReturnsTrue()
    {
        // Arrange
        Task<int> task = Task.FromResult(42);

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is faulted.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_FaultedTask_ReturnsTrue()
    {
        // Arrange
        Task<string> task = Task.FromException<string>(new InvalidOperationException("Test exception"));

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is canceled.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_CanceledTask_ReturnsTrue()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        Task<object> task = Task.FromCanceled<object>(cts.Token);

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns false when task is not yet completed, faulted, or canceled.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_RunningTask_ReturnsFalse()
    {
        // Arrange
        var tcs = new TaskCompletionSource<int>();
        Task<int> task = tcs.Task;

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true for completed task with different generic type.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_CompletedTaskWithStringType_ReturnsTrue()
    {
        // Arrange
        Task<string> task = Task.FromResult("test value");

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true for completed task with object type.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_CompletedTaskWithObjectType_ReturnsTrue()
    {
        // Arrange
        Task<object> task = Task.FromResult(new object());

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns false for running task with different generic type.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_RunningTaskWithStringType_ReturnsFalse()
    {
        // Arrange
        var tcs = new TaskCompletionSource<string>();
        Task<string> task = tcs.Task;

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that TimeoutTask returns a non-null Task when provided with CancellationToken.None.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void TimeoutTask_WithNoneCancellationToken_ReturnsNonNullTask()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var task = TaskExtensions.TimeoutTask(cancellationToken);

        // Assert
        task.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that TimeoutTask returns a task that is not completed when created with a non-cancelled token.
    /// Verifies the task is in the correct initial state.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void TimeoutTask_WithNonCancelledToken_ReturnsNonCompletedTask()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var task = TaskExtensions.TimeoutTask(cts.Token);

        // Assert
        task.IsCompleted.Should().BeFalse();
        task.IsCanceled.Should().BeFalse();
        task.IsFaulted.Should().BeFalse();
    }

    /// <summary>
    /// Tests that TimeoutTask handles multiple cancellation token sources correctly.
    /// Verifies that each task is independently associated with its cancellation token.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void TimeoutTask_WithMultipleCancellationTokenSources_EachTaskIndependent()
    {
        // Arrange
        using var cts1 = new CancellationTokenSource();
        using var cts2 = new CancellationTokenSource();

        // Act
        var task1 = TaskExtensions.TimeoutTask(cts1.Token);
        var task2 = TaskExtensions.TimeoutTask(cts2.Token);
        cts1.Cancel();

        // Assert
        task1.IsCanceled.Should().BeTrue();
        task2.IsCanceled.Should().BeFalse();
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Tests that CanBeAwaited throws ArgumentNullException when task is null on .NET 6.0 or greater.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_Generic_NullTask_ThrowsArgumentNullException()
    {
        // Arrange
        Task<int> task = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => task.CanBeAwaited());
    }
#else
    /// <summary>
    /// Tests that CanBeAwaited throws ArgumentNullException when task is null on frameworks before .NET 6.0.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_Generic_NullTask_ThrowsArgumentNullException()
    {
        // Arrange
        Task<int> task = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => task.CanBeAwaited());
    }
#endif

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is successfully completed.
    /// Verifies with different generic type parameters to ensure type-agnostic behavior.
    /// </summary>
    /// <param name="taskFactory">Factory method to create a completed task of specific type.</param>
    [Theory(Timeout = 10_000)]
    [MemberData(nameof(CompletedTaskTestCases))]
    public void CanBeAwaited_Generic_CompletedTask_ReturnsTrue(Func<Task> taskFactory)
    {
        // Arrange
        var task = taskFactory();

        // Act
        var result = InvokeCanBeAwaited(task);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is in a faulted state.
    /// Verifies with different generic type parameters and exception types.
    /// </summary>
    /// <param name="taskFactory">Factory method to create a faulted task of specific type.</param>
    [Theory(Timeout = 10_000)]
    [MemberData(nameof(FaultedTaskTestCases))]
    public void CanBeAwaited_Generic_FaultedTask_ReturnsTrue(Func<Task> taskFactory)
    {
        // Arrange
        var task = taskFactory();

        // Act
        var result = InvokeCanBeAwaited(task);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true when task is in a canceled state.
    /// Verifies with different generic type parameters.
    /// </summary>
    /// <param name="taskFactory">Factory method to create a canceled task of specific type.</param>
    [Theory(Timeout = 10_000)]
    [MemberData(nameof(CanceledTaskTestCases))]
    public void CanBeAwaited_Generic_CanceledTask_ReturnsTrue(Func<Task> taskFactory)
    {
        // Arrange
        var task = taskFactory();

        // Act
        var result = InvokeCanBeAwaited(task);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns false when task is not yet completed, faulted, or canceled.
    /// Verifies with different generic type parameters.
    /// </summary>
    /// <param name="taskFactory">Factory method to create a running/pending task of specific type.</param>
    [Theory(Timeout = 10_000)]
    [MemberData(nameof(RunningTaskTestCases))]
    public void CanBeAwaited_Generic_RunningTask_ReturnsFalse(Func<Task> taskFactory)
    {
        // Arrange
        var task = taskFactory();

        // Act
        var result = InvokeCanBeAwaited(task);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true for a task with nullable value type that completed with null result.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_Generic_CompletedTaskWithNullableValueType_ReturnsTrue()
    {
        // Arrange
        Task<int?> task = Task.FromResult<int?>(null);

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanBeAwaited returns true for a task with nullable reference type that completed with null result.
    /// </summary>
    [Fact(Timeout = 10_000)]
    public void CanBeAwaited_Generic_CompletedTaskWithNullResult_ReturnsTrue()
    {
        // Arrange
        Task<string> task = Task.FromResult<string>(null!);

        // Act
        bool result = task.CanBeAwaited();

        // Assert
        result.Should().BeTrue();
    }

    public static TheoryData<Func<Task>> CompletedTaskTestCases => new()
    {
        () => Task.FromResult(42),
        () => Task.FromResult("test string"),
        () => Task.FromResult(new object()),
        () => Task.FromResult(true),
        () => Task.FromResult(3.14),
        () => Task.FromResult<int?>(100)
    };

    public static TheoryData<Func<Task>> FaultedTaskTestCases => new()
    {
        () => Task.FromException<int>(new InvalidOperationException("Test exception")),
        () => Task.FromException<string>(new ArgumentException("Test argument exception")),
        () => Task.FromException<object>(new NullReferenceException("Test null reference")),
        () => Task.FromException<bool>(new Exception("Generic exception")),
        () => Task.FromException<double>(new InvalidCastException("Cast exception"))
    };

    public static TheoryData<Func<Task>> CanceledTaskTestCases
    {
        get
        {
            var data = new TheoryData<Func<Task>>();

            var cts1 = new CancellationTokenSource();
            cts1.Cancel();
            data.Add(() => Task.FromCanceled<int>(cts1.Token));

            var cts2 = new CancellationTokenSource();
            cts2.Cancel();
            data.Add(() => Task.FromCanceled<string>(cts2.Token));

            var cts3 = new CancellationTokenSource();
            cts3.Cancel();
            data.Add(() => Task.FromCanceled<object>(cts3.Token));

            var cts4 = new CancellationTokenSource();
            cts4.Cancel();
            data.Add(() => Task.FromCanceled<bool>(cts4.Token));

            return data;
        }
    }

    public static TheoryData<Func<Task>> RunningTaskTestCases => new()
    {
        () => new TaskCompletionSource<int>().Task,
        () => new TaskCompletionSource<string>().Task,
        () => new TaskCompletionSource<object>().Task,
        () => new TaskCompletionSource<bool>().Task,
        () => new TaskCompletionSource<double>().Task
    };

    private static bool InvokeCanBeAwaited(Task task)
    {
        var method = typeof(TaskExtensions).GetMethod(nameof(TaskExtensions.CanBeAwaited));
        var genericMethod = method!.MakeGenericMethod(task.GetType().GetGenericArguments()[0]);
        return (bool)genericMethod.Invoke(null, new object[] { task })!;
    }
}