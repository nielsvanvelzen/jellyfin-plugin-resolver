dotnet publish
New-Item -ItemType Directory -Force -Path "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Debug\net6.0\Jellyfin.Plugin.Resolver.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Debug\net6.0\Jellyfin.Plugin.Resolver.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Debug\net6.0\AnitomySharp.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Debug\net6.0\AnitomySharp.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
