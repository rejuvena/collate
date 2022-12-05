using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rejuvena.Collate.Packing.Properties;
using Rejuvena.Collate.Util;
using TML.Files;
using TML.Files.Extraction;

namespace Rejuvena.Collate.Packing;

/// <summary>
///     Exposes an API for conveniently packing mods into .tmod files alongside Collate features.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class TModPacker
{
    public static void PackMod(PackingOptions options) {
        var props = processProperties(options.Properties);
        var modFile = new TModFile
        {
            ModLoaderVersion = options.TmlVersion,
            Name             = options.AssemblyName,
            Version          = props.Version.ToString()
        };

        // Add .dll and .pdb files.
        var modDll = new PathNamePair(options.AssemblyName + ".dll", options.OutputTmodPath);
        var modPdb = new PathNamePair(options.AssemblyName + ".pdb", options.OutputTmodPath);

        if (File.Exists(modDll.Path)) modFile.AddFile(new TModFileData(modDll.Name, File.ReadAllBytes(modDll.Path)));
        else Console.WriteLine("Could not resolve mod .dll, expected at: " + modDll.Path);

        if (File.Exists(modPdb.Path)) modFile.AddFile(new TModFileData(modPdb.Name, File.ReadAllBytes(modPdb.Path)));
        else Console.WriteLine("Could not resolve mod .pdb, expected at: " + modDll.Path);

        // Add references.
        Console.WriteLine("Adding references...");
        addReferences(modFile, props);

        // Add resources.
        Console.WriteLine("Packing project directory files (resource files) into .tmod file...");
        logBuildIgnore(props.BuildIgnores);
        TModFileExtractor.Pack(options.ProjectDirectory, options.TmlVersion, options.AssemblyName, props);

        // Write .tmod file to specified path.
        Console.WriteLine("Writing .tmod file to: " + options.OutputTmodPath);
        if (File.Exists(options.OutputTmodPath)) File.Delete(options.OutputTmodPath);
        TModFileSerializer.Serialize(modFile, options.OutputTmodPath);

        // Add to enabled.json.
        ModsFolderUtils.EnableMod(Path.Combine(Path.GetDirectoryName(options.OutputTmodPath)!, "enabled.json"), options.AssemblyName);
    }

    private static BuildProperties processProperties(IPropertiesProvider propertiesProvider) {
        var properties = propertiesProvider.GetProperties();
    }

    private static void addReferences(TModFile modFile, BuildProperties props) { }

    private static void logBuildIgnore(string[] ignores) {
        const string ignore_msg = "Ignoring files based on buildIgnore value: ";

        if (ignores.Length > 0) Console.Write(ignore_msg);
        else Console.WriteLine("No buildIgnore values were provided.");

        for (int i = 0; i < ignores.Length; i++) {
            var sb = new StringBuilder();

            sb.Append(' ', i == 0 ? 0 : ignore_msg.Length);
            sb.Append(ignores[i]);

            Console.WriteLine(sb.ToString());
        }
    }
}
