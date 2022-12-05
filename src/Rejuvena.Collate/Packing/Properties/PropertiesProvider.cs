using System.Collections.Generic;
using Rejuvena.Collate.Util;

namespace Rejuvena.Collate.Packing.Properties;

/// <summary>
///     An implementation of <see cref="IPropertiesProvider"/> which combines the properties provided by the given <see cref="CombinedProvider{TProvider}.Providers"/>.
/// </summary>
public sealed class PropertiesProvider : CombinedProvider<IPropertiesProvider>, IPropertiesProvider
{
    /// <summary>
    ///     Returns a dictionary containing the combined results of all given <see cref="CombinedProvider{TProvider}.Providers"/>.
    /// </summary>
    /// <returns>A collection of KVPs in a dictionary representing the properties as keys and their values.</returns>
    public Dictionary<string, string> GetProperties() {
        var properties = new Dictionary<string, string>();
        
        foreach (var provider in Providers) {
            foreach ((string key, string value) in provider.GetProperties()) {
                properties[key] = value;
            }
        }

        return properties;
    }
}
