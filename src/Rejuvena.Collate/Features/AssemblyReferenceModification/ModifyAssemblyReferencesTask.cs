using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using Rejuevna.Collate.AccessTransformer;
using Rejuvena.Collate.Cecil;
using Rejuvena.Collate.Cecil.Resolvers;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate.Features.AssemblyReferenceModification
{
    public sealed class ModifyAssemblyReferencesTask : BuildTask
    {
        [Required]
        public string AccessTransformerPaths { get; set; } = "";

        // [Required]
        [Obsolete("Use AccessTransformerPaths")]
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

            AccessTransformerPaths = AccessTransformerPaths.Trim().Trim(';');

#pragma warning disable CS0618
            if (!string.IsNullOrEmpty(AccessTransformerPath) && File.Exists(AccessTransformerPath)) AccessTransformerPaths += ';' + AccessTransformerPath;
#pragma warning restore CS0618

            string[] atPaths = AccessTransformerPaths.Split(';');
            List<IATFile> atFiles = atPaths
                                   .Where(LogFileExists)
                                   .Select(ATFileFactory.CreateFromFile)
                                   .ToList();

            foreach (string file in referenceFiles) {
                Log.LogMessage("Preparing to transform assembly: " + file);

                bool modified = false;
                string libPath = Path.GetDirectoryName(referenceFiles.First(x => x.EndsWith("tModLoader.dll")))!;
                ModuleDefinition module = ModuleFactory.CreateModuleFromFile(file, new TerrariaAssemblyResolver(Log, libPath));

                foreach (IATFile atFile in atFiles) modified |= atFile.Transform(module);

                if (!modified) {
                    module.Dispose();
                    Log.LogMessage($"No modifications were made to the assembly at \"{file}\", skipping.");
                }
                else {
                    string resultingDir = Path.Combine(LibDir, Path.GetFileName(file));
                    Log.LogMessage($"Modifications were made to \"{file}\", writing to \"{resultingDir}\"...");

                    using MemoryStream dllMem = new();
                    module.Write(dllMem);
                    module.Dispose();
                    File.WriteAllBytes(resultingDir, dllMem.ToArray());
                    cachedReferences.Add((original: file, transformed: resultingDir));
                }
            }

            ReferenceToRemove = string.Join(";", cachedReferences.Select(x => x.original));
            ReferenceToAdd = string.Join(";", cachedReferences.Select(x => x.transformed));
            return true;
        }

        private bool LogFileExists(string file) {
            if (File.Exists(file)) {
                Log.LogMessage("Found access transformer file at: " + file);
                return true;
            }

            Log.LogMessage("Could not find access transformer file at: " + file);
            return false;
        }
    }
}