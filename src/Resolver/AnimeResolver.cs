using System;
using System.Collections.Generic;
using Emby.Naming.Common;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Resolver.Resolver;

public class AnimeEpisodeResolver(
	ILogger<AnimeEpisodeResolver> logger,
	IServerApplicationPaths appPaths,
	ILibraryManager libraryManager,
	NamingOptions namingOptions
) : IItemResolver, IMultiItemResolver
{
	public ResolverPriority Priority => ResolverPriority.Plugin;

	public BaseItem ResolvePath(ItemResolveArgs args)
	{
		// Only for tv shows folders
		// Empty collection type is "programdata" folder
		if (args.CollectionType is not CollectionType.tvshows) return null;

		// Only enable for anime libraries
		if (!AnimeScanner.SupportsPath(args.Path)) return null;

		var type = AnimeScanner.GetFileType(namingOptions, args.Path, args.Parent.Path, args.IsDirectory);
		logger.LogDebug($"{args.Path} is {type}");

		if (type is AnimeScanner.FileType.FolderFranchise or AnimeScanner.FileType.FolderExtra)
		{
			return new Folder
			{
				Path = args.Path,
				Name = args.FileInfo.Name
			};
		}

		if (type is AnimeScanner.FileType.FolderAnime)
		{
			var name = AnimeScanner.GetAnimeFolderName(args.FileInfo.Name);

			return new Series
			{
				Path = args.Path,
				Name = name,
				SortName = name,
				ForcedSortName = name,
				IndexNumber = AnimeScanner.GetAnimeFolderIndex(args.FileInfo.Name)
			};
		}

		if (type is AnimeScanner.FileType.FileEpisode)
		{
			var video = type == AnimeScanner.FileType.FileEpisode ? new Episode() : new Video();
			video.Path = args.Path;

			AnimeScanner.ApplyVideoMetadata(video, type.Value);
			return video;
		}
		
		// Note: FileExtra* is handled in ExtraMetadataProvider

		return null;
	}

	public MultiItemResolverResult ResolveMultiple(Folder parent, List<FileSystemMetadata> files, CollectionType? collectionType, IDirectoryService directoryService)
	{
		// Only for tv shows folders
		// Empty collection type is "programdata" folder
		if (collectionType is not CollectionType.tvshows) return new MultiItemResolverResult();

		// Only enable for anime libraries
		if (!AnimeScanner.SupportsPath(parent.Path)) return new MultiItemResolverResult();

		return ResolveVideos(parent, files, collectionType.Value);
	}

	private MultiItemResolverResult ResolveVideos(
		Folder parent,
		IEnumerable<FileSystemMetadata> fileSystemEntries,
		CollectionType collectionType
	)
	{
		var files = new List<FileSystemMetadata>();
		var leftOver = new List<FileSystemMetadata>();

		// Loop through each child file/folder and see if we find a video
		foreach (var child in fileSystemEntries)
		{
			if (child.IsDirectory)
			{
				leftOver.Add(child);
			}
			else
			{
				files.Add(child);
			}
		}

		var items = new List<BaseItem>();

		foreach (var file in files)
		{
			var item = ResolvePath(new ItemResolveArgs(appPaths, libraryManager)
			{
				Parent = parent,
				CollectionType = collectionType,
				FileInfo = file,
				FileSystemChildren = Array.Empty<FileSystemMetadata>(),
				LibraryOptions = new LibraryOptions()
			});
			if (item == null) leftOver.Add(file);
			else items.Add(item);
		}

		return new MultiItemResolverResult
		{
			ExtraFiles = leftOver,
			Items = items,
		};
	}
}