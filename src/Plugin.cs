using System;
using Jellyfin.Plugin.Resolver.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Resolver;

public class Plugin(
	IApplicationPaths applicationPaths,
	IXmlSerializer xmlSerializer
) : BasePlugin<PluginConfiguration>(applicationPaths, xmlSerializer)
{
	public override string Name => "Resolver Plugin";
	public override Guid Id => Guid.Parse("057ee128-f2e4-4c0c-b5e1-b3c7adeb159d");
}