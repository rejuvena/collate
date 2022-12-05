namespace Rejuvena.Collate.Packing.References;

public readonly record struct NuGetReference(string PackageId, string Path, bool Private);
