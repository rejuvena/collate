using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Rejuvena.Collate.Features.Packaging.Converters;
using Rejuvena.Collate.TML;
using Rejuvena.Collate.Util;
using TML.Files;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace Rejuvena.Collate.Features.Packaging
{
    /// <summary>
    ///     Packages the compiled assembly and PDB of a mod alongside its resources into a .tmod archive.
    /// </summary>
    public class PackageModTask : BuildTask
    {
        /// <summary>
        ///     NuGet package references.
        /// </summary>
        [Required]
        public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        ///     Assembly references.
        /// </summary>
        [Required]
        public ITaskItem[] ReferencePaths { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        ///     Mod references.
        /// </summary>
        [Required]
        public ITaskItem[] ModReferences { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        ///     The directory of the project that is being built and packaged.
        /// </summary>
        [Required]
        public string ProjectDirectory { get; set; } = string.Empty;

        /// <summary>
        ///     The output path that the compiled mod assembly lies in. Not the same as <see cref="OutputTmodPath"/>.
        /// </summary>
        [Required]
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        ///     The assembly name of the mod, which also serves as the internal name.
        /// </summary>
        [Required]
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        ///     The mod loader version the mod was built with.
        /// </summary>
        [Required]
        public string TmlVersion { get; set; } = string.Empty;

        /// <summary>
        ///     The tModLoader assembly path.
        /// </summary>
        [Required]
        public string TmlDllPath { get; set; } = string.Empty;

        /// <summary>
        ///     The output path that the .tmod archive should be written to.
        /// </summary>
        public string OutputTmodPath { get; set; } = string.Empty;

        /// <summary>
        ///     Mod build properties, serving as an alternative to the <c>build.txt</c> file.
        /// </summary>
        [Required]
        public ITaskItem[] ModProperties { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute() {
            OutputTmodPath = GetOutputTmodPath();

            PathNamePair modDll = new(AssemblyName + ".dll", Path.Combine(ProjectDirectory, OutputPath));
            PathNamePair modPdb = new(AssemblyName + ".pdb", Path.Combine(ProjectDirectory, OutputPath));
            ModFileWriter writer = new();
            BuildProperties props = MakeModProperties();
            CollateModFile buildFile = new(TmlVersion, AssemblyName, props.Version.ToString());

            buildFile.AddFileFromPath(modDll, onError: () => throw new FileNotFoundException("Mod assembly not present, expected at: " + modDll.Path));
            buildFile.AddFileFromPath(modPdb, onError: () => { Log.LogWarning("Could not resolve mod .pdb, expected at: " + modPdb.Path); });

            AddAllReferences(buildFile, props);
            buildFile.AddFile("Info", props.ToBytes(TmlVersion));

            Log.LogMessage(MessageImportance.Low, "Adding resources...");
            List<string> resources = Directory.GetFiles(ProjectDirectory, "*", SearchOption.AllDirectories).Where(x => !IgnoreResource(props, x)).ToList();
            Parallel.ForEach(resources, x => AddResource(buildFile, x));

            Log.LogMessage(MessageImportance.Low, "Writing .tmod file to: " + OutputTmodPath);

            if (File.Exists(OutputTmodPath)) File.Delete(OutputTmodPath);
            using Stream modFile = File.Open(OutputTmodPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            writer.Write(buildFile, modFile, new ModFileWriterSettings(buildFile.Header, buildFile.ModLoaderVersion, buildFile.Name, buildFile.Version));

            // TODO: Let people specify the enabled.json file path? Kinda useless...?
            string enabledJsonPath = Path.Combine(Path.GetDirectoryName(OutputTmodPath)!, "enabled.json");
            Utilities.EnableMod(Log, enabledJsonPath, AssemblyName);
            return true;
        }

        protected string GetOutputTmodPath() {
            OutputTmodPath = string.IsNullOrEmpty(OutputTmodPath) ? Utilities.FindSavePath(Log, TmlDllPath, AssemblyName) : OutputTmodPath;
            Directory.CreateDirectory(
                Path.GetDirectoryName(OutputTmodPath) ?? throw new DirectoryNotFoundException("Could not find parent directory of output path!")
            );

            Log.LogMessage(MessageImportance.Normal, "Using output path (for resulting .tmod file): " + OutputTmodPath);
            return OutputTmodPath;
        }

        protected void AddAllReferences(CollateModFile modFile, BuildProperties props) {
            List<ITaskItem> nugetReferences = GetNugetReferences();
            List<ITaskItem> modReferences = GetModReferences();

            // Assumes all DLL references are under the mod's folder (at same level or in sub-folders).
            // Letting DLL references by anywhere would mean doing some weird filters on references, TODO: explore?
            // or using a custom <DllReference> tag that would get translated to a <Reference>.
            List<ITaskItem> dllReferences = ReferencePaths.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectDirectory)).ToList();
            Log.LogMessage(MessageImportance.Low, $"Found {dllReferences.Count} DLL references.");

            foreach (ITaskItem taskItem in nugetReferences) {
                string nugetName = "lib/" + taskItem.GetMetadata("NuGetPackageId") + ".dll";
                string nugetFile = taskItem.GetMetadata("HintPath");

                Log.LogMessage(MessageImportance.Low, $"Adding NuGet dependency {nugetFile} at path \"{nugetFile}\".");
                if (string.Equals(taskItem.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase)) continue;

                modFile.AddFile(nugetName, File.ReadAllBytes(nugetFile));
                props.AddDllReference(taskItem.GetMetadata("NuGetPackageId"));
            }

            foreach (ITaskItem dllReference in dllReferences) {
                string dllPath = dllReference.GetMetadata("FullPath");
                string dllName = Path.GetFileNameWithoutExtension(dllPath);

                Log.LogMessage(MessageImportance.Low, $"Adding DLL reference at path \"{dllPath}\".");
                if (string.Equals(dllReference.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase)) continue;

                modFile.AddFile($"lib/{dllName}.dll", File.ReadAllBytes(dllPath));
                props.AddDllReference(dllName);
            }

            foreach (ITaskItem modReference in modReferences) {
                string? modName = modReference.GetMetadata("Identity");
                string? weakRef = modReference.GetMetadata("Weak");

                Log.LogMessage(MessageImportance.Low, $"Adding mod reference \"{modName}\" (weak: {weakRef}).");
                props.AddModReference(modName, string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase));
            }
        }

        protected List<ITaskItem> GetNugetReferences() {
            Dictionary<string, ITaskItem> nugetLookup = PackageReferences.ToDictionary(x => x.ItemSpec);

            if (nugetLookup.ContainsKey("Rejuvena.Collate")) nugetLookup.Remove("Rejuvena.Collate");
            if (nugetLookup.ContainsKey("tModLoader.CodeAssist")) nugetLookup.Remove("tModLoader.CodeAssist");

            List<ITaskItem> nugetReferences = new();
            foreach (ITaskItem referencePath in ReferencePaths) {
                string? hintPath = referencePath.GetMetadata("HintPath");
                string? nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
                string? nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

                if (string.IsNullOrEmpty(nugetPackageId)) continue;
                if (!nugetLookup.ContainsKey(nugetPackageId)) continue;

                Log.LogMessage(MessageImportance.Low, $"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
                nugetReferences.Add(referencePath);
            }

            Log.LogMessage(MessageImportance.Normal, $"Found {nugetReferences.Count} NuGet references.");

            if (nugetLookup.Count != nugetReferences.Count) Log.LogWarning($"Expected {nugetLookup.Count} NuGet references but found {nugetReferences.Count}.");
            return nugetReferences;
        }

        protected List<ITaskItem> GetModReferences() {
            List<ITaskItem> modReferences = new();
            foreach (ITaskItem modReference in ModReferences) {
                string? modPath = modReference.GetMetadata("HintPath");
                if (string.IsNullOrEmpty(modPath)) modPath = modReference.GetMetadata("ProjectPath");
                string? modName = modReference.GetMetadata("Identity");
                string? weakRef = modReference.GetMetadata("Weak");
                bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrEmpty(modName))
                    throw new ArgumentException(
                        "A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing."
                    );

                Log.LogMessage(MessageImportance.Low, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
                modReferences.Add(modReference);
            }

            Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
            return modReferences;
        }

        protected BuildProperties MakeModProperties() {
            // TODO: Let this be specified as a property in the .csproj? Would be funny!!
            string buildInfoFile = Path.Combine(ProjectDirectory, "build.txt");

            if (File.Exists(buildInfoFile)) {
                // TODO: Remove this when we add support for custom paths (we should still discourage this, though).
                Log.LogWarning("Using ./build.txt file for properties instead of .csproj files; this is deprecated in Rejuvena.Collate and not recommended.");
                return BuildProperties.ReadBuildInfo(buildInfoFile);
            }

            // TODO: Let description.txt be specified as a property in the .csproj?
            BuildProperties props = BuildProperties.ReadTaskItems(ModProperties);
            string descFilePath = Path.Combine(ProjectDirectory, "description.txt");
            if (!File.Exists(descFilePath)) {
                Log.LogWarning("Mod description (description.txt) file was not found, expected at path: " + descFilePath);
                return props;
            }

            props.Description = File.ReadAllText(descFilePath);
            return props;
        }

        protected bool IgnoreResource(BuildProperties props, string resPath) {
            string relPath = resPath[(ProjectDirectory.Length + 1)..];
            return props.IgnoreFile(relPath)
                   || relPath[0] == '.' // ignore dotfiles (config files/folders, such as .git/, .vs/, .idea/, .gitignore, etc. 
                   || relPath.StartsWith("bin" + Path.DirectorySeparatorChar) // ignore ./bin/
                   || relPath.StartsWith("obj" + Path.DirectorySeparatorChar) // ignore ./obj/
                   || relPath == "build.txt" // TODO: Replace with .csproj-specified path if we add support.
                   || !props.IncludeSource && new[] {".csproj", ".cs", ".sln"}.Contains(Path.GetExtension(resPath)) // remove .cs, .csproj, and .sln files
                   || Path.GetFileName(resPath) == "Thumbs.db"; // ignore Thumbs.db, a dumb Windows file; people should just use Details view anyway
        }

        protected void AddResource(CollateModFile modFile, string resPath) {
            string relPath = resPath[(ProjectDirectory.Length + 1)..];
            Log.LogMessage(MessageImportance.Low, "Packing file into resulting .tmod: " + relPath);

            using FileStream open = File.OpenRead(resPath);
            using MemoryStream memStream = new();

            if (!ContentConverters.Convert(ref relPath, open, memStream)) open.CopyTo(memStream);
            modFile.AddFile(relPath, memStream.ToArray());
        }
    }
}