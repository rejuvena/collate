using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    [CommandOption("output-file", 'o', IsRequired = true, Description = "The output file.")]
    public string OutputFile { get; set; } = string.Empty;

    protected override async ValueTask ExecuteAsync(IConsole console, Version version) {
        // ReSharper disable once CollectionNeverUpdated.Local
        var added = new List<string>();
        // ReSharper disable once CollectionNeverUpdated.Local
        var removed = new List<string>();

        string contents = string.Join("\n", added.Select(x => '+' + x).Concat(removed.Select(x => '-' + x)));
        await File.WriteAllTextAsync(OutputFile, contents);
    }

    protected override async ValueTask ExecuteDebugAsync(IConsole console, Version version) {
        console.ForegroundColor = ConsoleColor.DarkGray;
        await console.Output.WriteLineAsync();

        await base.ExecuteDebugAsync(console, version);

        await console.Output.WriteLineAsync("Options:");
        await console.Output.WriteLineAsync($"  {nameof(ProjectDirectory)}: "       + ProjectDirectory);
        await console.Output.WriteLineAsync($"  {nameof(AccessTransformerPaths)}: " + AccessTransformerPaths);
        await console.Output.WriteLineAsync($"  {nameof(References)}: "             + References);

        await console.Output.WriteLineAsync();
        console.ResetColor();
    }
}
