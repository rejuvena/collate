using Rejuvena.Collate.Packing.Properties;
using Rejuvena.Collate.Packing.References;

namespace Rejuvena.Collate.Packing;

/// <summary>
///     Describes options for packing a mod into a .tmod file.
/// </summary>
public record PackingOptions
{
    /// <summary>
    ///     The directory of the project.
    /// </summary>
    public string ProjectDirectory { get; set; } = string.Empty;

    /// <summary>
    ///     The path containing the compiled mod assembly.
    /// </summary>
    public string ProjectBuildDirectory { get; set; } = string.Empty;

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
    public string OutputTmodPath { get; set; } = string.Empty;

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
}
