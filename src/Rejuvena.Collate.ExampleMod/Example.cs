using Terraria.ModLoader;

namespace CollateExample
{
    public class Example : Mod
    {
        public override void Load() {
            base.Load();
            Logger.Info("Hello from outside of ModSources!");
        }
    }
}