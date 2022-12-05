using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterAssemblyRefsToReadableOutputTask : FilterTask
{
    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? fullPath = item.GetMetadata("FullPath");
            string? @private = item.GetMetadata("Private");

            output.Add($"{fullPath ?? "null"}|{@private ?? "null"}");
        }

        Output = string.Join(";", output);
        return true;
    }
}
