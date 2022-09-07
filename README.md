# `collate`

_Collate_ is an alternative, open-source build toolchain for easily building mods with convenient dependency and file management, as well as [powerful additional features](PLANNED.md) that aim to reduce boilerplate and complexity.

## Features

### Implemented

* MSBuild toolchain.
  * All building is handled through the `dotnet` CLI tool and MSBuild.
* `build.txt` properties in your `.csproj` file.
  * You can specify build properties like the mod display name in your `.csproj` instead of a separate file.
* Private member accessing
  * With the power of access transformers, mods can access non-public members without needing reflection.
* Root namespace requirement bypassing.
  * Your root namespace may now differ from your mod's internal name.

### Planned

* Automatic mod dependeny resolution.
* `build.txt` and `description.txt` in other spots, redirected through a mod's `.csproj`.
