using System.Collections.Generic;

namespace Rejuvena.Collate.Packing.References;

/// <summary>
///     Providers references (mod references, assembly references, package references).
/// </summary>
public interface IReferencesProvider
{
    /// <summary>
    ///     Provides a collection of all mod references.
    /// </summary>
    /// <returns>A collection of <see cref="ModReference"/>s.</returns>
    IEnumerable<ModReference> GetModReferences();
    
    /// <summary>
    ///     Provides a collection of all assembly references.
    /// </summary>
    /// <returns>A collection of local paths (local to the <c>./lib/</c> folder of a mod.</returns>
    IEnumerable<AssemblyReference> GetAssemblyReferences();
    
    /// <summary>
    ///     Provides a collection of NuGet package references.
    /// </summary>
    /// <returns>A collection of NuGet package paths.</returns>
    IEnumerable<NuGetReference> GetPackageReferences();
}
