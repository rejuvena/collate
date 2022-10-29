namespace Rejuvena.Collate.ModAssemblyModifiers
{
    internal record ModAssemblyContext(string AssemblyName)
    {
        public string AssemblyName { get; } = AssemblyName;
    }
}