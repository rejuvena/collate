using System;
using System.IO;

namespace Rejuvena.Collate.Packing.Properties;

/// <summary>
///     Utility class for automatically creating <see cref="IPropertiesProvider"/> instances from files. <br />
///     Doubles as API integrity insurance in the event that a would-be breaking change is made by hiding away direct access to specified <see cref="IPropertiesProvider"/> implementations.
/// </summary>
public static class PropertiesProviderFactory
{
    public static IPropertiesProvider CreateProvider(string filePath) {
        if (!File.Exists(filePath)) throw new FileNotFoundException("Attempted to create a properties provider for file that does not exist: " + filePath);

        // TODO: Support .json, .lua, .js, .yaml, .toml, etc.
        // csproj support will be part of Rejuvena.Collate.MSBuild
        return Path.GetExtension(filePath) switch
        {
            ".txt" => new TextPropertiesProvider(filePath),
            _      => throw new Exception("Attempted to create a properties provider for file with unsupported extension: " + filePath)
        };
    }
}
