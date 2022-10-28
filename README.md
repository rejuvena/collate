# Collate

> Efficient tModLoader ModCompile (mod building) toolchain alternative.

---

_Collate_ is an alternative, [free](https://en.wikipedia.org/wiki/Free_software) (as in both [free speech and free beer](https://en.wikipedia.org/wiki/Gratis_versus_libre)) build toolchain for [tModLoader](https://github.com/tModLoader/tModLoader) 2022.x.x.x (1.4).

Collate attempts to supercharge your mod development experience by providing a flexible, easy-to-use build toolchain packed full with additional features.

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
