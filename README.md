# Jellyfin Resolver
_A proper name should still be considered._

Jellyfin resolvers provide the functionality that assigns proper types like "season" or "episode" to files and folders.
This plugin adds a new resolver to Jellyfin that uses [Anitomy](https://github.com/erengy/anitomy) to parse files. Additionally the way the folder structure is defined is changed to better suit anime watchers.

## Current state
The plugin is usable to a certain degree. Unfortunately the Jellyfin server doesn't support alternative resolvers properly so this plugin needs to do some hacks to get it working:
- It defines a high priority so it will run before most of the other build-in resolvers
- It check if the library it is resolving contains "anime" in the name because there is no way to assign it to certain libraries only (yet)
- It **always** returns something so other resolvers won't provide false information
- Only works for mp4 and mkv files at this moment

A issue for custom resolvers exists in the Jellyfin issue tracker: [jellyfin/#2187](https://github.com/jellyfin/jellyfin/issues/2187)

## Folder structure
Folders can be nested as much as you want, whenever a folder starts with a digit it is considered a "show". The digit will be stripped.
Season support is currently not supported. All shows will get a "Season 1" season with their respective content.
Folders called "extra" inside a show will be added as a separate season.

### Definition
- `/[folder]/[order]. [show]/[episode]`
- `/[folder]/[folder]/[order]. [show]/[episode]`

### Examples
- `/Cowboy Bebop/1. Cowboy Bebop/Cowboy Bebop - 01.mkv`
- `/Cowboy Bebop/1. Cowboy Bebop/extra/OP01.mkv`
- `/Cowboy Bebop/2. Cowboy Bebop Tengoku no Tobira/Cowboy Bebop Tengoku no Tobira - 01.mkv`