using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterNuGetRefsToReadableOutputTask : FilterTask
{
    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? hintPath            = item.GetMetadata("HintPath");
            string? nugetPackageId      = item.GetMetadata("NuGetPackageId");
            string? nugetPackageVersion = item.GetMetadata("NuGetPackageVersion");

            output.Add($"{hintPath ?? "null"}|{nugetPackageId ?? "null"}|{nugetPackageVersion ?? "null"}");
        }

        Output = string.Join(";", output);
        return true;
    }
}
