using System;
using System.IO;
using System.Linq;
using Felt.Needle;
using Felt.Needle.API;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate.Tasks
{
    public class ModifyModAssemblyTask : BuildTask
    {
        [Required]
        public string OutputPath { get; set; } = "";

        [Required]
        public string AssemblyName { get; set; } = "";

        [Required]
        public string RootNamespace { get; set; } = "";

        public override bool Execute() {
            string assemblyPath = Path.Combine(Path.GetFullPath(OutputPath), AssemblyName + ".dll");
            bool modified = false;

            try {
                Log.LogMessage("Preparing to modify the compiled mod assembly...");

                IModuleHandler handler = new StandardModuleHandler(new StandardModuleResolver(), new StandardModuleWriter(), new StandardModuleTransformer());
                ModuleDefinition? module = handler.ModuleResolver.ResolveFromPath(assemblyPath);
                if (module is null) throw new FileLoadException("Failed to load mod assembly into ModuleDefinition, path: " + assemblyPath);

                modified |= InjectRootNamespaceType(Log, module, AssemblyName);

                if (!modified) {
                    module.Dispose();
                    Log.LogMessage("No modifications were made to the compiled mod assembly, as none were necessary.");
                }
                else {
                    Log.LogMessage("Writing modified mod assembly to disk...");

                    using MemoryStream mem = new();
                    handler.ModuleWriter.Write(module, mem);
                    module.Dispose();
                    File.WriteAllBytes(assemblyPath, mem.ToArray());
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

        private static bool InjectRootNamespaceType(TaskLoggingHelper log, ModuleDefinition module, string assemblyName) {
            log.LogMessage("Determining whether mod assembly needs to be injected with a type to appease tML's root namespace requirements...");

            // TODO: The !x.HasCustomAttribute check can be improved - we expect a type with no attributes since JIT control is attribute-based, but we can implement specific JIT attribute checks.
            bool hasRootNamespaceType = module.Types.Any(x => x.Namespace.StartsWith(assemblyName) && !x.HasCustomAttributes);

            if (hasRootNamespaceType) {
                log.LogMessage("Mod assembly already has a type in the root namespace, no injection necessary.");
                return false;
            }

            log.LogMessage("Mod assembly does not have a type in the root namespace, injecting a dummy type...");
            module.Types.Add(
                new TypeDefinition(
                    assemblyName,
                    "CollateGeneratedDummyType",
                    TypeAttributes.Class
                    | TypeAttributes.Abstract
                    | TypeAttributes.Sealed
                )
            );
            log.LogMessage($"Added dummy type to mod assembly (\"{assemblyName}.CollateGeneratedDummyType\").");
            return true;
        }
    }
}