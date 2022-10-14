/*using System.Linq;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Rejuvena.Collate.Util.Cecil;

namespace Rejuvena.Collate.Features.ModAssemblyModification
{
    internal sealed class InjectRootNamespaceTypeModification : ModuleTransformer<ModifyModAssemblyTask.ModAssemblyContext>
    {
        public override bool Transform(TaskLoggingHelper log, ModuleDefinition module, ModifyModAssemblyTask.ModAssemblyContext ctx) {
            log.LogMessage("Determining whether mod assembly needs to be injected with a type to appease tML's root namespace requirements...");

            // TODO: The !x.HasCustomAttribute check can be improved - we expect a type with no attributes since JIT control is attribute-based, but we can implement specific JIT attribute checks.
            bool hasRootNamespaceType = module.Types.Any(x => x.Namespace.StartsWith(ctx.AssemblyName) && !x.HasCustomAttributes);

            if (hasRootNamespaceType) {
                log.LogMessage("Mod assembly already has a type in the root namespace, no injection necessary.");
                return false;
            }

            log.LogMessage("Mod assembly does not have a type in the root namespace, injecting a dummy type...");
            module.Types.Add(
                new TypeDefinition(
                    ctx.AssemblyName,
                    "CollateGeneratedDummyType",
                    TypeAttributes.Class
                    | TypeAttributes.Abstract
                    | TypeAttributes.Sealed
                )
                {
                    BaseType = module.TypeSystem.Object
                }
            );
            log.LogMessage($"Added dummy type to mod assembly (\"{ctx.AssemblyName}.CollateGeneratedDummyType\").");
            return true;
        }
    }
}*/