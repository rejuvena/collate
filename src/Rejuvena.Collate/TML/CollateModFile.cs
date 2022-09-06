using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Rejuvena.Collate.Util;
using TML.Files.Abstractions;

namespace Rejuvena.Collate.TML
{
    public class CollateModFile : IModFile
    {
        public const uint MINIMUM_COMPRESSION_SIZE = 1 << 10; // 1kb
        public const float COMPRESSION_TRADEOFF = 0.9f;
        public const string HEADER = "TMOD";

        public string Header => HEADER;

        public string ModLoaderVersion { get; }

        public byte[] Hash { get; } = Array.Empty<byte>();

        public byte[] Signature { get; } = Array.Empty<byte>();

        public string Name { get; }

        public string Version { get; }

        public IEnumerable<IModFileEntry> Files { get; } = new List<IModFileEntry>();

        public CollateModFile(string modLoaderVersion, string name, string version) {
            ModLoaderVersion = modLoaderVersion;
            Name = name;
            Version = version;
        }

        public void AddFile(string fileName, byte[] data) {
            fileName = fileName.Replace('\\', '/');

            int size = data.Length;
            if (size > MINIMUM_COMPRESSION_SIZE && ShouldCompress(fileName)) {
                using MemoryStream memStream = new(data.Length);
                using (DeflateStream compressStream = new(memStream, CompressionMode.Compress)) compressStream.Write(data, 0, data.Length);

                byte[] compressed = memStream.ToArray();
                if (compressed.Length < size * COMPRESSION_TRADEOFF) data = compressed;
            }

            ((List<IModFileEntry>) Files).Add(new CollateModEntry(fileName, -1, size, data.Length, data));
        }

        public static bool ShouldCompress(string fileName) {
            return !new[] {".png", ".mp3", ".ogg", ".rawimg"}.Contains(Path.GetExtension(fileName));
        }
    }

    public static class CollateModFileExtensions
    {
        public static void AddFileFromPath(this CollateModFile modFile, string fileName, string path, Action? onSuccess = null, Action? onError = null) {
            if (File.Exists(path)) {
                modFile.AddFile(fileName, File.ReadAllBytes(path));
                onSuccess?.Invoke();
            }
            else
                onError?.Invoke();
        }

        public static void AddFileFromPath(this CollateModFile modFile, PathNamePair pnPair, Action? onSuccess = null, Action? onError = null) {
            modFile.AddFileFromPath(pnPair.Name, pnPair.Path, onSuccess, onError);
        }
    }
}