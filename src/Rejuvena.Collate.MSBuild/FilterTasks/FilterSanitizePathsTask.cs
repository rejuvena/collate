using System;
using Microsoft.Build.Framework;

namespace Rejuvena.Collate.MSBuild.FilterTasks;

public class FilterSanitizePathsTask : FilterTask
{
    [Output]
    public string Output { get; set; }

    protected override bool ExecuteTask() {
        Output = Environment.OSVersion.Platform == PlatformID.Win32NT ? Input[0].ToString().Replace('\\', '/') : Input[0].ToString();
        return true;
    }
}
