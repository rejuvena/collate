using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Felt.Needle;
using Felt.Needle.API;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Rejuvena.Collate.Tasks.AcessTransformer;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate.Tasks
{
    public class ModifyAssemblyReferencesTask : BuildTask
    {
        private class TerrariaAssemblyResolver : BaseAssemblyResolver
        {
            private readonly DefaultAssemblyResolver DefaultResolver = new();
            private readonly List<AssemblyDefinition> Libraries = new();

            public TerrariaAssemblyResolver(TaskLoggingHelper log, string rootDirectory) {
                foreach (string dll in Directory.GetFiles(Path.Combine(rootDirectory, "Libraries"), "*.dll", SearchOption.AllDirectories)) {
                    // Ignore misc. DLLs that we don't care about.
                    if (dll.Contains("runtimes") || dll.Contains("resources") || dll.Contains("Native")) continue;

                    // Add all usable libraries to a collection that we use to resolve from later.
                    try {
                        Libraries.Add(AssemblyDefinition.ReadAssembly(dll));
                    }
                    catch (Exception e) {
                        log.LogMessage($"Failed to resolve: {e}");
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

        [Required]
        public string AccessTransformerPath { get; set; } = "";

        [Required]
        public string ProjectDirectory { get; set; } = "";

        [Required]
        public string References { get; set; } = "";

        [Output]
        public string ReferenceToRemove { get; set; } = "";

        [Output]
        public string ReferenceToAdd { get; set; } = "";

        public string CollateDir => Path.Combine(ProjectDirectory, ".collate");

        public string LibDir => Path.Combine(CollateDir, "lib");

        public override bool Execute() {
            if (Directory.Exists(LibDir)) Directory.Delete(LibDir, true);
            Directory.CreateDirectory(LibDir);

            string[] referenceFiles = References.Split(';');
            List<(string original, string transformed)> cachedReferences = new();

            bool applyAccessTransformer = AccessTransformerPath != "none" && File.Exists(AccessTransformerPath);
            AccessTransformerFile? transformer = applyAccessTransformer ? AccessTransformerFile.ReadFile(AccessTransformerPath) : null;

            foreach (string file in referenceFiles) {
                Log.LogMessage("Preparing to transform assembly: " + file);

                bool modified = false;
                IModuleHandler handler = new StandardModuleHandler(new StandardModuleResolver(), new StandardModuleWriter(), new StandardModuleTransformer());
                ModuleDefinition? module = handler.ModuleResolver.ResolveFromPath(
                    file,
                    new ReaderParameters
                    {
                        AssemblyResolver = new TerrariaAssemblyResolver(Log, Path.GetDirectoryName(referenceFiles.First(x => x.EndsWith("tModLoader.dll")))!)
                    }
                );
                if (module is null) throw new FileLoadException("Failed to load assembly into ModuleDefinition, path: " + file);

                if (transformer is not null) modified |= ApplyAccessTransformers(Log, transformer, module);

                if (!modified) {
                    module.Dispose();
                    Log.LogMessage($"No modifications were made to the assembly at \"{file}\", skipping.");
                }
                else {
                    string resultingDir = Path.Combine(LibDir, Path.GetFileName(file));
                    Log.LogMessage($"Modifications were made to \"{file}\", writing to \"{resultingDir}\"...");

                    using MemoryStream dllMem = new();
                    handler.ModuleWriter.Write(module, dllMem);
                    module.Dispose();
                    File.WriteAllBytes(resultingDir, dllMem.ToArray());
                    cachedReferences.Add((original: file, transformed: resultingDir));
                }
            }

            ReferenceToRemove = string.Join(';', cachedReferences.Select(x => x.original));
            ReferenceToAdd = string.Join(';', cachedReferences.Select(x => x.transformed));
            return true;
        }

        private static bool ApplyAccessTransformers(TaskLoggingHelper log, AccessTransformerFile transformers, ModuleDefinition module) {
            bool modified = false;

            foreach (Transformer transformer in transformers.Items) {
                List<TypeDefinition> types = new();
                foreach (TypeDefinition type in module.Types) CollectAllNested(type, types);

                foreach (TypeDefinition type in types) {
                    if (transformer.ObjectToTransform == type.FullName) {
                        modified = true;
                        type.IsSealed = ReadonlyState(transformer, type.IsSealed);
                        if (type.IsSealed && type.IsAbstract) type.IsSealed = false; // We don't want to ever make types static.
                    }
                    else {
                        IEnumerable<MethodDefinition> methods = type.Methods.Where(method => transformer.ObjectToTransform == method.FullName);
                        foreach (MethodDefinition method in methods) {
                            modified = true;
                            method.IsFinal = ReadonlyState(transformer, type.IsSealed);

                            if (!StaticSafeOperation(transformer, method.IsStatic)) continue;
                            if (transformer.AccessorTransformation == AccessorTransformationType.Inherit) continue;
                            if (transformer.AccessorTransformation == AccessorTransformationType.Internal)
                                method.IsAssembly = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Private)
                                method.IsPrivate = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Protected)
                                method.IsFamily = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Public)
                                method.IsPublic = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.PrivateProtected)
                                method.IsFamilyAndAssembly = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.ProtectedInternal) method.IsFamilyOrAssembly = true;
                        }

                        IEnumerable<FieldDefinition> fields = type.Fields.Where(field => transformer.ObjectToTransform == field.FullName);
                        foreach (FieldDefinition field in fields) {
                            modified = true;
                            field.IsInitOnly = ReadonlyState(transformer, field.IsStatic);

                            if (!StaticSafeOperation(transformer, field.IsStatic)) continue;
                            if (transformer.AccessorTransformation == AccessorTransformationType.Inherit) continue;
                            if (transformer.AccessorTransformation == AccessorTransformationType.Internal)
                                field.IsAssembly = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Private)
                                field.IsPrivate = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Protected)
                                field.IsFamily = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.Public)
                                field.IsPublic = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.PrivateProtected)
                                field.IsFamilyAndAssembly = true;
                            else if (transformer.AccessorTransformation == AccessorTransformationType.ProtectedInternal) field.IsFamilyOrAssembly = true;
                        }
                    }
                }
            }

            return modified;
        }

        private static void CollectAllNested(TypeDefinition type, ICollection<TypeDefinition> types) {
            types.Add(type);
            if (!type.HasNestedTypes) return;
            foreach (TypeDefinition nested in type.NestedTypes) CollectAllNested(nested, types);
        }

        private static bool StaticSafeOperation(Transformer transformer, bool @static) =>
            transformer.AccessorTransformation.NoStatic && !@static || !transformer.AccessorTransformation.NoStatic;

        private static bool ReadonlyState(Transformer transformer, bool @readonly) {
            return transformer.ReadonlyTransformation == ReadonlyTransformationType.Inherit
                ? @readonly
                : transformer.ReadonlyTransformation == ReadonlyTransformationType.Readonly;
        }
    }
}