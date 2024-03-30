# Jellyfin Resolver

_Experimental resolver plugin for Jellyfin._

In Jellyfin, resolvers provide the functionality that assigns item types like "series", "season" or "episode" to files and folders. This plugin is an experimental resolver in a
plugin using [Anitomy](https://github.com/erengy/anitomy) to parse file names. Additionally it uses a different folder naming structure to better suit anime watchers.

## Building

Run `dotnet publish` and copy the plugin + anitomysharp dll files to a plugin folder. 

## Current state

The plugin is usable and kept up-to-date with new Jellyfin versions. Keep in mind that the Jellyfin server does not support third-party resolvers. The plugins contains a check to
only activate itself when the path of an item contains "anime" in the name, because there is no way to assign specific resolvers to specific libraries.

## Folder structure

Folders can be nested as much as you want, whenever a folder starts with a digit it is considered a "show". The digit will be stripped for the item name. Seasons are not supported.
All shows have a "Season 1" season with their respective content.

### Definition

- `/[folder]/[order]. [show]/[episode]`
- `/[folder]/[folder]/[order]. [show]/[episode]`

### Examples

- `/Cowboy Bebop/1. Cowboy Bebop/Cowboy Bebop - 01.mkv`
- `/Cowboy Bebop/1. Cowboy Bebop/extra/OP01.mkv`
- `/Cowboy Bebop/2. Cowboy Bebop Tengoku no Tobira/Cowboy Bebop Tengoku no Tobira - 01.mkv`