using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Rejuvena.Collate.Cecil.AT
{
    internal sealed class AccessTransformerFile
    {
        public readonly int Version;
        public readonly Transformer[] Items;

        public AccessTransformerFile(int version, params Transformer[] items) {
            Version = version;
            Items = items;
        }

        public override string ToString() {
            StringBuilder sb = new();
            foreach (Transformer item in Items) sb.AppendLine(item.ToString());
            return sb.ToString();
        }

        public static AccessTransformerFile ReadFile(string path) {
            string[] lines = File.ReadAllLines(path).Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#")).Select(x => x.Trim()).ToArray();
            string versionLine = lines[0];
            if (!versionLine.StartsWith('v')) throw new InvalidOperationException("Tried to parse file without specified version.");

            return new AccessTransformerFile(
                int.Parse(versionLine[1..]),
                lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('v')).Select(Transformer.Parse).ToArray()
            );
        }

        public void Write(StreamWriter writer) {
            Write(this, writer);
        }

        public void WriteFile(string path) {
            WriteFile(this, path);
        }

        public static void Write(AccessTransformerFile file, StreamWriter writer) {
            writer.Write(file.ToString());
        }

        public static void WriteFile(AccessTransformerFile file, string filePath) {
            File.WriteAllText(filePath, file.ToString());
        }
    }
}