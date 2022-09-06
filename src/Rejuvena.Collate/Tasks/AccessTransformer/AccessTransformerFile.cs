using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Rejuvena.Collate.Tasks.AcessTransformer
{
    public class AccessTransformerFile
    {
        public virtual int Version { get; }

        public virtual Transformer[] Items { get; }

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
            string[] lines = File.ReadAllLines(path);
            string versionLine = lines[1].Trim();
            if (!versionLine.StartsWith('v')) throw new InvalidOperationException("Tried to parse file without specified version.");

            return new AccessTransformerFile(
                int.Parse(versionLine[1..]),
                lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('v') && !line.StartsWith('#')).Select(Transformer.Parse).ToArray()
            );
        }

        public virtual void Write(StreamWriter writer) {
            Write(this, writer);
        }

        public virtual void WriteFile(string path) {
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