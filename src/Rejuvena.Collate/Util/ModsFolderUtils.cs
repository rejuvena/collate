using System;
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
        if (enabled is not null && !enabled.Contains(modName)) enabled.Add(modName);
        File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled, Formatting.Indented));
    }

    // When adding new data here, ensure that the counterpart is updated in the ModSourceHelper mod.
    class CollateSaveData {
        public Dictionary<string, string> Projects;

        public CollateSaveData() {
            Projects = new Dictionary<string, string>();
        }
    }

    /// <summary>
    ///     Adds a projects csproj file to collate.json in ModSources
    /// </summary>
    public static void AddBuildPath(string collateJsonPath, string assemblyName, string csprojPath) {
        Console.WriteLine("Updating collate.json");
        string dir = Path.GetDirectoryName(collateJsonPath) ?? throw new DirectoryNotFoundException("Could not get directory of: " + collateJsonPath);
        Directory.CreateDirectory(dir);

        var data = File.Exists(collateJsonPath) ? JsonConvert.DeserializeObject<CollateSaveData>(File.ReadAllText(collateJsonPath)) : new CollateSaveData();

        if (data is null)
            data = new CollateSaveData();

        data.Projects[assemblyName] = csprojPath;

        foreach (var project in data.Projects) {
            if (!File.Exists(project.Value))
                data.Projects.Remove(project.Key);
        }

        File.WriteAllText(collateJsonPath, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}