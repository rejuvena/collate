using System.Collections.Generic;

namespace Rejuevna.Collate.AccessTransformer.V1
{
    public enum ReadonlyType
    {
        Inherit = 0,
        Readonly = 1,
        ReadWrite = 2
    }

    public static class ReadonlyTypeUtils
    {
        private static readonly Dictionary<string, ReadonlyType> ParseMap = new()
        {
            {"=", ReadonlyType.Inherit},
            {"+r", ReadonlyType.Readonly},
            {"-r", ReadonlyType.ReadWrite}
        };
        
        private static readonly Dictionary<ReadonlyType, string> StringMap = new()
        {
            {ReadonlyType.Inherit, "="},
            {ReadonlyType.Readonly, "+r"},
            {ReadonlyType.ReadWrite, "-r"}
        };

        public static ReadonlyType Parse(string value) {
            return ParseMap.TryGetValue(value.Trim().ToLower(), out ReadonlyType type) ? type : ReadonlyType.Inherit;
        }
        
        public static string ToString(ReadonlyType type) {
            return StringMap.TryGetValue(type, out string? value) ? value : "=";
        }
    }
}