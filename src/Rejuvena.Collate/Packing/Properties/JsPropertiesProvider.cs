using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jint.CommonJS;
using Rejuvena.Collate.Util;

namespace Rejuvena.Collate.Packing.Properties;

internal class JsPropertiesProvider : IPropertiesProvider
{
    private readonly string filePath;

    public JsPropertiesProvider(string filePath) {
        this.filePath = filePath;
    }
    /*
     *     Dictionary<string, string> IPropertiesProvider.GetProperties() {
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
     */

    Dictionary<string, string> IPropertiesProvider.GetProperties() {
        var properties = new Dictionary<string, string>();
        var exports    = new Jint.Engine().CommonJS().RunMain(filePath);

        foreach (var kvp in exports.AsObject().GetOwnProperties()) properties[kvp.Key] = kvp.Value.Value.ToString();

        return properties;
    }
}
