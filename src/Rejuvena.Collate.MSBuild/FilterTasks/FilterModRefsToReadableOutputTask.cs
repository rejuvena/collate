using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterModRefsToReadableOutputTask : FilterTask
{
    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? hintPath = item.GetMetadata("HintPath") ?? item.GetMetadata("ProjectPath");
            string? identity = item.GetMetadata("Identity");
            string? weak     = item.GetMetadata("Weak");

            output.Add($"{hintPath ?? "null"}|{identity ?? "null"}|{weak ?? "null"}");
        }

        Output = string.Join(";", output);
        return true;
    }
}
