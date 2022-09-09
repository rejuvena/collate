using System;
using System.Collections.Generic;
using System.Linq;

namespace Rejuevna.Collate.AccessTransformer.V1
{
    public enum AccessorType
    {
        Inherit = 0,
        Public = 1,
        Private = 2,
        Internal = 3,
        Protected = 4,
        ProtectedInternal = 5,
        PrivateProtected = 6
    }

    public static class AccessorTypeUtils
    {
        private static readonly Dictionary<AccessorType, bool> NoStatic = new()
        {
            {AccessorType.Inherit, false},
            {AccessorType.Public, false},
            {AccessorType.Private, false},
            {AccessorType.Internal, false},
            {AccessorType.Protected, true},
            {AccessorType.ProtectedInternal, true},
            {AccessorType.PrivateProtected, true}
        };

        private static readonly Dictionary<string, AccessorType> ParseMap = new()
        {
            {"=", AccessorType.Inherit},
            {"public", AccessorType.Public},
            {"private", AccessorType.Private},
            {"internal", AccessorType.Internal},
            {"protected", AccessorType.Protected},
            {"protected-internal", AccessorType.ProtectedInternal},
            {"private-protected", AccessorType.PrivateProtected}
        };

        private static readonly Dictionary<AccessorType, string> StringMap = new()
        {
            {AccessorType.Inherit, "="},
            {AccessorType.Public, "public"},
            {AccessorType.Private, "private"},
            {AccessorType.Internal, "internal"},
            {AccessorType.Protected, "protected"},
            {AccessorType.ProtectedInternal, "protected-internal"},
            {AccessorType.PrivateProtected, "private-protected"}
        };

        public static bool AllowsStatic(this AccessorType accessorType) {
            return NoStatic[accessorType];
        }

        public static AccessorType Parse(string value) {
            return ParseMap.TryGetValue(value.Trim().ToLower().Replace(' ', '-'), out AccessorType type) ? type : AccessorType.Inherit;
        }

        public static string ToString(AccessorType type) {
            return StringMap.TryGetValue(type, out string? value) ? value : "=";
        }
    }
}