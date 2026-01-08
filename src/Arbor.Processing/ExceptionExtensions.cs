using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Arbor.Processing;

internal static class ExceptionExtensions
{
    public static bool IsFatal(this Exception? ex)
    {
        if (ex is null)
        {
            return false;
        }

        return ex is OutOfMemoryException
               || ex is AccessViolationException
               || ex is AppDomainUnloadedException
               || ex is StackOverflowException
               || ex is ThreadAbortException
               || ex is SEHException;
    }
}