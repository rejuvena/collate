using System;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

/// <summary>
///     An abstraction over <see cref="CollateTask"/> used for filtering <c>Exec</c> task outputs.
/// </summary>
public abstract class FilterTask : CollateTask
{
    [Required]
    public ITaskItem[] Input { get; set; } = Array.Empty<ITaskItem>();
}
