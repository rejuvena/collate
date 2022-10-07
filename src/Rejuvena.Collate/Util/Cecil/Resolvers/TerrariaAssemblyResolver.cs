using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace Rejuvena.Collate.Util.Cecil.Resolvers
{
    internal sealed class TerrariaAssemblyResolver : BaseAssemblyResolver
    {
        private readonly DefaultAssemblyResolver DefaultResolver = new();
        private readonly List<AssemblyDefinition> Libraries = new();

        public TerrariaAssemblyResolver(TaskLoggingHelper log, string rootDirectory) {
            foreach (string dll in Directory.GetFiles(rootDirectory, "*.dll", SearchOption.AllDirectories)) {
                // Ignore misc. DLLs that we don't care about.
                if (dll.Contains("runtimes") || dll.Contains("resources") || dll.Contains("Native")) continue;

                // Add all usable libraries to a collection that we use to resolve from later.
                try {
                    Libraries.Add(AssemblyDefinition.ReadAssembly(dll));
                }
                catch (Exception e) {
                    log.LogError($"Failed to resolve: {e}");
                }
            }
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name) {
            AssemblyDefinition assembly;
            try {
                // Try to resolve normally.
                assembly = DefaultResolver.Resolve(name);
            }
            catch {
                // Resolve from our collection if normal resolution fails.
                assembly = Libraries.First(x => x.Name.Name == name.Name);
            }

            return assembly ?? base.Resolve(name);
        }
    }
}