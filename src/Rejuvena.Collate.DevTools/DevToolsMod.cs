using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Rejuvena.Collate.DevTools;

public class DevToolsMod : Mod {
    public static string collateJsonPath = Path.Combine(Main.SavePath, "Mods", "collate.json");
    public Type uiModSourcesType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIModSources")!;
    public Type uiModSourceItemType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIModSourceItem")!;
    public Type localModType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.LocalMod")!;
    public ConstructorInfo uiModSourceItemCtor => uiModSourceItemType
        .GetConstructor(BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string), typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.LocalMod")! })!;
    public Type modCompileType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.ModCompile")!;

    MethodInfo populateInfo => uiModSourcesType.GetMethod("Populate", BindingFlags.NonPublic | BindingFlags.Instance)!;
    MethodInfo buildModInfo => modCompileType.GetMethod(
        "Build",
        BindingFlags.NonPublic | BindingFlags.Instance,
        new Type[] { modCompileType.GetNestedType("BuildingMod", BindingFlags.NonPublic | BindingFlags.Instance)! })!;
    MethodInfo drawSelfInfo => uiModSourceItemType.GetMethod("DrawSelf", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public override void Load() {
        base.Load();

        HookEndpointManager.Modify(populateInfo, Populate);
        HookEndpointManager.Modify(buildModInfo, BuildMod);
        HookEndpointManager.Modify(drawSelfInfo, DrawSelf);
    }

    public override void Unload() {
        base.Unload();

        // Does not unload???? Need to figure out why IL is duplicating
        HookEndpointManager.Unmodify(populateInfo, Populate);
        HookEndpointManager.Unmodify(buildModInfo, BuildMod);
        HookEndpointManager.Unmodify(drawSelfInfo, DrawSelf);
    }

    // Terraria.ModLoader.UI.UIModSources.Populate
    private void Populate(ILContext il) {
        ILCursor c = new(il);

        // Matches the very end of Populate(), could be a detour if Ldfld wasn't needed for _items
        if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchRet())) {
            Logger.Error("Terraria.ModLoader.UI.UIModSources.Populate IL Patch could not be applied");
            return;
        }

        // Pass instance of class onto stack so Ldfld can be used
        c.Emit(OpCodes.Ldarg_0);

        // Get the List<UIModSourceItem> in UIModSources
        c.Emit(OpCodes.Ldfld, uiModSourcesType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));

        // Add sources found in collate.json to _items
        c.EmitDelegate<Func<IList, IList>>((list) => {
            Task.Run(() => {
                CollateSaveData data = File.Exists(collateJsonPath) ? JsonConvert.DeserializeObject<CollateSaveData>(File.ReadAllText(collateJsonPath)) : new CollateSaveData();

                if (data is null)
                    data = new();

                // Get list of detected mods
                IEnumerable<object> localMods = (IEnumerable<object>)typeof(Mod).Assembly
                    .GetType("Terraria.ModLoader.Core.ModOrganizer")!
                    .GetMethod("FindDevFolderMods", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, null)!;

                // Loop through every mod registered for collate
                foreach (KeyValuePair<string, string> project in data.Projects) {
                    if (!File.Exists(project.Value)) {
                        Logger.Warn($"Terraria.ModLoader.UI.UIModSources.Populate: Project {project.Key}:{project.Value} does not exist on the filesystem!");
                        continue;
                    }

                    Logger.Info($"Detected project {project.Key}:{project.Value}");

                    // Match project name with tmod file on disk
                    object localMod = localMods.SingleOrDefault(
                            m => (string)localModType
                            .GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)!
                            .GetValue(m)! == project.Key + ".tmod")!;

                    // Create new UIModSourceItem. Takes the sourcePath and a LocalMod
                    object uiModSourceItem = uiModSourceItemCtor
                        .Invoke(new object?[] { Path.GetDirectoryName(project.Value), localMod });

                    list.Add(uiModSourceItem);
                }
            });

            return list;
        });

        c.Emit(OpCodes.Ldarg_0);

        // Set field with the new value on the stack
        c.Emit(OpCodes.Stfld, uiModSourcesType.GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
    }

    // Terraria.ModLoader.Core.ModCompile.Build
    private void BuildMod(ILContext il) {
        ILCursor c = new(il);

        FieldInfo statusInfo = modCompileType.GetField("status", BindingFlags.NonPublic | BindingFlags.Instance)!;
        ILLabel normalMod = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchLdarg(0),
            i => i.MatchLdfld(statusInfo),
            i => i.MatchLdstr("tModLoader.Building"))) {
            Logger.Error("Terraria.ModLoader.Core.ModCompile.Build IL Patch could not be applied");
            return;
        }

        // Load IBuildStatus instance
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldfld, statusInfo);
        c.Emit(OpCodes.Box, typeof(object));

        // Get BuildingMod parameter
        c.Emit(OpCodes.Ldarg_1);
        c.Emit(OpCodes.Box, typeof(object));

        // Handle collate mods, returning false if the mod isn't collate.
        c.EmitDelegate<Func<object, object, bool>>((status, mod) => {
            CollateSaveData data = File.Exists(collateJsonPath) ? JsonConvert.DeserializeObject<CollateSaveData>(File.ReadAllText(collateJsonPath)) : new CollateSaveData();
            Type iBuildStatusType = modCompileType.GetNestedType("IBuildStatus", BindingFlags.Public | BindingFlags.Instance)!;
            MethodInfo setProgress = iBuildStatusType.GetMethod("SetProgress", BindingFlags.Public | BindingFlags.Instance)!;

            if (data is null) {
                Logger.Warn($"Terraria.ModLoader.Core.ModCompile.Build: {collateJsonPath} could not be found");
                return false;
            }

            string name = (string)modCompileType
                .GetNestedType("BuildingMod", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(mod)!;

            // Normal mod. Goofy stuff because the name of the BuildingMod isn't the same as what's stored in the dict
            if (!data.Projects.Values.Select(v => Path.GetFileNameWithoutExtension(v)).Contains(name))
                return true;

            string key = data.Projects.FirstOrDefault(b => Path.GetFileNameWithoutExtension(b.Value) == name).Key;

            // Set build status
            iBuildStatusType.GetMethod("SetStatus", BindingFlags.Public | BindingFlags.Instance)!
                .Invoke(status, new object?[] { Language.GetTextValue("tModLoader.Building", $"[Collate] {name}") });

            ProcessStartInfo psi;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                psi = new("/usr/bin/env", $"dotnet build {data.Projects[key]}");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                psi = new("dotnet.exe", $"build {data.Projects[key]}");
            else
                throw new PlatformNotSupportedException("Unknown platform, cannot find dotnet executable");

            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            // Prevent dotnet from freaking out on *nix systems
            if (psi.EnvironmentVariables.ContainsKey("LD_PRELOAD"))
                psi.EnvironmentVariables["LD_PRELOAD"] = "";

            Process proc = new() {
                StartInfo = psi,
            };
            proc.Start();

            // Half progress bar
            setProgress.Invoke(status, new object?[] { 1, 2});

            proc.WaitForExit();
            string res = proc.StandardOutput.ReadToEnd();

            if (res.Contains("Build FAILED."))
                throw new Exception("Collate Dotnet Build Error:\n" + res);

            // If the mod is loaded, close it to avoid open handles
            if (ModLoader.TryGetMod(name, out Mod loadedMod))
                loadedMod.Close();

            // Full progress bar
            setProgress.Invoke(status, new object?[] { 2, 2});

            // Enable the mod
            typeof(Mod).Assembly.GetType("Terraria.ModLoader.ModLoader")!
                .GetMethod("EnableMod", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object?[] { name });

            return false;
        });

        // If the mod is a normal mod, then jump to the label and continue the normal execution
        c.Emit(OpCodes.Brtrue, normalMod);

        // Return if the mod is a collate mod
        c.Emit(OpCodes.Ret);

        c.MarkLabel(normalMod);
    }

    // Terraria.ModLoader.UI.UIModSourceItem.DrawSelf
    private void DrawSelf(ILContext il) {
        ILCursor c = new(il);

        FieldInfo _modInfo = uiModSourceItemType.GetField("_mod", BindingFlags.NonPublic | BindingFlags.Instance)!;
        ILLabel collateMod = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchStfld(out _),
            i => i.MatchLdcI4(-26),
            i => i.MatchStloc(4),
            i => i.MatchLdcI4(0),
            i => i.MatchStloc(5))) {
            Logger.Error("Terraria.ModLoader.UI.UIModSourceItem.DrawSelf IL Patch could not be applied");
            return;
        }

        // Get _mod
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldfld, _modInfo);

        // Check for a collate mod, returning false if so
        c.EmitDelegate<Func<string, bool>>((mod) => {
            string modFolderName = Path.GetFileName(mod);
            string csprojFile = Path.Combine(mod, $"{modFolderName}.csproj");

            CollateSaveData data = File.Exists(collateJsonPath) ? JsonConvert.DeserializeObject<CollateSaveData>(File.ReadAllText(collateJsonPath)) : new CollateSaveData();

            if (data.Projects.Values.Contains(csprojFile))
                return false;

            return true;
        });

        // Jump to collateMod if false
        c.Emit(OpCodes.Brfalse, collateMod);

        // Match after the body of the if statement for the exclamation button
        if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchLdloc(3),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld(_modInfo),
            i => i.MatchLdstr("*.lang"),
            i => i.MatchLdcI4(1))) {
            Logger.Error("Terraria.ModLoader.UI.UIModSourceItem.DrawSelf unable to define collateMod label");
            return;
        }

        // Jump to IL_084 if the mod is collate
        c.MarkLabel(collateMod);
    }
}

// Ensure the counterpart in ModsFolderUtils is the same
public class CollateSaveData {
    public Dictionary<string, string> Projects;

    public CollateSaveData() {
        Projects = new Dictionary<string, string>();
    }
}