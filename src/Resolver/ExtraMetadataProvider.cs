﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emby.Naming.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Resolver.Resolver;

public class ExtraMetadataProvider(
	ILogger<AnimeEpisodeResolver> logger,
	NamingOptions namingOptions
) : ICustomMetadataProvider<Video>
{
	public string Name => "ExtraMetadataProvider";

	public Task<ItemUpdateType> FetchAsync(Video item, MetadataRefreshOptions options, CancellationToken cancellationToken)
	{
		// Not a supported path
		if (!AnimeScanner.SupportsPath(item.Path)) return Task.FromResult(ItemUpdateType.None);

		var type = AnimeScanner.GetFileType(namingOptions, item.Path, Path.GetDirectoryName(item.Path), false);
		logger.LogDebug($"{item.Path} is {type}");

		// Not an extra
		if (type != AnimeScanner.FileType.FileExtra) return Task.FromResult(ItemUpdateType.None);

		// Apply metadata
		AnimeScanner.ApplyVideoMetadata(item, AnimeScanner.FileType.FileExtra);
		logger.LogDebug($"{item.Path} is {item.ExtraType}");
		
		// Return as edit
		return Task.FromResult(ItemUpdateType.MetadataEdit);
	}
}