using System;
using System.Threading;
using System.Threading.Tasks;

namespace Arbor.Processing;

internal static class TaskExtensions
{
    public static bool CanBeAwaited(this Task task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return task.IsCompleted || task.IsFaulted || task.IsCanceled;
    }

    public static bool CanBeAwaited<T>(this Task<T> task)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        return task.IsCompleted || task.IsFaulted || task.IsCanceled;
    }

    public static Task TimeoutTask(CancellationToken cancellationToken) => Task.Delay(-1, cancellationToken);
}