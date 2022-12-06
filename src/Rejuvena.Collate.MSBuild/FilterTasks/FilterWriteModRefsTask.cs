using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterWriteModRefsTask : FilterTask
{
    [Required]
    public string File { get; set; } = string.Empty;

    [Output]
    public string Output { get; set; } = string.Empty;

    protected override bool ExecuteTask() {
        var output = new List<string>();

        foreach (var item in Input) {
            string? identity = item.GetMetadata("Identity");
            string? hintPath = item.GetMetadata("HintPath") ?? item.GetMetadata("ProjectPath");
            string? weak     = item.GetMetadata("Weak");

            output.Add($"{identity ??"null"};{hintPath ?? "null"};{weak ?? "null"}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(File)!);
        System.IO.File.WriteAllText(File, string.Join("\n", output));
        Output = File;
        return true;
    }
}
