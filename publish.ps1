dotnet publish
New-Item -ItemType Directory -Force -Path "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\Jellyfin.Plugin.Resolver.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\Jellyfin.Plugin.Resolver.pdb" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\AnitomySharp.dll" -Destination "C:\ProgramData\Jellyfin\Server\plugins\Resolver_local\"
