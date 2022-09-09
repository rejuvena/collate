using TML.Files.Abstractions;

namespace Rejuvena.Collate.TML
{
    internal sealed class CollateModEntry : IModFileEntry
    {
        public string Name { get; }

        public int Offset { get; }

        public int Length { get; }

        public int CompressedLength { get; }

        public byte[]? CachedBytes { get; }

        public CollateModEntry(string name, int offset, int length, int compressedLength, byte[]? cachedBytes) {
            Name = name;
            Offset = offset;
            Length = length;
            CompressedLength = compressedLength;
            CachedBytes = cachedBytes;
        }
    }
}