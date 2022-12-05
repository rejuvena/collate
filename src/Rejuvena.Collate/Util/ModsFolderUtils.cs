using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Rejuvena.Collate.Util;

/// <summary>
///     Provides utilities for interacting with the Mods folder and files within it.
/// </summary>
public static class ModsFolderUtils
{
    public static void EnableMod(string enabledPath, string modName) {
        string dir = Path.GetDirectoryName(enabledPath) ?? throw new DirectoryNotFoundException("Could not get directory of: " + enabledPath);
        Directory.CreateDirectory(dir);

        var enabled = File.Exists(enabledPath) ? JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(enabledPath)) : new List<string>();
        enabled?.Add(modName);
        File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled, Formatting.Indented));
    }
}
