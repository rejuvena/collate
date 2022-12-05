using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Rejuvena.Collate.Packing.Properties;

namespace Rejuvena.Collate.CLI.Commands;

[Command(COMMAND_NAME, Description = "Packages a mod into a .tmod file.")]
public sealed class PackageModCommand : VersionSensitiveCommand, IPropertiesProvider
{
    public const string COMMAND_NAME = "pack";

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

    public string AssemblyReferences { get; set; }

    public string NuGetReferences { get; set; }

    public string ModReferences { get; set; }

    /// <summary>
    ///     The project directory.
    /// </summary>
    [CommandOption("proj-dir", 'p', IsRequired = true, Description = "The project directory.")]
    public string ProjectDirectory { get; set; } = string.Empty;
    
    public string ProjectOutputDirectory { get; set; }

    public string AssemblyName { get; set; }

    public string TmlVersion { get; set; }

    public string TmlPath { get; set; }

    public string OutputTmodPath { get; set; }

    // asm-refs
    // nuget-refs
    // mod-refs
    // proj-out-dir ProjectOutputPath
    // asm-name AssemblyName
    // tml-ver TmlVersion
    // tml-path TmlPath
    // out-dir OutputTmodPath

    protected override async ValueTask ExecuteAsync(IConsole console, Version version) { }

    public Dictionary<string, string> GetProperties() {
        var properties = new Dictionary<string, string>();

        void includeIfNotNull(string key, string? value) {
            if (value is not null) properties.Add(key, value);
        }
        
        includeIfNotNull("displayName", DisplayName);
        includeIfNotNull("author", Author);
        includeIfNotNull("modVersion", ModVersion);
        includeIfNotNull("homepage", Homepage);
        includeIfNotNull("side", ModSide);
        includeIfNotNull("sortBefore", SortBefore);
        includeIfNotNull("sortAfter", SortAfter);
        includeIfNotNull("hideCode", HideCode.ToString());
        includeIfNotNull("hideResources", HideResources.ToString());
        includeIfNotNull("includeSource", IncludeSource.ToString());
        includeIfNotNull("buildIgnore", BuildIgnore);

        return properties;
    }
}
