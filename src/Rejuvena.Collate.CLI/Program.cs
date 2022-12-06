using System;
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
        try {
            return await new CliApplicationBuilder()
                         .SetTitle("Rejuvena.Collate")
                         .AddCommandsFromThisAssembly()
                         .Build()
                         .RunAsync(args);
        }
        catch (Exception e) {
            Console.WriteLine("An error occurred:\n" + e.Message);
            Console.WriteLine("The following arguments were passed:\n" + string.Join('\n', args));
            return 1;
        }
    }
}
