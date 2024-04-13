using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emby.Naming.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Resolver.Resolver;

public class ExtraAudioMetadataProvider(
	ILogger<ExtraAudioMetadataProvider> logger,
	NamingOptions namingOptions,
	AnimeScanner animeScanner
) : ICustomMetadataProvider<Audio>
{
	public string Name => nameof(ExtraAudioMetadataProvider);

	public Task<ItemUpdateType> FetchAsync(Audio item, MetadataRefreshOptions options, CancellationToken cancellationToken)
	{
		// Not a supported path
		if (!AnimeScanner.SupportsPath(item.Path)) return Task.FromResult(ItemUpdateType.None);

		var type = AnimeScanner.GetFileType(namingOptions, item.Path, Path.GetDirectoryName(item.Path), false);
		logger.LogDebug($"{item.Path} is {type}");

		// Not an extra
		if (type != AnimeScanner.FileType.FileExtraAudio) return Task.FromResult(ItemUpdateType.None);

		// Apply metadata
		animeScanner.ApplyAudioMetadata(item, AnimeScanner.FileType.FileExtraAudio);
		logger.LogDebug($"{item.Path} is {item.ExtraType}");
		
		// Return as edit
		return Task.FromResult(ItemUpdateType.MetadataEdit);
	}
}