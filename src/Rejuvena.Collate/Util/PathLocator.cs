using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using TML.Files.Extraction;

namespace Rejuvena.Collate.Util;

public static class PathLocator
{
#region Save path
    public const string STABLE_FOLDER_NAME  = "tModLoader";
    public const string PREVIEW_FOLDER_NAME = "tModLoader-preview";
    public const string DEV_FOLDER_NAME     = "tModLoader-dev";
    public const string MODS_FOLDER_NAME    = "Mods";
    public const string TMOD_FILE_EXTENSION = ".tmod";

    public static string FindSavePath(string tmlDllPath, string assemblyName, out string? version) {
        return Path.Combine(findSaveFolder(tmlDllPath, out version), MODS_FOLDER_NAME, assemblyName + TMOD_FILE_EXTENSION);
    }

    private static string findSaveFolder(string tmlDllPath, out string? version) {
        string tmlSteamPath = Path.GetDirectoryName(tmlDllPath) ?? throw new ArgumentException("Could not resolve directory of file: " + tmlDllPath);
        (var buildPurpose, version) = getBuildInfo(tmlDllPath);
        string fileFolder = buildPurpose switch
        {
            BuildPurpose.Dev     => DEV_FOLDER_NAME,
            BuildPurpose.Preview => PREVIEW_FOLDER_NAME,
            BuildPurpose.Stable  => STABLE_FOLDER_NAME,
            _                    => throw new InvalidOperationException("Attempted to resolve save path for unknown build purpose: " + buildPurpose)
        };

        if (File.Exists(Path.Combine(tmlSteamPath, "savehere.txt"))) {
            string path = Path.Combine(tmlSteamPath, fileFolder);
            Console.WriteLine("Found \"savehere.txt\" at expected Steam installation path, saving to: " + path);
            return path;
        }

        string savePath = Path.Combine(getStoragePath("Terraria"), fileFolder);
        Console.WriteLine("Couldn't verify expected Steam path (this is okay!), saving to: " + savePath);
        return savePath;
    }

    private static string getStoragePath(string subFolder) {
        return Path.Combine(getStoragePath(), subFolder);
    }

    private static string getStoragePath() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            string? environmentVariable = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(environmentVariable)) return ".";

            return environmentVariable + "/Library/Application Support";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            string? text = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

            if (!string.IsNullOrEmpty(text)) {
                return text;
            }

            text = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(text)) return ".";

            return text + "/.local/share";
        }

        throw new PlatformNotSupportedException("Could not locate storage path for platform: " + RuntimeInformation.OSDescription);
    }

    private static (BuildPurpose buildPurpose, string? version) getBuildInfo(string tmlDllPath) {
        var     tmlAsm         = ModuleDefinition.ReadModule(tmlDllPath);
        var     attrs          = tmlAsm?.Assembly.CustomAttributes;
        var     attr           = attrs?.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyInformationalVersionAttribute));
        string? tmlInfoVersion = attr?.ConstructorArguments[0].Value as string;

        if (string.IsNullOrEmpty(tmlInfoVersion)) {
            Console.WriteLine("Could not retrieve informational version from the tModLoader DLL, assuming a 'Stable' build.");
            return (BuildPurpose.Stable, null);
        }

        Console.WriteLine("Retrieved informational version from tModLoader DLL: " + tmlInfoVersion);

        string[] parts = tmlInfoVersion.Split('|');

        BuildPurpose? purpose = null;
        string?       version = null;

        foreach (string part in parts) {
            // Parse build purpose, f.e.: stable, dev, preview.
            if (Enum.TryParse(part, true, out BuildPurpose p)) purpose ??= p;

            // Parse version, f.e.: 1.x.x.x+20xx.xx.xx.xx.
            if (part.Contains('+')) version ??= part.Split('+', 2)[1];
        }

        if (purpose is null) Console.WriteLine($"Could not parse resolved build purpose \"{parts[2]}\", assuming a 'Stable' build.");
        if (version is null) Console.WriteLine("Could not parse resolved tModLoader version, defaulting to 'null'.");

        return (purpose ?? BuildPurpose.Stable, version);
    }
#endregion
}
