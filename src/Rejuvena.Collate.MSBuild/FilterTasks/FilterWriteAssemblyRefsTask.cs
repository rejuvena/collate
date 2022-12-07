using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterWriteAssemblyRefsTask : FilterTask
{
    [Required]
    public string File { get; set; } = string.Empty;

    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? fullPath = item.GetMetadata("FullPath");
            string? @private = item.GetMetadata("Private");

            output.Add($"{fullPath ?? "null"};{@private ?? "null"}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(File)!);
        System.IO.File.WriteAllText(File, string.Join("\n", output));
        Output = File;
        return true;
    }
}
