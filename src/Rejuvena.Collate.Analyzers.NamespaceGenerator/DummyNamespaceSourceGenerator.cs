using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Rejuvena.Collate.Analyzers.NamespaceGenerator
{
    /// <summary>
    ///     A source generator which creates an internal, dummy type under the namespace corresponding to the mod's internal name if a type under the determined namespace does not already exist. <br />
    ///     This is done to bypass a restriction within tModLoader which advanced users may find inconveniencing.
    /// </summary>
    [Generator]
    public class DummyNamespaceSourceGenerator : ISourceGenerator
    {
        public const string CLASS_NAME = "_CollateDummyType";

        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context) {
            // Retrieve the assembly name, which matches the mod's internal name.
            string? assemblyName = context.Compilation.AssemblyName;

            // In the event that an assembly is somehow without a name, skip - this is likely a symptom of a larger issue on the modder's side.
            if (assemblyName is null) return;

            // Retrieve all namespaces within the compilation.
            IEnumerable<INamespaceSymbol> namespaces = context.Compilation.GlobalNamespace.GetNamespaceMembers();

            // Check if any namespaces match the assembly name. If at least one does, skip.
            if (namespaces.Any(x => x.Name == assemblyName)) return;

            //$"namespace {assemblyName} {{ internal static class {CLASS_NAME} {{ }} }}"
            StringBuilder sb = new StringBuilder()
                              .AppendLine("using System.Runtime.CompilerServices;")
                              .AppendLine()
                              .AppendLine($"namespace {assemblyName};")
                              .AppendLine()
                              .AppendLine("[CompilerGenerated]")
                              .AppendLine($"internal static class {CLASS_NAME} {{ }}");

            context.AddSource($"{CLASS_NAME}.g.cs", sb.ToString());
        }
    }
}