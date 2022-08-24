using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Rejuvena.Collate.Extensions;

namespace Rejuvena.Collate
{
    public static class ModEnabler
    {
        public static void EnableMod(TaskLoggingHelper log, string enabledPath, string modName) {
            string dir = Path.GetDirectoryName(enabledPath) ?? throw new DirectoryNotFoundException("Could not get directory of path: " + enabledPath);
            Directory.CreateDirectory(dir);

            List<string>? enabled = File.Exists(enabledPath) ? JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(enabledPath)) : new List<string>();
            File.WriteAllText(enabledPath, JsonConvert.SerializeObject((enabled ?? new List<string>()).With(modName), Formatting.Indented));
        }
    }
}