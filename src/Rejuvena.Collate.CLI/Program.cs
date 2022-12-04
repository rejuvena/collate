using System.Threading.Tasks;
using CliFx;

namespace Rejuvena.Collate.CLI;

/// <summary>
///     Entrypoint class.
/// </summary>
public static class Program
{
    /// <summary>
    ///     Entrypoint method.
    /// </summary>
    /// <param name="args">The environment arguments.</param>
    /// <returns>The exit code.</returns>
    public static async Task<int> Main(string[] args) {
        return await new CliApplicationBuilder()
            .SetTitle("Rejuvena.Collate")
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync(args);
    }
}
