using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Rejuvena.Collate.CLI.Commands;
// dotnet tool -g collate modify-assembly-references --version $(CollateVersion) --projectDirectory "$(ProjectDirectory)" --accessTransformerPaths "$(AccessTransformerPaths)" --references "@(Reference)"

[Command(COMMAND_NAME, Description = "Modifies the assembly references of a project.")]
public sealed class ModifyAssemblyReferencesCommand : VersionSensitiveCommand
{
    public const string COMMAND_NAME = "modify-assembly-reference";

    protected override string CommandName => COMMAND_NAME;

    [CommandOption("projectDirectory", IsRequired = true, Description = "The project directory.")]
    public string ProjectDirectory { get; set; } = string.Empty;

    [CommandOption("accessTransformerPaths", IsRequired = true, Description = "Paths to various access transformers.")]
    public string AccessTransformerPaths { get; set; } = string.Empty;

    [CommandOption("references", IsRequired = true, Description = "The references to modify.")]
    public string References { get; set; } = string.Empty;

    protected override async ValueTask ExecuteAsync(IConsole console, Version version) {
        // The following is how the output should be interpreted, given what a line starts with:
        // -  ' ': Ignore, logging.
        // -  '+': Add a reference.
        // -  '-': Remove a reference.

        async Task log(string message) {
            await console.Output.WriteLineAsync(' ' + message);
        }

        async Task addReference(string reference) {
            await console.Output.WriteLineAsync('+' + reference);
        }

        async Task removeReference(string reference) {
            await console.Output.WriteLineAsync('-' + reference);
        }

        await log("Project directory: " + ProjectDirectory);
        await log("Access transformer paths: " + AccessTransformerPaths);
        await log("References: " + References);
    }
}
