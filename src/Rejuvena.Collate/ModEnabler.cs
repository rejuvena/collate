using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;

namespace Rejuvena.Collate
{
    public static class ModEnabler
    {
        public static void EnableMod(TaskLoggingHelper log, string enabledPath, string modName) {
            List<string> enabled = new();
            if (File.Exists(enabledPath))
                enabled = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(enabledPath)) ?? new List<string>();
            enabled.Add(modName);

            Directory.CreateDirectory(Path.GetDirectoryName(enabledPath)!);
            File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled, Formatting.Indented));
        }
    }
}