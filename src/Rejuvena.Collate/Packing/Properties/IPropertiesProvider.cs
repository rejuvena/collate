using System.Collections.Generic;

namespace Rejuvena.Collate.Packing.Properties;

/// <summary>
///     Provides mod properties (typically defined in the `build.txt` file).
/// </summary>
public interface IPropertiesProvider
{
    /// <summary>
    ///     Retrieves the properties provided by this object.
    /// </summary>
    /// <returns>A collection of KVPs in a dictionary representing the properties as keys and their values.</returns>
    Dictionary<string, string> GetProperties();
}
