using System;

namespace Rejuvena.Collate.MSBuild;

/// <summary>
///     An abstraction over <see cref="BuildTask"/> with minimal extra exception handling.
/// </summary>
public abstract class CollateTask : BuildTask
{
    /// <inheritdoc cref="BuildTask.Execute"/>
    public sealed override bool Execute() {
        try {
            return ExecuteTask();
        }
        catch (Exception e) {
            Log.LogErrorFromException(e);
            return false;
        }
    }

    /// <summary>
    ///     Executes the task.
    /// </summary>
    /// <returns><see langword="true"/>, if successful</returns>
    protected abstract bool ExecuteTask();
}
