using System.IO;
using Mono.Cecil;

namespace Rejuevna.Collate.AccessTransformer
{
    public interface IATFile
    {
        int Version { get; }

        bool Transform(ModuleDefinition module);

        void Write(StreamWriter writer);

        void WriteFile(string path);
    }
}