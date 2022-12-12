using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Rejuvena.Collate.Packing;
using Rejuvena.Collate.Packing.Properties;
using Rejuvena.Collate.Packing.References;
using Rejuvena.Collate.Util;
using TML.Files;

namespace Rejuvena.Collate.CLI.Commands;

[Command(COMMAND_NAME, Description = "Packages a mod into a .tmod file.")]
public sealed class PackageModCommand : VersionSensitiveCommand, IPropertiesProvider, IReferencesProvider
{
    public const string COMMAND_NAME = "package";

    protected override string CommandName => COMMAND_NAME;

#region Build properties
    [CommandOption("display-name", Description = "The display name of the mod.")]
    public string? DisplayName { get; set; } = null;

    [CommandOption("author", Description = "The author(s) of the mod.")]
    public string? Author { get; set; } = null;

    [CommandOption("mod-version", Description = "The version of the mod.")]
    public string? ModVersion { get; set; } = null;

    [CommandOption("homepage", Description = "The homepage of the mod.")]
    public string? Homepage { get; set; } = null;

    [CommandOption("mod-side", Description = "The client-server side the mod should be loaded on (Both, Client, Server, NoSync.")]
    public string? ModSide { get; set; } = null;

    [CommandOption("sort-before", Description = "What mods should be loaded before this mod.")]
    public string? SortBefore { get; set; } = null;

    [CommandOption("sort-after", Description = "What mods should be loaded after this mod.")]
    public string? SortAfter { get; set; } = null;

    [CommandOption("hide-code", Description = "Whether the DLL of this mod should be hidden.")]
    public bool? HideCode { get; set; } = null;

    [CommandOption("hide-resources", Description = "Whether the resources of this mod should be hidden.")]
    public bool? HideResources { get; set; } = null;

    [CommandOption("include-source", Description = "Whether additional source files of this mod should be hidden.")]
    public bool? IncludeSource { get; set; } = null;

    [CommandOption("build-ignore", Description = "Additional, finer control over what files should be ignored.")]
    public string? BuildIgnore { get; set; } = null;
#endregion

    [CommandOption("asmrefs-path")]
    public string AsmRefsPath { get; set; }

    [CommandOption("nugetrefs-path")]
    public string NuGetRefsPath { get; set; }

    [CommandOption("modrefs-path")]
    public string ModRefsPath { get; set; }

    [CommandOption("proj-name")]
    public string ProjectName { get; set; }

    /// <summary>
    ///     The project directory.
    /// </summary>
    [CommandOption("proj-dir", 'p', IsRequired = true, Description = "The project directory.")]
    public string ProjectDirectory { get; set; } = string.Empty;

    [CommandOption("proj-out-dir")]
    public string ProjectOutputDirectory { get; set; }

    [CommandOption("asm-name")]
    public string AssemblyName { get; set; }

    [CommandOption("tml-ver")]
    public string TmlVersion { get; set; }

    [CommandOption("tml-path")]
    public string TmlPath { get; set; }

    [CommandOption("out-dir")]
    public string OutputTmodPath { get; set; }

    [CommandOption("prop-prov-paths")]
    public string PropertiesProviderPaths { get; set; }

    protected override async ValueTask ExecuteAsync(IConsole console, Version version) {
        string? tempVers = null;

        if (string.IsNullOrEmpty(OutputTmodPath)) OutputTmodPath = PathLocator.FindSavePath(TmlPath, AssemblyName, out tempVers);

        if (string.IsNullOrEmpty(TmlVersion) && tempVers is not null) TmlVersion = tempVers;

        var options = new PackingOptions
                      {
                          AssemblyName = AssemblyName,
                          TmlVersion   = TmlVersion,
                          OutputPath   = OutputTmodPath,
                      }
                      .WithReferencesProvider(this)
                      .WithPropertiesProvider(this)
                      .WithBuildDirectory(ProjectDirectory, "");

        foreach (string propProvider in PropertiesProviderPaths.Split(';')) {
            if (string.IsNullOrEmpty(propProvider)) continue;

            await console.Output.WriteLineAsync("Using properties provider at path: " + propProvider);
            options.WithPropertiesProvider(PropertiesProviderFactory.CreateProvider(propProvider));
        }

        // Add .dll and .pdb files. ProjectOutputDirectory
        var modDll = new PathNamePair(AssemblyName + ".dll", ProjectOutputDirectory);
        var modPdb = new PathNamePair(AssemblyName + ".pdb", ProjectOutputDirectory);

        if (File.Exists(modDll.Path)) options.WithBuildFile(new TModFileData(modDll.Name, await File.ReadAllBytesAsync(modDll.Path)));
        else await console.Output.WriteLineAsync("Could not resolve mod .dll, expected at: " + modDll.Path);

        if (File.Exists(modPdb.Path)) options.WithBuildFile(new TModFileData(modPdb.Name, await File.ReadAllBytesAsync(modPdb.Path)));
        else await console.Output.WriteLineAsync("Could not resolve mod .pdb, expected at: " + modDll.Path);

        TModPacker.PackMod(options);

        ModsFolderUtils.AddBuildPath(Path.Combine(Path.GetDirectoryName(OutputTmodPath)!, "collate.json"), AssemblyName, Path.Combine(ProjectDirectory, ProjectName) + ".csproj");
    }

    protected override async ValueTask ExecuteDebugAsync(IConsole console, Version version) {
        console.ForegroundColor = ConsoleColor.DarkGray;
        await console.Output.WriteLineAsync();

        await base.ExecuteDebugAsync(console, version);

        await console.Output.WriteLineAsync("Options:");
        await console.Output.WriteLineAsync($"  {nameof(AsmRefsPath)}: {AsmRefsPath}");
        await console.Output.WriteLineAsync($"  {nameof(NuGetRefsPath)}: {NuGetRefsPath}");
        await console.Output.WriteLineAsync($"  {nameof(ModRefsPath)}: {ModRefsPath}");
        await console.Output.WriteLineAsync($"  {nameof(ProjectDirectory)}: {ProjectDirectory}");
        await console.Output.WriteLineAsync($"  {nameof(ProjectOutputDirectory)}: {ProjectOutputDirectory}");
        await console.Output.WriteLineAsync($"  {nameof(AssemblyName)}: {AssemblyName}");
        await console.Output.WriteLineAsync($"  {nameof(TmlPath)}: {TmlPath}");
        await console.Output.WriteLineAsync($"  {nameof(OutputTmodPath)}: {OutputTmodPath}");
        await console.Output.WriteLineAsync($"  {nameof(PropertiesProviderPaths)}: {PropertiesProviderPaths}");

        await console.Output.WriteLineAsync("Properties:");
        foreach ((string key, string value) in GetProperties()) await console.Output.WriteLineAsync($"  {key}: {value}");

        await console.Output.WriteLineAsync();
        console.ResetColor();
    }

    public Dictionary<string, string> GetProperties() {
        var properties = new Dictionary<string, string>();

        void includeIfNotNull(string key, string? value) {
            if (value is not null) properties.Add(key, value);
        }

        includeIfNotNull("displayName",   DisplayName);
        includeIfNotNull("author",        Author);
        includeIfNotNull("modVersion",    ModVersion);
        includeIfNotNull("homepage",      Homepage);
        includeIfNotNull("side",          ModSide);
        includeIfNotNull("sortBefore",    SortBefore);
        includeIfNotNull("sortAfter",     SortAfter);
        includeIfNotNull("hideCode",      HideCode.ToString());
        includeIfNotNull("hideResources", HideResources.ToString());
        includeIfNotNull("includeSource", IncludeSource.ToString());
        includeIfNotNull("buildIgnore",   BuildIgnore);

        return properties;
    }

    public IEnumerable<ModReference> GetModReferences() {
        string   text  = File.ReadAllText(ModRefsPath).Trim();
        string[] lines = text.Split('\n');

        foreach (string line in lines) {
            string[] parts = line.Split(';', 3);
            if (parts.Length != 3) continue;

            yield return new ModReference(parts[0], parts[1], !string.IsNullOrEmpty(parts[2]) && bool.Parse(parts[2]));
        }
    }

    public IEnumerable<AssemblyReference> GetAssemblyReferences() {
        string   text  = File.ReadAllText(AsmRefsPath).Trim();
        string[] lines = text.Split('\n');

        foreach (string line in lines) {
            string[] parts = line.Split(';', 2);
            if (parts.Length != 2) continue;

            yield return new AssemblyReference(Path.GetFileNameWithoutExtension(parts[0]), parts[0], !string.IsNullOrEmpty(parts[1]) && bool.Parse(parts[1]));
        }
    }

    public IEnumerable<NuGetReference> GetPackageReferences() {
        string   text  = File.ReadAllText(NuGetRefsPath).Trim();
        string[] lines = text.Split('\n');

        foreach (string line in lines) {
            string[] parts = line.Split(';', 3);
            if (parts.Length != 3) continue;

            yield return new NuGetReference(parts[0], parts[1], !string.IsNullOrEmpty(parts[2]) && bool.Parse(parts[2]));
        }
    }
}