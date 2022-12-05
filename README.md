# Collate

> Convenient, cross-platform tModLoader "ModCompile" build toolchain and build system alternative.

---

_Collate_ is a [free](https://www.gnu.org/philosophy/free-sw.en.html) (as in both [free speech and free beer](https://en.wikipedia.org/wiki/Gratis_versus_libre)) toolchain and system for compiling and packing (building) tModLoader mods into a distributable `.tmod` archive for [tModLoader](https://github.com/tModLoader/tModLoader) 2022.x.x.x (Terraria 1.4).

Skipping the buzzwords, Collate uses a CLI tool and MSBuild tasks to manipulate and modify references and project build steps in order to introduce new, convenient features as well as to automate annoying tasks.

## Features

> This is the `dev/v2` branch, which is a full rewrite of the program.

Collate is split into three main projects:

- `Rejuvena.Collate` - The main library that could theoretically be implemented wherever.
- `Rejuvena.Collate.CLI` - CLI implementation of the `Rejuvena.Collate` API, used to communicate with the MSBuild project.
- `Rejuvena.Collate.MSBuild` - MSBuild tasks for interacting with `Rejuvena.Collate.CLI` through `Exac` tasks.

Numerous features are planned/implemented:

- [x] Custom `build.txt` formats[^1]:
  - [x] `.txt` (original)
    - Extended syntax: supports lines starting with `#` as comments.
  - [x] `.csproj` (properties)
  - [ ] `.js`
  - [ ] `.lua`
  - [ ] `.json`
  - [ ] `.yaml`
  - [ ] `.toml`
- [ ] Building through MSBuild/`dotnet build`, no building using the tModLoader assembly.

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

[^1]: Any features that support multiple file types will be checked if even one file type as been implemented.
