using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Rejuvena.Collate.ModSourceHelper;

public class ModSourceHelper : Mod {
    public static string collateJsonPath = Path.Combine(Main.SavePath, "Mods", "collate.json");
    public Type uiModSourcesType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIModSources")!;
    public Type localModType = typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.LocalMod")!;
    public ConstructorInfo uiModSourceItemCtor = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIModSourceItem")!
                        .GetConstructor(BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string), typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.LocalMod")! })!;

    public override void Load() {
        base.Load();

        MethodInfo populateInfo = uiModSourcesType.GetMethod("Populate", BindingFlags.NonPublic | BindingFlags.Instance)!;
        HookEndpointManager.Modify(populateInfo, Populate);
        /* MethodInfo getStreamInfo = tModFile.GetMethod("GetStream", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string), typeof(bool) })!; */
        /* HookEndpointManager.Modify(getStreamInfo, GetStream); */
    }

    private void Populate(ILContext il) {
        ILCursor c = new(il);

        // Matches the very end of Populate(), could be a detour if Ldfld wasn't needed for _items
        if (!c.TryGotoNext(MoveType.Before,
            i => i.MatchRet())) {
            Logger.Debug("Terraria.ModLoader.UI.UIModSources.Populate IL Patch could not be applied");
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

                IEnumerable<object> localMods = (IEnumerable<object>)typeof(Mod).Assembly.GetType("Terraria.ModLoader.Core.ModOrganizer")!.GetMethod("FindDevFolderMods", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;

                // Loop through every mod registered for collate
                foreach (KeyValuePair<string, string> project in data.Projects) {
                    if (!File.Exists(project.Value)) {
                        Logger.Warn($"Project {project.Key}:{project.Value} does not exist on the filesystem!");
                        continue;
                    }

                    /* Logger.Debug("TmodFile path: " + ((TmodFile)file).path); */
                    Logger.Info($"Detected project {project.Key}:{project.Value}");

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

        /* WriteIL(il); */
    }

/*     private void GetStream(ILContext il) { */
/*         ILCursor c = new(il); */

/*         c.Emit(OpCodes.Ldarg_0); */
/*         c.Emit(OpCodes.Ldfld, tModFile.GetField("files", BindingFlags.NonPublic | BindingFlags.Instance)); */

/*         c.EmitDelegate<Func<Dictionary<string, FileEntry>, Dictionary<string, FileEntry>>>((files) => { */
/*             Logger.Debug("Count in tmodfile : " + files.Count); */
/*             foreach (KeyValuePair<string, FileEntry> file in files) { */
/*                 Logger.Debug(file.Value.Name); */
/*             } */

/*             return files; */
/*         }); */

/*         c.Emit(OpCodes.Pop); */
/*     } */
}

// Ensure the counterpart in ModsFolderUtils is the same
public class CollateSaveData {
    public Dictionary<string, string> Projects;

    public CollateSaveData() {
        Projects = new Dictionary<string, string>();
    }
}