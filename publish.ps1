dotnet build
Copy-Item ".\bin\Debug\netstandard2.1\Jellyfin.Plugin.Resolver.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
Copy-Item ".\bin\Debug\netstandard2.1\Jellyfin.Plugin.Resolver.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins"

Copy-Item ".\bin\Debug\netstandard2.1\AnitomySharp.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins"
Copy-Item ".\bin\Debug\netstandard2.1\AnitomySharp.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins"