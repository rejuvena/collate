﻿using System;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.ModLoader;

namespace Rejuvena.Collate.ExampleMod
{
    public class Example : Mod
    {
        public class Test : IAsset
        {
            public void Dispose() { }

            public AssetState State { get; }
            public IContentSource Source { get; }
            public string Name { get; }
            public bool IsLoaded { get; }
            public bool IsDisposed { get; }

            Action IAsset.Continuation {
                set { }
            }
        }

        public override void Load() {
            base.Load();
            Logger.Info("Hello from outside of ModSources!");

            // Demonstration of using a NuGet package.
            HtmlAgilityPack.HtmlWeb web = new();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://tmodloader.net");
            Logger.Info(doc.Text);

            // This doesn't actually change anything, but demonstrates accessing a non-public member through regular code with an access transformer.
            loading = true;
        }
    }
}