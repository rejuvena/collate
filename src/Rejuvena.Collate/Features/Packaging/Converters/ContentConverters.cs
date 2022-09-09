using System.IO;

namespace Rejuvena.Collate.Features.Packaging.Converters
{
    public static class ContentConverters
    {
        public static readonly ContentConverter[] Converters =
        {
            new PngRawimgConverter()
        };

        public static bool Convert(ref string resName, Stream from, MemoryStream to) {
            foreach (ContentConverter converter in Converters) {
                if (!converter.CanConvert(resName)) continue;
                converter.Convert(ref resName, from, to);
                return true;
            }

            return false;
        }
    }
}