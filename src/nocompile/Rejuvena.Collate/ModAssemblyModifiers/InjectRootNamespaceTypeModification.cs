/*using System.Linq;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Rejuvena.Collate.Util.Cecil;

namespace Rejuvena.Collate.Features.ModAssemblyModification
{
    internal sealed class InjectRootNamespaceTypeModification : ModuleTransformer<ModAssemblyContext>
    {
        public override bool Transform(TaskLoggingHelper log, ModuleDefinition module, ModAssemblyContext ctx) {
            log.LogMessage("Determining whether mod assembly needs to be injected with a type to appease tML's root namespace requirements...");

            // TODO: The !x.HasCustomAttribute check can be improved - we expect a type with no attributes since JIT control is attribute-based, but we can implement specific JIT attribute checks.
            bool hasRootNamespaceType = module.Types.Any(x => x.Namespace.StartsWith(ctx.AssemblyName) && !x.HasCustomAttributes);

            if (hasRootNamespaceType) {
                log.LogMessage("Mod assembly already has a type in the root namespace, no injection necessary.");
                return false;
            }

            log.LogMessage("Mod assembly does not have a type in the root namespace, injecting a dummy type...");
            // TODO: CompilerGeneratedAttribute is not necessary, but it's nice to have.
            TypeDefinition type = new(ctx.AssemblyName, "<CollateGeneratedDummyType>", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed)
            {
                BaseType = module.TypeSystem.Object
            };
            module.Types.Add(type);
            log.LogMessage($"Added dummy type to mod assembly (\"{ctx.AssemblyName}.<CollateGeneratedDummyType>\").");
            return true;
        }
    }
}*/