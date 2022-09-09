using System.IO;

namespace Rejuvena.Collate.Features.Packaging.Converters
{
    internal abstract class ContentConverter
    {
        public abstract bool CanConvert(string resName);

        public abstract void Convert(ref string resName, Stream from, MemoryStream to);
    }
}