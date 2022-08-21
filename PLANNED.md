Due to how _Collate_ functions, migrating your codebase to _Collate_ will require all contributors to use it.

# Planned Features
* tModLoader assembly transformation.
  * Powered by [rejuvena/whisker](https://github.com/rejuvena/whisker), the _tModLoader_ assembly (as well as the _ReLogic_ assembly, potentially) will be transformed during the initial setup process, opening up possibilities for things such as access transformers.
* Automatic mod dependeny resolution.
  * Powered by [TML.Patcher](https://github.com/Steviegt6/TML.Patcher), metadata in `build.txt` will be analyzed to download and decompile mods locally for development use, removing the need to distribute these assemblies or worry that others may be unable to obtain them swiftly and conveniently.