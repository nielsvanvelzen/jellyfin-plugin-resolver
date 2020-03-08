dotnet build
Copy-Item ".\bin\Debug\netcoreapp3.1\Jellyfin.Plugin.Resolver.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
Copy-Item ".\bin\Debug\netcoreapp3.1\Jellyfin.Plugin.Resolver.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins"