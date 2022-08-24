using System;
using System.IO;
using Rejuvena.Collate.Util;

namespace Rejuvena.Collate.Extensions
{
    public static class CollateModFileExtensions
    {
        public static void AddFileFromPath(this CollateModFile modFile, string fileName, string path, Action? onSuccess = null, Action? onError = null) {
            if (File.Exists(path)) {
                modFile.AddFile(fileName, File.ReadAllBytes(path));
                onSuccess?.Invoke();
            }
            else
                onError?.Invoke();
        }

        public static void AddFileFromPath(this CollateModFile modFile, PathNamePair pnPair, Action? onSuccess = null, Action? onError = null) {
            modFile.AddFileFromPath(pnPair.Name, pnPair.Path, onSuccess, onError);
        }
    }
}