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
        private static readonly Lazy<Dictionary<string, string>> TrustedPlatformAssembles = new(CreateTrustedPlatformAssemblyMap);

        private readonly DefaultAssemblyResolver DefaultResolver = new();
        private readonly List<AssemblyDefinition> Libraries = new();
        private readonly TaskLoggingHelper Log;

        public TerrariaAssemblyResolver(TaskLoggingHelper log, string rootDirectory) {
            Log = log;
            Log.LogMessage("Using root directory to resolve assemblies: " + rootDirectory);

            foreach (string dll in Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories)) {
                // Ignore misc. DLLs that we don't care about.
                if (dll.Contains("runtimes") || dll.Contains("resources") || dll.Contains("Native")) continue;
                AddSearchDirectory(dll);
                /*// Add all usable libraries to a collection that we use to resolve from later.
                try {
                    Libraries.Add(AssemblyDefinition.ReadAssembly(dll));
                    Log.LogMessage($"Resolved assembly: {dll}");
                }
                catch (Exception e) {
                    Log.LogError($"Failed to resolve: {dll}\n{e}");
                }*/
            }

            /*// This is some temporary hacky stuff to fix a weird issue.
            foreach (string dll in Directory.GetFiles(Path.Combine(rootDirectory, "..", "dotnet", "6.0.0", "shared"), "**.dll", SearchOption.AllDirectories)) {
                try {
                    Libraries.Add(AssemblyDefinition.ReadAssembly(dll));
                    Log.LogMessage($"Resolved assembly: {dll}");
                }
                catch (Exception e) {
                    Log.LogMessage($"Failed to resolve: {dll}\n{e}");
                }
            }*/
        }

        public override AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters) {
            AssemblyDefinition? assembly = null;
            try {
                // Try to resolve normally.
                assembly = DefaultResolver.Resolve(name);
            }
            catch {
                // Resolve from our collection if normal resolution fails.
                assembly ??= Libraries.FirstOrDefault(x => x.Name.Name == name.Name);
                assembly ??= SearchTrustedPlatformAssemblies(name, parameters);
            }

            Log.LogMessage($"Failed to resolve {name.Name}, falling back to BaseAssemblyResolver...");
            assembly ??= base.Resolve(name, parameters);
            Log.LogMessage(assembly is null ? $"Failed to resolve {name.Name}." : $"Successfully resolved {name.Name}.");
            return assembly;
        }

        private AssemblyDefinition? SearchTrustedPlatformAssemblies(AssemblyNameReference name, ReaderParameters parameters) {
            if (name.IsWindowsRuntime) return null;
            if (!TrustedPlatformAssembles.Value.TryGetValue(name.Name, out string path)) return null;
            parameters.AssemblyResolver ??= this;
            return ModuleDefinition.ReadModule(path, parameters).Assembly;
        }

        private static Dictionary<string, string> CreateTrustedPlatformAssemblyMap() {
            Dictionary<string, string> res = new(StringComparer.Ordinal);
            string? paths;
            try {
                paths = (string) AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            }
            catch {
                return res;
            }

            foreach (string path in paths.Split(Path.PathSeparator))
                if (string.Equals(Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase))
                    res[Path.GetFileNameWithoutExtension(path)] = path;

            return res;
        }
    }
}