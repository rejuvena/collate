using System;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterModifyAssemblyReferencesTask : FilterTask
{
    [Output]
    public string[] ReferencesToRemove { get; set; } = Array.Empty<string>();
    
    [Output]
    public string[] ReferencesToAdd { get; set; } = Array.Empty<string>();

    protected override bool ExecuteTask() {
        foreach (var taskItem in Input) {
            Log.LogError(taskItem.ToString());
        }

        return true;
    }
}
