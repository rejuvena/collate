using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rejuvena.Collate.Util
{
    public static class ContentConverters
    {
        public const int RAWIMG_FORMAT_VERSION = 1;

        public static bool Convert(ref string resName, Stream source, MemoryStream dest) {
            switch (Path.GetExtension(resName).ToLowerInvariant()) {
                case ".png":
                    if (resName == "icon.png") return false;
                    
                    ToRaw(source, dest);
                    resName = Path.ChangeExtension(resName, ".rawimg");
                    return true;

                default:
                    return false;
            }
        }

        public static void ToRaw(Stream src, Stream dst) {
            using Image<Rgba32> image = Image.Load<Rgba32>(src);
            using BinaryWriter writer = new(dst);

            writer.Write(RAWIMG_FORMAT_VERSION);
            writer.Write(image.Width);
            writer.Write(image.Height);

            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    Rgba32 color = image[x, y];

                    if (color.A == 0) {
                        dst.WriteByte(0);
                        continue;
                    }

                    dst.WriteByte(color.R);
                    dst.WriteByte(color.G);
                    dst.WriteByte(color.B);
                    dst.WriteByte(color.A);
                }
            }
        }
    }
}