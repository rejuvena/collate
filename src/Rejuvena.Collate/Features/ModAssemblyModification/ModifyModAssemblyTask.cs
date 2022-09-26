using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Rejuvena.Collate.Cecil;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate.Features.ModAssemblyModification
{
    public sealed class ModifyModAssemblyTask : BuildTask
    {
        internal record ModAssemblyContext(string AssemblyName)
        {
            public string AssemblyName { get; } = AssemblyName;
        }

        [Required]
        public string OutputPath { get; set; } = "";

        [Required]
        public string AssemblyName { get; set; } = "";

        /*[Required]
        public string RootNamespace { get; set; } = "";*/

        private static ModuleTransformer<ModAssemblyContext>[] Modifications =
        {
            new InjectRootNamespaceTypeModification()
        };

        public override bool Execute() {
            string assemblyPath = Path.Combine(Path.GetFullPath(OutputPath), AssemblyName + ".dll");
            bool modified = false;

            try {
                Log.LogMessage("Preparing to modify the compiled mod assembly...");

                ModuleDefinition? module = ModuleFactory.CreateModuleFromFile(assemblyPath);
                if (module is null) throw new FileLoadException("Failed to load mod assembly into ModuleDefinition, path: " + assemblyPath);

                ModAssemblyContext ctx = new(AssemblyName);
                modified = Modifications.Aggregate(modified, (current, modification) => current | modification.Transform(Log, module, ctx));

                if (!modified) {
                    module.Dispose();
                    Log.LogMessage("No modifications were made to the compiled mod assembly, as none were necessary.");
                }
                else {
                    Log.LogMessage("Writing modified mod assembly to disk...");

                    using MemoryStream dllMem = new();
                    module.Write(dllMem);
                    module.Dispose();
                    File.WriteAllBytes(assemblyPath, dllMem.ToArray());
                }
            }
            catch (Exception e) {
                Log.LogError(!modified
                                 ? "An error occured when preparing to transform the compiled mod assembly."
                                 : "An error occured when transforming the compiled mod assembly.");
                Log.LogErrorFromException(e);
            }

            return true;
        }
    }
}