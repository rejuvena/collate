# Changelog

User-facing changes are documented here, per version.


## 1.2.0 - Unreleased

> **Warning** | This version has not yet been released, and does not reflect the final product nor the current released version.

## 1.1.0 - 7 September 2022

- Added the `TMLCodeAssist` property, allowing a user to enable tModLoader's official analyzers through an `enable`/`disable` switch:
  - `<TMLCodeAssist>enable</TMLCodeAssist>
  - `<TMLCodeAssist>disable</TMLCodeAssist>
- Added a task to insert a dummy class with the mod's internal name as its namespace if not present (implements [#2](https://github.com/rejuvena/collate/issues/2)).
- Added access transformers. <!-- TODO: ELABORATE -->

## 1.0.1 - 4 September 2022

- Fixed bug that improperly converted `.png` files to `.rawimg` files, causing. 

## 1.0.0 - 25 August 2022

- Added the core of Collate, the alternative method of packaging a mod into a `.tmod` archive file.
- Added the ability to define mod build properties (`build.txt`) inside of the `.csproj` file.
- Added rudimentary automatic `tModLoader.targets` path detection.
- Added checking for the `TMODLOADER_TARGETS_PATH` environment variable, allowing users to set their `tModLoader.targets` path manually.
- Added rudimentary support for building mods with NuGet dependencies.
