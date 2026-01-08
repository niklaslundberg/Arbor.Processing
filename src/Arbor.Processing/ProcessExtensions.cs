using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Arbor.Processing;

public static class ProcessExtensions
{
    internal static bool? IsWin64(this Process process)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        if (Environment.OSVersion.Version.Major > 5
            || (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1))
        {
            IntPtr processHandle;

            try
            {
                processHandle = Process.GetProcessById(process.Id).Handle;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                return false;
            }

            return NativeMethods.IsWow64Process(processHandle, out bool retVal) && retVal;
        }

        return false;
    }
}