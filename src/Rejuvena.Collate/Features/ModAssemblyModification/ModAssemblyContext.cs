namespace Rejuvena.Collate.Features.ModAssemblyModification
{
    internal record ModAssemblyContext(string AssemblyName)
    {
        public string AssemblyName { get; } = AssemblyName;
    }
}