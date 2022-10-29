using System;
using System.IO;

namespace Rejuevna.Collate.AccessTransformer
{
    public static class AtFileFactory
    {
        public static IAtFile CreateFromFile(string path) {
            string[] lines = File.ReadAllLines(path);
            string versionLine = lines[0];

            if (!versionLine.StartsWith("v")) throw new InvalidOperationException("Tried to parse file without specified version.");

            return int.Parse(versionLine.Substring(1)) switch
            {
                1 => V1.AtFile.Read(path),
                _ => throw new InvalidOperationException("Tried to parse file with unsupported version.")
            };
        }
    }
}