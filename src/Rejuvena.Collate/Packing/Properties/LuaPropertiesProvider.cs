using System.Collections.Generic;
using NLua;

namespace Rejuvena.Collate.Packing.Properties;

internal class LuaPropertiesProvider : IPropertiesProvider
{
    private readonly string filePath;

    public LuaPropertiesProvider(string filePath) {
        this.filePath = filePath;
    }

    Dictionary<string, string> IPropertiesProvider.GetProperties() {
        var properties = new Dictionary<string, string>();
        var lua = new Lua(); // New lua state
        var tbl = lua.DoFile(filePath)[0] as LuaTable; // Have state execute the file, which should return a LuaTable.
        var exports = lua.GetTableDict(tbl); // Convert LuaTable to Dictionary<object, object>

        foreach (var kvp in exports) properties[kvp.Key.ToString()!] = kvp.Value.ToString()!;

        return properties;
    }
}