﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Rejuvena.Collate.Util
{
    // TODO: This class could probably substitute my terrible boilerplate code for determining the .targets file location.
    public static class SavePathLocator
    {
        public const string STABLE_FOLDER_NAME = "tModLoader";
        public const string PREVIEW_FOLDER_NAME = "tModLoader-preview";
        public const string DEV_FOLDER_NAME = "tModLoader-dev";
        public const string MODS_FOLDER_NAME = "Mods";
        public const string TMOD_FILE_EXTENSION = ".tmod";

        public static string FindSavePath(TaskLoggingHelper log, string tmlDllPath, string assemblyName) {
            return Path.Combine(FindSaveFolder(log, tmlDllPath), MODS_FOLDER_NAME, assemblyName + TMOD_FILE_EXTENSION);
        }

        public static string FindSaveFolder(TaskLoggingHelper log, string tmlDllPath) {
            log.LogMessage(MessageImportance.Low, "Searching for valid tModLoader save path...");
            
            string tmlSteamPath = Path.GetDirectoryName(tmlDllPath) ?? throw new ArgumentException("Could not resolve directory of file: " + tmlDllPath);
            string fileFolder = GetBuildPurpose(log, tmlDllPath) switch
            {
                BuildPurpose.Stable => STABLE_FOLDER_NAME,
                BuildPurpose.Preview => PREVIEW_FOLDER_NAME,
                BuildPurpose.Dev => DEV_FOLDER_NAME
            };

            if (File.Exists(Path.Combine(tmlSteamPath, "savehere.txt"))) {
                string path = Path.Combine(tmlSteamPath, fileFolder);
                log.LogMessage(MessageImportance.Low, "Found \"savehere.txt\" at expected Steam installation path, saving to: " + path);
                return path;
            }

            string savePath = Path.Combine(GetStoragePath("Terraria"), fileFolder);
            log.LogMessage(MessageImportance.Low, "Couldn't verify expected Steam path, saving to: " + savePath);
            return savePath;
        }

        private static string GetStoragePath(string subFolder) => Path.Combine(GetStoragePath(), subFolder);

        private static string GetStoragePath() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                string? environmentVariable = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(environmentVariable))
                    return ".";
                return environmentVariable + "/Library/Application Support";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                string? text = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (!string.IsNullOrEmpty(text)) {
                    return text;
                }

                text = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(text))
                    return ".";
                return text + "/.local/share";
            }
            
            throw new PlatformNotSupportedException("Could not locate storage path for platform: " + RuntimeInformation.OSDescription);
        }

        private static BuildPurpose GetBuildPurpose(TaskLoggingHelper log, string tmlDllPath) {
            Assembly tmlAssembly = Assembly.LoadFrom(tmlDllPath);
            string? tmlInfoVersion = tmlAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            if (string.IsNullOrEmpty(tmlInfoVersion)) {
                log.LogWarning("Could not retrieve informational version from the tModLoader DLL, assuming a 'Stable' build.");
                return BuildPurpose.Stable;
            }
            
            log.LogMessage(MessageImportance.Low, "Retrieved informational version from tModLoader DLL: " + tmlInfoVersion);

            string[] parts = tmlInfoVersion[(tmlInfoVersion.IndexOf('+') + 1)..].Split('|');
            if (parts.Length >= 3) {
                if (Enum.TryParse(parts[2], true, out BuildPurpose purpose)) return purpose;
            }
            
            log.LogWarning($"Could not parse resolved build purpose \"{parts[2]}\", assuming a 'Stable' build.");
            return BuildPurpose.Stable;
        }
    }
}