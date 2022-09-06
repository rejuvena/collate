using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rejuvena.Collate.Converters
{
    public class PngRawimgConverter : ContentConverter
    {
        public const int RAWIMG_FORMAT_VERSION = 1;
        
        public override bool CanConvert(string resName) {
            return resName != "icon.png" && Path.GetExtension(resName).ToLowerInvariant() == ".png";
        }

        public override void Convert(ref string resName, Stream from, MemoryStream to) {
            resName = Path.ChangeExtension(resName, ".rawimg");
            
            using Image<Rgba32> image = Image.Load<Rgba32>(from);
            using BinaryWriter writer = new(to);

            writer.Write(RAWIMG_FORMAT_VERSION);
            writer.Write(image.Width);
            writer.Write(image.Height);

            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    Rgba32 color = image[x, y];
                    
                    if (color.A == 0) color = new Rgba32(0, 0, 0, 0);
                    to.WriteByte(color.R);
                    to.WriteByte(color.G);
                    to.WriteByte(color.B);
                    to.WriteByte(color.A);
                }
            }

        }
    }
}