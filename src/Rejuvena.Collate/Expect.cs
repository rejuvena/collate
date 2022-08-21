using System.IO;

namespace Rejuvena.Collate
{
    public static class Expect
    {
        public static void FileExists(string filePath, string errorMessage) {
            if (Directory.Exists(filePath)) throw new FileNotFoundException(errorMessage + filePath + "\nFile path led to a directory.");
            if (!File.Exists(filePath)) throw new FileNotFoundException(errorMessage + filePath);
        }
    }
}