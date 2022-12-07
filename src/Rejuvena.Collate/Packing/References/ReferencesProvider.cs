using System;
using System.Collections.Generic;
using Rejuvena.Collate.Util;

namespace Rejuvena.Collate.Packing.References;

/// <summary>
///     An implementation of <see cref="IReferencesProvider"/> which combines the references provided by the given <see cref="CombinedProvider{TProvider}.Providers"/>.
/// </summary>
public class ReferencesProvider : CombinedProvider<IReferencesProvider>, IReferencesProvider
{
    public IEnumerable<ModReference> GetModReferences() {
        return hashUnion(provider => provider.GetModReferences());
    }

    public IEnumerable<AssemblyReference> GetAssemblyReferences() {
        return hashUnion(provider => provider.GetAssemblyReferences());
    }

    public IEnumerable<NuGetReference> GetPackageReferences() {
        // TODO: Handle versions here as well.
        return hashUnion(provider => provider.GetPackageReferences());
    }

    private IEnumerable<T> hashUnion<T>(Func<IReferencesProvider, IEnumerable<T>> func) {
        var refs = new HashSet<T>();

        foreach (var provider in Providers) refs.UnionWith(func(provider));

        return refs;
    }
}
