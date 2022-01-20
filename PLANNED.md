# Planned Features
To keep everyone in the know, provided is a list of planned features and the basic idea behind their implementation.
* tModLoader assembly transformation.
  * Powered by [whisker](https://github.com/rejuvena/whisker), there will be a set-up process upon initial cloning that will download and transform a tModLoader assembly locally for assisted development. Typically, transformations will not be required to actually load mods in-game, just to compile them.
* Automatic mod resolution.
  * Powered by TML.Patcher, metadata in `build.txt` will be analyzed to download and decompile mods locally for development use, removing the need to distribute these assemblies or worry that others may be unable to collect them quickly.

Unfortunately, these features will prevent compilation if a user is not using the collate toolchain.
