using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rejuvena.Collate.Packing.Properties;

/// <summary>
///     <see cref="IPropertiesProvider"/> implementation 
/// </summary>
internal sealed class TextPropertiesProvider : IPropertiesProvider
{
    private readonly string filePath;

    public TextPropertiesProvider(string filePath) {
        this.filePath = filePath;
    }

    Dictionary<string, string> IPropertiesProvider.GetProperties() {
        var      properties = new Dictionary<string, string>();
        string[] lines      = File.ReadAllLines(filePath).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith('#')).ToArray();

        foreach (string line in lines) {
            int    splitIndex = line.IndexOf('=');
            string key        = line[..splitIndex].Trim();
            string value      = line[(splitIndex + 1)..].Trim();

            if (!string.IsNullOrEmpty(value)) properties[key] = value;
        }

        return properties;
    }
}
