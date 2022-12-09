using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Rejuvena.Collate.Packing.Properties;

internal class YamlPropertiesProvider : IPropertiesProvider
{
    private readonly string filePath;

    public YamlPropertiesProvider(string filePath) {
        this.filePath = filePath;
    }

    Dictionary<string, string> IPropertiesProvider.GetProperties() {
        var properties = new Dictionary<string, string>();
        var yaml = new YamlStream();
        yaml.Load(new StringReader(File.ReadAllText(filePath)));

        var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

        foreach (var entry in mapping.Children) properties[entry.Key.ToString()] = entry.Value.ToString();

        return properties;
    }
}