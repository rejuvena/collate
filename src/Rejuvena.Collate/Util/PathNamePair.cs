using IOPath = System.IO.Path;

namespace Rejuvena.Collate.Util
{
    internal readonly record struct PathNamePair
    {
        public string Name { get; }

        public string Path { get; }

        public PathNamePair(string name, string pathWithoutName) {
            Name = name;
            Path = IOPath.Combine(pathWithoutName, name);
        }
    }
}