using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Rejuvena.Collate.Packing.Properties;

internal class JsonPropertiesProvider : IPropertiesProvider
{
    private readonly string filePath;

    public JsonPropertiesProvider(string filePath) {
        this.filePath = filePath;
    }

    Dictionary<string, string> IPropertiesProvider.GetProperties() {
        var properties = new Dictionary<string, string>();
        var reader = new JsonTextReader(new StringReader(File.ReadAllText(filePath)));

        string index = "";
        while (reader.Read()) {
            if (reader.Value != null) {
                if (reader.TokenType.ToString() == "PropertyName")
                    index = (string)reader.Value;
                else
                    properties[index] = reader.Value.ToString()!;
            }
        }

        return properties;
    }
}