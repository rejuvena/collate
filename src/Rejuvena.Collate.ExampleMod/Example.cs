using HtmlAgilityPack;
using Terraria.ModLoader;

namespace Rejuvena.Collate.ExampleMod;

public class Example : Mod
{
    public override void Load() {
        base.Load();
        
        Logger.Info("Hello from Collate!");

        var web = new HtmlWeb();
        var doc = web.Load("https://tmodloader.net");
        Logger.Info(doc.ToString());
    }
}
