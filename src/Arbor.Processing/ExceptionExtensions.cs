using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Arbor.Processing;

internal static class ExceptionExtensions
{
    public static bool IsFatal(this Exception? ex) =>
        ex is OutOfMemoryException
            or AccessViolationException
            or AppDomainUnloadedException
            or StackOverflowException
            or ThreadAbortException
            or SEHException;
}