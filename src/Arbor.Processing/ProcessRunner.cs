using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Arbor.Processing;

public sealed class ProcessRunner : IDisposable
{
    private const string ProcessRunnerName = $"[{nameof(ProcessRunner)}]";
    private CategoryLog? _debugAction;
    private bool _disposed;
    private bool _disposing;

    private ExitCode? _exitCode;
    private Process? _process;
    private int? _processId;
    private string? _processWithArgs;
    private bool _shouldDispose;
    private CategoryLog? _standardErrorAction;

    private CategoryLog? _standardOutLog;
    private TaskCompletionSource<ExitCode>? _taskCompletionSource;
    private CategoryLog? _toolAction;
    private CategoryLog? _verboseAction;

    private ProcessRunner()
    {
        _process = new Process();
        _taskCompletionSource =
            new TaskCompletionSource<ExitCode>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private bool NeedsCleanup => !_disposed || !_disposing || _process is { } || _shouldDispose;

    public void Dispose()
    {
        if (!_disposed && !_disposing)
        {
            _disposing = true;

            if (_verboseAction is { } && _taskCompletionSource is { })
            {
                _verboseAction?.Invoke(
                    $"Task status for process {_processWithArgs}: {_taskCompletionSource.Task.Status}; is completed: {_taskCompletionSource.Task.IsCompleted}",
                    ProcessRunnerName);

                _verboseAction?.Invoke($"Disposing process {_processWithArgs}", ProcessRunnerName);
            }

            if (_taskCompletionSource?.Task.CanBeAwaited() == false)
            {
                _standardErrorAction?.Invoke("Task completion was not set on dispose, setting to failure",
                    ProcessRunnerName);

                _taskCompletionSource.TrySetResult(ExitCode.Failure);
            }

            bool needsDisposeCheck = _taskCompletionSource?.Task.IsCompleted == false;

            _taskCompletionSource = null;

            DisposeProcess(needsDisposeCheck);

            _disposed = true;
            _disposing = false;
        }

        _process = null;
        _verboseAction?.Invoke($"Dispose completed for process {_processWithArgs}", ProcessRunnerName);
    }

    private void DisposeProcess(bool needsDisposeCheck)
    {
        if (_process is { })
        {
            try
            {
                TryCleanupProcess();
            }
            catch (Exception ex)
            {
                _verboseAction?.Invoke("Could not get exit status in dispose " + ex, null);
            }

            if (!needsDisposeCheck && _process is { })
            {
                _process.Disposed -= OnDisposed;
            }

            _process?.Dispose();

            if (needsDisposeCheck && _process is { })
            {
                _process.Disposed -= OnDisposed;
            }

            if (_process is { })
            {
                _process.Exited -= OnExited;
            }
        }
    }

    private Task<ExitCode> ExecuteAsync(
        string executePath,
        IEnumerable<string>? arguments = null,
        CategoryLog? standardOutLog = null,
        CategoryLog? standardErrorAction = null,
        CategoryLog? toolAction = null,
        CategoryLog? verboseAction = null,
        IEnumerable<KeyValuePair<string, string>>? environmentVariables = null,
        CategoryLog? debugAction = null,
        bool noWindow = true,
        bool? shellExecute = false,
        bool? formatArgs = true,
        DirectoryInfo? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ThrowIfDisposing();

        if (string.IsNullOrWhiteSpace(executePath))
        {
            throw new ArgumentNullException(nameof(executePath));
        }

        if (!File.Exists(executePath))
        {
            throw new ArgumentException(
                $"The executable file '{executePath}' does not exist",
                nameof(executePath));
        }

        IEnumerable<string> usedArguments = arguments ?? [];

        string[] formattedArguments = formatArgs ?? false
            ? usedArguments.Select(arg => $"\"{arg}\"").ToArray()
            : usedArguments.ToArray();

        return RunProcessAsync(executePath,
            formattedArguments,
            standardErrorAction,
            standardOutLog,
            toolAction,
            verboseAction,
            environmentVariables,
            debugAction,
            noWindow,
            shellExecute,
            workingDirectory,
            cancellationToken);
    }

    private bool IsAlive(CancellationToken cancellationToken)
    {
        if (CheckedDisposed())
        {
            _verboseAction?.Invoke($"Process {_processWithArgs} does no longer exist", ProcessRunnerName);
            return false;
        }

        if (_taskCompletionSource is { Task.IsCompleted: true })
        {
            return false;
        }

        _process?.Refresh();

        try
        {
            if (_process?.HasExited == true)
            {
                return false;
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            //ignore
        }

        if (_taskCompletionSource is { } && _taskCompletionSource.Task.CanBeAwaited())
        {
            TaskStatus status = _taskCompletionSource.Task.Status;
            _verboseAction?.Invoke($"Task status for process {_processWithArgs} is: {status}", ProcessRunnerName);
            return false;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            _verboseAction?.Invoke($"Cancellation is requested for process {_processWithArgs}", ProcessRunnerName);
            return false;
        }

        if (_exitCode.HasValue)
        {
            _verboseAction?.Invoke(
                $"Process {_processWithArgs} is flagged as done with exit code {_exitCode.Value}",
                ProcessRunnerName);
            return false;
        }

        return _taskCompletionSource is { } && !_taskCompletionSource.Task.CanBeAwaited();
    }

    private bool CheckedDisposed() => _disposed || _disposing;

    private async Task<ExitCode> RunProcessAsync(
        string executePath,
        string[] arguments,
        CategoryLog? standardErrorAction,
        CategoryLog? standardOutputLog,
        CategoryLog? toolAction,
        CategoryLog? verboseAction = null,
        IEnumerable<KeyValuePair<string, string>>? environmentVariables = null,
        CategoryLog? debugAction = null,
        bool noWindow = true,
        bool? shellExecute = false,
        DirectoryInfo? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ThrowIfDisposing();

        cancellationToken.Register(EnsureTaskIsCompleted);

        _toolAction = toolAction;
        _standardOutLog = standardOutputLog;
        _standardErrorAction = standardErrorAction;
        _verboseAction = verboseAction;
        _debugAction = debugAction;

        if (!File.Exists(executePath))
        {
            throw new InvalidOperationException($"The executable file '{executePath}' does not exist");
        }

        var executableFile = new FileInfo(executePath);

        string processName = $"{ProcessRunnerName} [{executableFile.Name}]";

        string formattedArguments = string.Join(" ", arguments);

        _processWithArgs = $"\"{executePath}\" {formattedArguments}".Trim();

        _toolAction?.Invoke($"Executing: {_processWithArgs}", ProcessRunnerName);

        bool useShellExecute = shellExecute ?? (standardErrorAction is null && standardOutputLog == null);

        bool redirectStandardError = standardErrorAction is { };

        bool redirectStandardOutput = standardOutputLog is { };

        KeyValuePair<string, string>[] usedEnvironmentVariables =
            environmentVariables?.ToArray() ?? [];

        formattedArguments = GetArguments(usedEnvironmentVariables, formattedArguments);

        var processStartInfo = new ProcessStartInfo(executePath)
        {
            Arguments = formattedArguments,
            RedirectStandardError = redirectStandardError,
            RedirectStandardOutput = redirectStandardOutput,
            UseShellExecute = useShellExecute,
            CreateNoWindow = noWindow
        };

        SetWorkingDirectory(workingDirectory, processStartInfo);

        AddProcessEnvironmentVariables(usedEnvironmentVariables, processStartInfo);

        EnsureProcessIsSet();

        _process!.StartInfo = processStartInfo;
        _process.Exited += OnExited;
        _process.Disposed += OnDisposed;

        SetupStandardErrorRedirect(redirectStandardError, processName);

        SetupStandardOutRedirect(redirectStandardOutput, processName);

        _process.EnableRaisingEvents = true;

        try
        {
            bool started = _process.Start();

            if (!started && _taskCompletionSource is { })
            {
                _standardErrorAction?.Invoke($"Process {_processWithArgs} could not be started", null);

                SetFailureResult();

                await Task.WhenAny(_taskCompletionSource.Task, TaskExtensions.TimeoutTask(cancellationToken));

                return await _taskCompletionSource.Task;
            }

            if (redirectStandardError)
            {
                _process.BeginErrorReadLine();
            }

            if (redirectStandardOutput)
            {
                _process.BeginOutputReadLine();
            }

            int? bits = GetProcessBits();

            try
            {
                _processId = _process.Id;
                _processWithArgs = $"{_processWithArgs} id {_processId}";
            }
            catch (InvalidOperationException ex)
            {
                _debugAction?.Invoke($"Could not get process id for process '{_processWithArgs}'. {ex}", null);
            }

            string temp = _process.HasExited ? "was" : "is";

            if (bits.HasValue)
            {
                _verboseAction?.Invoke(
                    $"The process '{_processWithArgs}' {temp} running in {bits}-bit mode",
                    ProcessRunnerName);
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _standardErrorAction?.Invoke($"An error occured while running process {_processWithArgs}: {ex}",
                ProcessRunnerName);
            SetResultException(ex);
        }

        if (_taskCompletionSource is { } && _taskCompletionSource.Task.CanBeAwaited())
        {
            await Task.WhenAny(_taskCompletionSource.Task, TaskExtensions.TimeoutTask(cancellationToken));

            return await _taskCompletionSource.Task;
        }

        try
        {
            while (IsAlive(cancellationToken)
                   && !cancellationToken.IsCancellationRequested
                   && _taskCompletionSource is { })
            {
                var delay = Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

                await Task.WhenAny(_taskCompletionSource.Task, TaskExtensions.TimeoutTask(cancellationToken));

                await _taskCompletionSource.Task;

                await delay;

                await SetExitCode();
            }
        }
        finally
        {
            if ((_exitCode?.IsSuccess is null || !_exitCode.Value.IsSuccess)
                && cancellationToken.IsCancellationRequested
                && NeedsCleanup)
            {
                _shouldDispose = true;
            }
        }

        try
        {
            if (_processId > 0)
            {
                bool stillAlive = false;

                using (Process? stillRunningProcess =
                       Process.GetProcesses().SingleOrDefault(p => p.Id == _processId))
                {
                    if (stillRunningProcess is { HasExited: false })
                    {
                        stillAlive = true;
                    }
                }

                if (stillAlive && _taskCompletionSource is { })
                {
                    _verboseAction?.Invoke(
                        $"The process with ID {_processId?.ToString(CultureInfo.InvariantCulture) ?? "N/A"} '{_processWithArgs}' is still running",
                        ProcessRunnerName);
                    SetFailureResult();

                    await Task.WhenAny(_taskCompletionSource.Task, TaskExtensions.TimeoutTask(cancellationToken));

                    await _taskCompletionSource.Task;
                }
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            debugAction?.Invoke($"Could not check processes. {ex}", ProcessRunnerName);
        }

        if (_taskCompletionSource is null)
        {
            throw new InvalidOperationException("Task completion source is null");
        }

        await Task.WhenAny(_taskCompletionSource.Task, TaskExtensions.TimeoutTask(cancellationToken));

        ExitCode result = await _taskCompletionSource.Task;

        _verboseAction?.Invoke($"Process runner exit code {_exitCode} for process {_processWithArgs}",
            ProcessRunnerName);

        return result;
    }

    private static void SetWorkingDirectory(DirectoryInfo? workingDirectory, ProcessStartInfo processStartInfo)
    {
        if (workingDirectory is null)
        {
            return;
        }

        processStartInfo.WorkingDirectory = workingDirectory.FullName;
    }

    private void EnsureProcessIsSet()
    {
        if (_process is null)
        {
            throw new InvalidOperationException("Process is not defined");
        }
    }

    private void SetupStandardOutRedirect(bool redirectStandardOutput, string processName)
    {
        if (!redirectStandardOutput || _standardOutLog is null)
        {
            return;
        }

        if (_process is null)
        {
            return;
        }

        _process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _standardOutLog(args.Data, processName);
            }
        };
    }

    private void SetupStandardErrorRedirect(bool redirectStandardError, string processName)
    {
        if (!redirectStandardError)
        {
            return;
        }

        if (_process is null)
        {
            return;
        }

        _process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _standardErrorAction?.Invoke(args.Data, processName);
            }
        };
    }

    private async Task SetExitCode()
    {
        if (_taskCompletionSource is null)
        {
            return;
        }

        if (_taskCompletionSource.Task.IsCompleted ||
            _taskCompletionSource.Task.IsCanceled ||
            _taskCompletionSource.Task.IsFaulted)
        {
            _exitCode = await _taskCompletionSource.Task;
        }
    }

    private static void AddProcessEnvironmentVariables(KeyValuePair<string, string>[] usedEnvironmentVariables,
        ProcessStartInfo processStartInfo)
    {
        if (usedEnvironmentVariables.Length <= 0)
        {
            return;
        }

        foreach (KeyValuePair<string, string> environmentVariable in usedEnvironmentVariables)
        {
            processStartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
        }
    }

    private static string GetArguments(KeyValuePair<string, string>[] usedEnvironmentVariables, string formattedArguments)
    {
        if (usedEnvironmentVariables.Length <= 0)
        {
            return formattedArguments;
        }

        foreach (KeyValuePair<string, string> pair in usedEnvironmentVariables)
        {
            formattedArguments = formattedArguments.Replace($"%{pair.Key}%", pair.Value);
        }

        return formattedArguments;
    }

    private int? GetProcessBits()
    {
        bool? isWin64 = _process?.IsWin64();

        if (isWin64 is null)
        {
            return null;
        }

        return isWin64.Value ? 64 : 32;
    }

    private void TryCleanupProcess()
    {
        _verboseAction?.Invoke($"Trying to stop process {_processWithArgs}", ProcessRunnerName);

        if (!NeedsCleanup)
        {
            _verboseAction?.Invoke("No cleanup is needed", ProcessRunnerName);
            return;
        }

        if (_process is null)
        {
            _verboseAction?.Invoke("No cleanup is needed, process is null", ProcessRunnerName);
            return;
        }

        HandleUnfinishedProcess();
    }

    private void HandleUnfinishedProcess()
    {
        try
        {
            _process?.Refresh();
            if (!_exitCode.HasValue)
            {
                _toolAction?.Invoke($"Cancellation is requested, trying to stop process {_processWithArgs}",
                    ProcessRunnerName);

                bool forceCloseWithStopProcess = true;
                try
                {
                    if (_process?.HasExited == false)
                    {
                        _process?.Kill();
                    }

                    if (_process?.HasExited == true)
                    {
                        forceCloseWithStopProcess = false;
                        _verboseAction?.Invoke($"Successfully stopped process {_processWithArgs}",
                            ProcessRunnerName);
                    }
                    else
                    {
                        _verboseAction?.Invoke($"Could not stop process {_processWithArgs}", ProcessRunnerName);
                    }
                }
                catch (Exception ex)
                {
                    _verboseAction?.Invoke($"Got exception, trying taskkill.exe {ex}", ProcessRunnerName);
                    forceCloseWithStopProcess = true;
                }

                if (forceCloseWithStopProcess)
                {
                    _verboseAction?.Invoke($"Force stopping process {_processWithArgs}", ProcessRunnerName);

                    using (Process? foundProcess = Process.GetProcesses().SingleOrDefault(p => p.Id == _processId))
                    {
                        if (foundProcess is null)
                        {
                            return;
                        }
                    }


                    KillProcess();
                }
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _standardErrorAction?.Invoke(
                $"ProcessRunner could not stop process {_processWithArgs} when cancellation was requested",
                ProcessRunnerName);

            _standardErrorAction?.Invoke(
                $"Could not stop process {_processWithArgs} when cancellation was requested",
                ProcessRunnerName);

            _standardErrorAction?.Invoke(ex.ToString(), ProcessRunnerName);
        }
        finally
        {
            _verboseAction?.Invoke($"Ended process cleanup for process {_processWithArgs}", ProcessRunnerName);
        }
    }

    private void KillProcess()
    {
        if (_processId is null or < 0)
        {
            _debugAction?.Invoke(
                $"Could not stop process '{_processWithArgs}', missing process id",
                ProcessRunnerName);
            return;
        }

        string args = $"/PID {_processId}";
        string stopProcessPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                "taskkill.exe");

        _toolAction?.Invoke($"Running {stopProcessPath} {args}", ProcessRunnerName);

        using (var taskKillProcess = Process.Start(stopProcessPath, args))
        {
            if (taskKillProcess is { Id: > 0 })
            {
                try
                {
                    int waited = 0;
                    const int maxWait = 5000;
                    const int waitInterval = 1000;

                    while (!taskKillProcess.WaitForExit(1000) && waited <= maxWait)
                    {
                        waited += waitInterval;

                        if (!taskKillProcess.HasExited)
                        {
                            continue;
                        }

                        _verboseAction?.Invoke(
                            "External process stop exit code " + taskKillProcess.ExitCode,
                            ProcessRunnerName);

                        using Process? foundProcess = Process.GetProcesses()
                            .SingleOrDefault(pr => pr.Id == _processId);

                        if (foundProcess is null)
                        {
                            _verboseAction?.Invoke($"Process is stopped {_processWithArgs}",
                                ProcessRunnerName);
                        }
                        else
                        {
                            foundProcess.Kill();
                        }
                    }
                }
                catch (Exception stopEx)
                {
                    _verboseAction?.Invoke($"Could not stop process {_processWithArgs} {stopEx}",
                        ProcessRunnerName);
                }
            }
        }

        _standardErrorAction?.Invoke(
            $"Stopped process {_processWithArgs} because cancellation was requested",
            ProcessRunnerName);
    }

    private void DisposeAndStopProcessIfRunning()
    {
        if (_disposed)
        {
            return;
        }

        if (_disposing)
        {
            return;
        }

        if (_process is null)
        {
            return;
        }

        _shouldDispose = true;
    }

    private void EnsureTaskIsCompleted()
    {
        try
        {
            if (!CheckedDisposed() && _taskCompletionSource?.Task.CanBeAwaited() == false)
            {
                _taskCompletionSource.TrySetCanceled();
            }
        }
        finally
        {
            if (NeedsCleanup)
            {
                DisposeAndStopProcessIfRunning();
            }
        }
    }

    private void OnExited(object? sender, EventArgs e)
    {
        if (sender is not Process proc)
        {
            if (_taskCompletionSource is { } && !_taskCompletionSource.Task.CanBeAwaited())
            {
                _standardErrorAction?.Invoke("Task is not in a valid state, sender is not process",
                    ProcessRunnerName);
                SetFailureResult();
            }

            return;
        }

        proc.EnableRaisingEvents = false;

        if (_taskCompletionSource?.Task.CanBeAwaited() == true)
        {
            return;
        }

        proc.Refresh();
        int procExitCode;
        try
        {
            if (_taskCompletionSource?.Task.CanBeAwaited() == true)
            {
                return;
            }

            procExitCode = proc.ExitCode;
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _standardErrorAction?.Invoke($"Failed to get exit code from process {ex}", ProcessRunnerName);

            SetResultException(ex);

            return;
        }

        var result = new ExitCode(procExitCode);
        _toolAction?.Invoke($"Process {_processWithArgs} exited with code {result}",
            ProcessRunnerName);

        if (_taskCompletionSource?.Task.CanBeAwaited() == false)
        {
            SetSuccessResult(result);
        }
    }

    private void SetSuccessResult(ExitCode result)
    {
        ThrowIfDisposed();
        ThrowIfDisposing();

        if (_taskCompletionSource is null)
        {
            return;
        }

        if (_taskCompletionSource.Task.CanBeAwaited()
            && _taskCompletionSource.Task.IsCompleted && _taskCompletionSource.Task.Result != result)
        {
            _toolAction?.Invoke(
                $"Task result has already been set to {_taskCompletionSource.Task.Status}, cannot re-set to exit code to {result}",
                ProcessRunnerName);
        }

        _taskCompletionSource.TrySetResult(result);
    }

    private void SetResultException(Exception ex)
    {
        ThrowIfDisposed();
        ThrowIfDisposing();

        if (_taskCompletionSource is null)
        {
            return;
        }

        if (_taskCompletionSource.Task.CanBeAwaited())
        {
            throw new InvalidOperationException(
                $"Task result has already been set to {_taskCompletionSource.Task.Status}, cannot re-set to with exception",
                ex);
        }

        _taskCompletionSource.TrySetException(ex);
    }

    private void SetFailureResult()
    {
        ThrowIfDisposed();
        ThrowIfDisposing();

        if (_taskCompletionSource is null)
        {
            return;
        }

        if (_taskCompletionSource.Task.CanBeAwaited()
            && (!_taskCompletionSource.Task.IsCompleted
                || _taskCompletionSource.Task.Result != ExitCode.Failure))
        {
            throw new InvalidOperationException(
                $"Task result has already been set to {_taskCompletionSource.Task.Status}, cannot re-set to exit code to {ExitCode.Failure}");
        }

        _taskCompletionSource.TrySetResult(ExitCode.Failure);
    }

    private void OnDisposed(object? sender, EventArgs _)
    {
        if (_taskCompletionSource is { } && !_taskCompletionSource.Task.CanBeAwaited())
        {
            _verboseAction?.Invoke($"Task was not completed, but process was disposed {_processWithArgs}",
                ProcessRunnerName);
            SetFailureResult();
        }

        _verboseAction?.Invoke($"Disposed process {_processWithArgs}", ProcessRunnerName);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ProcessRunnerName),
                $"Process {_processWithArgs} is already disposed");
        }
    }

    private void ThrowIfDisposing()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"Disposing in progress for process {_processWithArgs}");
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="executePath"></param>
    /// <param name="arguments"></param>
    /// <param name="standardOutLog">(message, category)</param>
    /// <param name="standardErrorAction">(message, category)</param>
    /// <param name="toolAction">(message, category)</param>
    /// <param name="verboseAction">(message, category)</param>
    /// <param name="environmentVariables"></param>
    /// <param name="debugAction"></param>
    /// <param name="noWindow"></param>
    /// <param name="formatArgs"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="shellExecute"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<ExitCode> ExecuteProcessAsync(
        string executePath,
        IEnumerable<string>? arguments = null,
        CategoryLog? standardOutLog = null,
        CategoryLog? standardErrorAction = null,
        CategoryLog? toolAction = null,
        CategoryLog? verboseAction = null,
        IEnumerable<KeyValuePair<string, string>>? environmentVariables = null,
        CategoryLog? debugAction = null,
        bool noWindow = true,
        bool? shellExecute = false,
        bool? formatArgs = true,
        DirectoryInfo? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executePath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(executePath));
        }

        if (!File.Exists(executePath))
        {
            throw new ArgumentException($"Executable path {executePath} does not exist");
        }

        ExitCode exitCode;

        var processStopWatch = Stopwatch.StartNew();

        string[] args = arguments?.ToArray() ?? [];

        try
        {
            using var runner = new ProcessRunner();
            exitCode = await runner.ExecuteAsync(
                executePath,
                args,
                standardOutLog,
                standardErrorAction,
                toolAction,
                verboseAction,
                environmentVariables,
                debugAction,
                noWindow,
                shellExecute,
                formatArgs,
                workingDirectory,
                cancellationToken);

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
        finally
        {
            processStopWatch.Stop();

            string processWithArgs = $"\"{executePath}\" {string.Join(" ", args.Select(arg => $"\"{arg}\""))}";
            toolAction?.Invoke(
                $"Running process {processWithArgs} took {processStopWatch.Elapsed.TotalMilliseconds:F1} milliseconds",
                ProcessRunnerName);
        }


        return exitCode;
    }
}