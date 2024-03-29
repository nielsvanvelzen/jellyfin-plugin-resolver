dotnet publish
# Windows Tray
#$jellyfin = "C:\ProgramData\Jellyfin\Server"
# dotnet run
$jellyfin = "C:\Users\Niels\AppData\Local\jellyfin"
New-Item -ItemType Directory -Force -Path "$jellyfin\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\Jellyfin.Plugin.Resolver.dll" -Destination "$jellyfin\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\Jellyfin.Plugin.Resolver.pdb" -Destination "$jellyfin\plugins\Resolver_local\"
Copy-Item ".\bin\Release\net8.0\publish\AnitomySharp.dll" -Destination "$jellyfin\plugins\Resolver_local\"
