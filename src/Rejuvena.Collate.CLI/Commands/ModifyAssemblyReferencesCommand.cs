using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Rejuvena.Collate.CLI.Commands;

[Command(COMMAND_NAME, Description = "Modifies the assembly references of a project.")]
public sealed class ModifyAssemblyReferencesCommand : VersionSensitiveCommand
{
    public const string COMMAND_NAME = "mar";

    protected override string CommandName => COMMAND_NAME;

    /// <summary>
    ///     The project directory.
    /// </summary>
    [CommandOption("proj-dir", 'p', IsRequired = true, Description = "The project directory.")]
    public string ProjectDirectory { get; set; } = string.Empty;

    /// <summary>
    ///     Paths to any access transformers to apply.
    /// </summary>
    [CommandOption("at-paths", 'a', Description = "Paths to any access transformers to apply.")]
    public string AccessTransformerPaths { get; set; } = string.Empty;

    /// <summary>
    ///     The references to modify and output.
    /// </summary>
    [CommandOption("references", 'r', IsRequired = true, Description = "The references to modify and output.")]
    public string References { get; set; } = string.Empty;

    protected override async ValueTask ExecuteAsync(IConsole console, Version version) {
        // The following is how the output should be interpreted, given what a line starts with:
        //  > '+': Add a reference.
        //  > '-': Remove a reference.
        //  > Anything else is to be ignored by the filter in Rejuvena.Collate.MSBuild.

        async Task log(string message) {
            await console.Output.WriteLineAsync(message);
        }

        async Task addReference(string reference) {
            await log('+' + reference);
        }

        async Task removeReference(string reference) {
            await log('-' + reference);
        }

        if (Debug) {
            await log("Project directory: "        + ProjectDirectory);
            await log("Access transformer paths: " + AccessTransformerPaths);
            await log("References: "               + References);
        }

        await addReference("test");
        await addReference("test2");
        await removeReference("test3");
        await removeReference("test4");
    }
}
