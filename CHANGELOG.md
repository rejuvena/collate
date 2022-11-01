# Changelog

User-facing changes are documented here, per version.

## Unreleased

> **Warning** | This version has not yet been released, and does not reflect the final product nor the current released version.

### Changes

-  Fix tModLoader preview build detection ([#10](https://github.com/rejuvena/collate/issues/10), thanks [@Ved_s](https://github.com/Ved_s)!).

## 1.2.3 - 31 October 2022

### Changes

- References modified by access transformers are no longer packaged as DLL references.

## 1.2.2 - 31 October 2022

### Changes

- Fixed poor assembly resolution logic (fixes [#9](https://github.com/rejuvena/collate/issues/9).
- Fixed project loading failing with no access transformers were specified (fixes [#7](https://github.com/rejuvena/collate/issues/7) for real).

## 1.2.1 - 29 October 2022

### Changes

- Fixed project loading failing with no access transformers were specified (resolves [#7](https://github.com/rejuvena/collate/issues/7)).
- Fixed mod packaging failing when attempting to load the tModLoader DLL (reslolves [#8](https://github.com/rejuvena/collate/issues/8)).

## 1.2.0 - 28 October 2022

### Additions

- Added an analyzer project (`Rejuvena.Collate.Analyzer`) for source generation and code analysis.
- Added the `NamespaceGenerator` property, allowing a user to enable the `Rejuvena.Collate.Analyzers.NamespaceGenerator` source generator through an `enable`/`disable` switch:
  - `<NamespaceGenerator>enable</NamespaceGenerator>
  - `<NamespaceGenerator>disable</NamespaceGenerator>

### Changes

- Back-ported the projects to .NET Standard 2.0.
- Fixed issues with packaging in Visual Studio (resolves [#1](https://github.com/rejuvena/collate/issues/1)).
- Fixed issues with transforming multiple assemblies and transforming dependencies of other assembly references (resolves [#6](https://github.com/rejuvena/collate/issues/6)).
- Dummy namespace injection has been moved to a more reliable source generator.
- Moved many core functionalities to NuGet packages that may be used separately from the main Rejuvena.Collate project.

### Removed

- Removed `Felt.Needle` dependency.

## 1.1.0 - 7 September 2022

### Additions

- Added the `TMLCodeAssist` property, allowing a user to enable tModLoader's official analyzers through an `enable`/`disable` switch:
  - `<TMLCodeAssist>enable</TMLCodeAssist>
  - `<TMLCodeAssist>disable</TMLCodeAssist>
- Added a task to insert a dummy class with the mod's internal name as its namespace if not present (implements [#2](https://github.com/rejuvena/collate/issues/2)).
- Added access transformers. <!-- TODO: ELABORATE -->

## 1.0.1 - 4 September 2022

### Changes

- Fixed bug that improperly converted `.png` files to `.rawimg` files, causing. 

## 1.0.0 - 25 August 2022 @ [`20b292..9cb36d`](https://github.com/rejuvena/collate/compare/20b292..9cb36d)

### Additions

- Added the core of Collate, the alternative method of packaging a mod into a `.tmod` archive file.
- Added the ability to define mod build properties (`build.txt`) inside of the `.csproj` file.
- Added rudimentary automatic `tModLoader.targets` path detection.
- Added checking for the `TMODLOADER_TARGETS_PATH` environment variable, allowing users to set their `tModLoader.targets` path manually.
- Added rudimentary support for building mods with NuGet dependencies.
