using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rejuvena.Collate.ModCompile.Converters
{
    public sealed class PngRawimgConverter : ContentConverter
    {
        public const int RAWIMG_FORMAT_VERSION = 1;

        private static Configuration config;
        static PngRawimgConverter() {
            config = Configuration.Default.Clone();
            config.PreferContiguousImageBuffers = true;
        }

        public override bool CanConvert(string resName) {
            return resName != "icon.png" && Path.GetExtension(resName).ToLowerInvariant() == ".png";
        }

        public override unsafe void Convert(ref string resName, Stream from, MemoryStream to) {
            resName = Path.ChangeExtension(resName, ".rawimg");

            using Image<Rgba32> image = Image.Load<Rgba32>(config, from);
            using BinaryWriter writer = new(to, System.Text.Encoding.UTF8, true);

            writer.Write(RAWIMG_FORMAT_VERSION);
            writer.Write(image.Width);
            writer.Write(image.Height);
            int totalPixels = image.Width * image.Height;

            if (to.Length < totalPixels)
                to.SetLength(totalPixels);

            if (to.TryGetBuffer(out var buffer) && buffer.Count >= totalPixels && image.DangerousTryGetSinglePixelMemory(out var memory)) {
                byte[] b = buffer.Array;
                fixed (byte* _ptr = b)
                fixed (Rgba32* _colorPtr = memory.Span)
                {
                    int* ptr = (int*)(_ptr + buffer.Offset);
                    int* colorPtr = (int*)_colorPtr;
                    for (nint i = 0, c = totalPixels / 4; i < c; i++) {
                        int col = (ptr[i] = colorPtr[i]);
                        if ((col & 0xFF) == 0) {
                            ptr[i] = 0;
                        }
                    }
                }
            }
            else {
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
}