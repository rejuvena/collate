using Mono.Cecil;

namespace Rejuvena.Collate.Util.Cecil
{
    internal static class ModuleFactory
    {
        public static ModuleDefinition CreateModuleFromFile(string path, IAssemblyResolver? assemblyResolver = null) {
            return ModuleDefinition.ReadModule(path, new ReaderParameters {AssemblyResolver = assemblyResolver});
        }
    }
}