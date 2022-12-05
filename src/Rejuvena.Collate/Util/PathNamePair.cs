using TML.Files;

namespace Rejuvena.Collate.Util;

/// <summary>
///     Utility object that contains both the file's name and the file's full path and name, used for adding to a <see cref="TModFile"/>.
/// </summary>
public readonly record struct PathNamePair
{
    public string Name { get; }

    public string Path { get; }

    public PathNamePair(string name, string path) {
        Name = name;
        Path = System.IO.Path.Combine(path, name);
    }
}
