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

    public static string FindSavePath(string tmlDllPath, string assemblyName) {
        return Path.Combine(findSaveFolder(tmlDllPath), MODS_FOLDER_NAME, assemblyName + TMOD_FILE_EXTENSION);
    }

    private static string findSaveFolder(string tmlDllPath) {
        string tmlSteamPath = Path.GetDirectoryName(tmlDllPath) ?? throw new ArgumentException("Could not resolve directory of file: " + tmlDllPath);
        string fileFolder = getBuildPurpose(tmlDllPath) switch
        {
            BuildPurpose.Dev     => DEV_FOLDER_NAME,
            BuildPurpose.Preview => PREVIEW_FOLDER_NAME,
            BuildPurpose.Stable  => STABLE_FOLDER_NAME,
            _                    => throw new ArgumentOutOfRangeException()
        };

        if (File.Exists(Path.Combine(tmlSteamPath, "savehere.txt"))) {
            string path = Path.Combine(tmlSteamPath, fileFolder);
            Console.WriteLine("Found \"savehere.txt\" at expected Steam installation path, saving to: " + path);
            return path;
        }

        string savePath = Path.Combine(getStoragePath("Terraria"), fileFolder);
        Console.WriteLine("Couldn't verify expected Steam path, saving to: " + savePath);
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

    private static BuildPurpose getBuildPurpose(string tmlDllPath) {
        var     tmlAsm         = ModuleDefinition.ReadModule(tmlDllPath);
        var     attrs          = tmlAsm.CustomAttributes;
        var     attr           = attrs.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyInformationalVersionAttribute));
        string? tmlInfoVersion = attr?.ConstructorArguments[0].Value as string;

        if (string.IsNullOrEmpty(tmlInfoVersion)) {
            Console.WriteLine("Could not retrieve informational version from the tModLoader DLL, assuming a 'Stable' build.");
            return BuildPurpose.Stable;
        }

        Console.WriteLine("Retrieved informational version from tModLoader DLL: " + tmlInfoVersion);

        string[] parts = tmlInfoVersion!.Substring(tmlInfoVersion.IndexOf('+') + 1).Split('|');
        if (parts.Length >= 3 && Enum.TryParse(parts[2], true, out BuildPurpose purpose)) return purpose;

        // tML Preview build type is 4th element for some reason
        if (parts.Length >= 4 && Enum.TryParse(parts[3], true, out purpose)) return purpose;

        Console.WriteLine($"Could not parse resolved build purpose \"{parts[2]}\", assuming a 'Stable' build.");
        return BuildPurpose.Stable;
    }
#endregion
}
