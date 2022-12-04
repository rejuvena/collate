using System;

namespace Rejuvena.Collate.MSBuild;

/// <summary>
///     An abstraction over <see cref="BuildTask"/> with minimal extra exception handling.
/// </summary>
public abstract class CollateTask : BuildTask
{
    public sealed override bool Execute() {
        try {
            return ExecuteTask();
        }
        catch (Exception e) {
            Log.LogErrorFromException(e);
            return false;
        }
    }

    protected abstract bool ExecuteTask();
}
