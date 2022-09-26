using System;

namespace Rejuvena.Collate.Features.Packaging
{
    internal readonly record struct ModReference(string Mod, Version? TargetVersion)
    {
        public string Mod { get; } = Mod;

        public Version? TargetVersion { get; } = TargetVersion;

        public override string ToString() {
            return TargetVersion is null ? Mod : Mod + '@' + TargetVersion;
        }

        public static ModReference Parse(string value) {
            string[] split = value.Split('@');

            return split.Length switch
            {
                1 => new ModReference(split[0], null),
                2 => new ModReference(split[0], new Version(split[1])),
                _ => throw new ArgumentException("Unable to parse mod reference: " + value)
            };
        }
    }
}