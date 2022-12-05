using System.Collections.Generic;
using System.Linq;

namespace Rejuvena.Collate.Util;

/// <summary>
///     An abstraction for dealing with implementations of <typeparamref name="TProvider"/> that simply combine multiple instances of <typeparamref name="TProvider"/> implementations.
/// </summary>
/// <typeparam name="TProvider">The instance to inherit from and combine.</typeparam>
public abstract class CombinedProvider<TProvider>
{
    /// <summary>
    ///     The <typeparamref name="TProvider"/> instances to combine the provided results of.
    /// </summary>
    public List<TProvider> Providers { get; }

    /// <summary>
    ///     Initializes a new instance with the given <paramref name="providers"/>.
    /// </summary>
    /// <param name="providers">The <typeparamref name="TProvider"/>s to use.</param>
    protected CombinedProvider(params TProvider[] providers) {
        Providers = providers.ToList();
    }
    
    /// <summary>
    ///     Initializes a new instance with the given <paramref name="providers"/>.
    /// </summary>
    /// <param name="providers">The <typeparamref name="TProvider"/>s to use.</param>
    protected CombinedProvider(List<TProvider> providers) {
        Providers = providers;
    }
}
