using System.Collections.Generic;
using Rejuvena.Collate.Packing.Properties;
using Rejuvena.Collate.Packing.References;
using TML.Files;

namespace Rejuvena.Collate.Packing;

/// <summary>
///     Describes options for packing a mod into a .tmod file.
/// </summary>
public record PackingOptions
{
    private readonly List<(string directory, string relative)> buildDirectories = new();
    private readonly List<TModFileData>                        buildFiles       = new();

    public IReadOnlyList<(string directory, string relative)> BuildDirectories => buildDirectories.AsReadOnly();

    public IReadOnlyList<TModFileData> BuildFiles => buildFiles.AsReadOnly();

    /// <summary>
    ///     The assembly name (internal name) of the mod.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    ///     The version of tML the mod is being packed with (the one it was built against).
    /// </summary>
    public string TmlVersion { get; set; } = string.Empty;

    /// <summary>
    ///     The path to write the packed .tmod file to.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    ///     Provides various types of references <br />
    ///     <see cref="IReferencesProvider"/> instances should be added to this object.
    /// </summary>
    public ReferencesProvider References { get; } = new();

    /// <summary>
    ///     Provides mod build properties. <br />
    ///     <see cref="IPropertiesProvider"/> instances should be added to this object.
    /// </summary>
    public PropertiesProvider Properties { get; } = new();

    public PackingOptions WithReferencesProvider(IReferencesProvider provider) {
        References.Providers.Add(provider);
        return this;
    }

    public PackingOptions WithPropertiesProvider(IPropertiesProvider provider) {
        Properties.Providers.Add(provider);
        return this;
    }

    public virtual PackingOptions WithBuildDirectory(string directory, string relative) {
        buildDirectories.Add((directory, relative));
        return this;
    }

    public virtual PackingOptions WithBuildFile(TModFileData fileData) {
        buildFiles.Add(fileData);
        return this;
    }
}
