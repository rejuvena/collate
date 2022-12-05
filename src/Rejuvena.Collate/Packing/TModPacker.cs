using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rejuvena.Collate.Packing.Properties;
using Rejuvena.Collate.Packing.References;
using Rejuvena.Collate.Util;
using TML.Files;
using TML.Files.Extraction;
using ModReference = Rejuvena.Collate.Packing.References.ModReference;

namespace Rejuvena.Collate.Packing;

/// <summary>
///     Exposes an API for conveniently packing mods into .tmod files alongside Collate features.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class TModPacker
{
    // Common packages modders may add that should be private but might not be - this is us correcting their errors.
    private static readonly List<string> package_blacklist = new()
    {
        "Rejuvena.Collate.MSBuild",
        "tModLoader.CodeAssist",
    };

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
        addReferences(modFile, options, props);

        // Add resources.
        Console.WriteLine("Packing project directory files (resource files) into .tmod file...");
        prettyLogCollection(props.BuildIgnores, "Ignoring files based on buildIgnore value: ", "No buildIgnore values were provided.");
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
        var props      = new BuildProperties();

        foreach ((string key, string value) in properties) BuildProperties.ProcessProperty(props, key, value);

        return props;
    }

    private static void addReferences(TModFile modFile, PackingOptions options, BuildProperties props) {
        var modRefs = filterModRefs(options.References.GetModReferences().ToList()).ToList();
        var asmRefs = filterAsmRefs(options.ProjectDirectory, options.References.GetAssemblyReferences().ToList()).ToList();
        var pkgRefs = filterPkgRefs(options.References.GetPackageReferences().ToList()).ToList();

        prettyLogCollection(modRefs, $"Resolved {modRefs.Count} mod references: ", "Resolved 0 mod references.");

        foreach (var modRef in modRefs) {
            Console.WriteLine($"Adding mod reference \"{modRef.Identity}\" [Weak: {modRef.Weak}]...");
            props.AddModReference(modRef.Identity, modRef.Weak);
        }

        prettyLogCollection(asmRefs, $"Resolved {modRefs.Count} assembly references: ", "Resolved 0 assembly references.");

        foreach (var asmRef in asmRefs) {
            Console.WriteLine($"Adding assembly reference at path \"{asmRef.Path}\" [Private: {asmRef.Private}]...");

            if (asmRef.Private) {
                Console.WriteLine("Skipping private assembly reference...");
                continue;
            }

            modFile.AddFile(new TModFileData($"lib/{asmRef.Name}.dll", File.ReadAllBytes(asmRef.Path)));
            props.AddDllReference(asmRef.Name);
        }

        prettyLogCollection(pkgRefs, $"Resolved {modRefs.Count} package references: ", "Resolved 0 package references.");

        foreach (var pkgRef in pkgRefs) {
            Console.WriteLine($"Adding package reference \"{pkgRef.PackageId}\" at path \"{pkgRef.Path}\" [Private: {pkgRef.Private}]...");

            if (pkgRef.Private) {
                Console.WriteLine("Skipping private package reference...");
                continue;
            }

            // TODO: We need considerably more in-depth support here.
            modFile.AddFile(new TModFileData($"lib/{pkgRef.PackageId}.dll", File.ReadAllBytes(pkgRef.Path)));
            props.AddDllReference(pkgRef.PackageId);
        }
    }

    private static IEnumerable<ModReference> filterModRefs(IEnumerable<ModReference> refs) {
        return refs;
    }

    private static IEnumerable<AssemblyReference> filterAsmRefs(string projectDirectory, IEnumerable<AssemblyReference> refs) {
        // Assumes all DLL references are under the mod's folder (at same level or in sub-folders).
        // Letting DLL references be anywhere would mean doing some weird filters on references, TODO: explore?
        // or using a custom <DllReference> tag that would get translated to a <Reference>.
        return refs.Where(x => x.Path.StartsWith(projectDirectory) && !x.Path.Contains(".collate"));
    }

    private static IEnumerable<NuGetReference> filterPkgRefs(IEnumerable<NuGetReference> refs) {
        return refs.Where(x => x.Private).Where(x => !package_blacklist.Contains(x.PackageId));
    }

    private static void prettyLogCollection<T>(IEnumerable<T> ignores, string nonEmptyMessage, string emptyMessage) {
        var enumerated = ignores.ToArray();

        if (enumerated.Length > 0) Console.Write(nonEmptyMessage);
        else Console.WriteLine(emptyMessage);

        for (int i = 0; i < enumerated.Length; i++) {
            var sb = new StringBuilder();

            sb.Append(' ', i == 0 ? 0 : nonEmptyMessage.Length);
            sb.Append(enumerated[i]);

            Console.WriteLine(sb.ToString());
        }
    }
}
