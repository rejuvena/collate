using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterWriteNuGetRefsTask : FilterTask
{
    [Required]
    public string File { get; set; } = string.Empty;

    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? hintPath            = item.GetMetadata("HintPath");
            string? nugetPackageId      = item.GetMetadata("NuGetPackageId");
            string? nugetPackageVersion = item.GetMetadata("NuGetPackageVersion");

            output.Add($"{hintPath ?? "null"};{nugetPackageId ?? "null"};{nugetPackageVersion ?? "null"}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(File)!);
        System.IO.File.WriteAllText(File, string.Join("\n", output));
        Output = File;
        return true;
    }
}
