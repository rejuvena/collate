using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

/// <summary>
///     Filters the output of Rejuvena.Collate.CLI's `mar` command.
/// </summary>
public class FilterModifyAssemblyReferencesTask : FilterTask
{
    /// <summary>
    ///     References to be removed.
    /// </summary>
    [Output]
    public string[] ReferencesToRemove { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     References to be added.
    /// </summary>
    [Output]
    public string[] ReferencesToAdd { get; set; } = Array.Empty<string>();

    protected override bool ExecuteTask() {
        string[] contents = File.ReadAllLines(Input[0].ToString());

        ReferencesToRemove = contents.Where(x => x.StartsWith("-")).Select(x => x.Substring(1)).ToArray();
        ReferencesToAdd    = contents.Where(x => x.StartsWith("+")).Select(x => x.Substring(1)).ToArray();
        return true;
    }
}
