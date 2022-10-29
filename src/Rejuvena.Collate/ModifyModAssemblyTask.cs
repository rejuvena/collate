using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Rejuvena.Collate.ModAssemblyModifiers;
using Rejuvena.Collate.Util.Cecil;
using Rejuvena.Collate.Util.Cecil.Resolvers;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate
{
    public sealed class ModifyModAssemblyTask : BuildTask
    {
        [Required]
        public string OutputPath { get; set; } = "";

        [Required]
        public string AssemblyName { get; set; } = "";

        private static ModuleTransformer<ModAssemblyContext>[] Modifications =
        {
            // new InjectRootNamespaceTypeModification()
        };

        public override bool Execute() {
            string assemblyPath = Path.Combine(Path.GetFullPath(OutputPath), AssemblyName + ".dll");
            string assemblyOutputPath = Path.Combine(Path.GetFullPath(OutputPath), AssemblyName + ".dll.modified");
            bool modified = false;
            
            // Delete the file if it was left over from a previous build.
            if (File.Exists(assemblyOutputPath)) File.Delete(assemblyOutputPath);

            try {
                Log.LogMessage("Preparing to modify the compiled mod assembly...");

                ModuleDefinition? module = ModuleFactory.CreateModuleFromFile(assemblyPath, new TerrariaAssemblyResolver(Log, Path.GetDirectoryName(OutputPath)!));
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
                    File.WriteAllBytes(assemblyOutputPath, dllMem.ToArray());
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