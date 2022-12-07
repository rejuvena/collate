using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Rejuvena.Collate.CLI.Commands;

/// <summary>
///     A CliFx <see cref="ICommand"/> which describes behavior based on a given <see cref="Version"/>. <br />
///     Provides additional minor abstractions.
/// </summary>
public abstract class VersionSensitiveCommand : ICommand
{
    /// <summary>
    ///     The command's name.
    /// </summary>
    protected abstract string CommandName { get; }

    /// <summary>
    ///     The version of Collate to run against, assumes latest if not specified.
    /// </summary>
    [CommandOption("version", 'v', Description = "The version of Collate to run against, assumes latest if not specified.")]
    public string Version { get; set; } = typeof(VersionSensitiveCommand).Assembly.GetName().Version?.ToString() ?? string.Empty;

    /// <summary>
    ///     Whether this command should run under debug mode.
    /// </summary>
    [CommandOption("debug", 'd', Description = "Whether this command should run under debug mode.")]
    public bool Debug { get; set; } = false;

    async ValueTask ICommand.ExecuteAsync(IConsole console) {
        if (!System.Version.TryParse(Version, out var vers)) throw new InvalidOperationException("Invalid version specified: " + Version);

        if (Debug) await ExecuteDebugAsync(console, vers);

        await ExecuteAsync(console, vers);
    }

    protected abstract ValueTask ExecuteAsync(IConsole console, Version version);

    protected virtual async ValueTask ExecuteDebugAsync(IConsole console, Version version) {
        await console.Output.WriteLineAsync($"Running command \"{CommandName}\" against version \"{version}\"...");
    }
}
