dotnet build
Copy-Item ".\bin\Debug\net5.0\Jellyfin.Plugin.Resolver.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
Copy-Item ".\bin\Debug\net5.0\Jellyfin.Plugin.Resolver.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins"

Copy-Item ".\bin\Debug\net5.0\AnitomySharp.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
Copy-Item ".\bin\Debug\net5.0\AnitomySharp.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
