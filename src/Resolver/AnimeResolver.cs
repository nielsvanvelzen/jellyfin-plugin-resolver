using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Emby.Naming.Common;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using Jellyfin.Plugin.Resolver.Api;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Resolver.Resolver
{
	enum FileType
	{
		FolderFranchise, // Can be nested
		FolderAnime, // Can not be nested, always has franchise as parent
		FolderExtra, // Can not be nested, always has anime as parent

		FileEpisode, // Always has anime as parent
		FileExtra, // Always has extra as parent

		Unknown // No clue what this file is
	}

	public class AnimeEpisodeResolver : IItemResolver, IMultiItemResolver
	{
		private static readonly string[] AnimeTypeThemes = ["ED", "ENDING", "NCED", "NCOP", "OP", "OPENING"];
		private static readonly string[] AnimeTypeTrailer = ["PV", "Teaser", "TRAILER", "CM", "SPOT"];
		private static readonly string[] AnimeTypeInterview = ["INTERVIEW"];

		public ResolverPriority Priority => ResolverPriority.Plugin;
		private readonly ILogger<AnimeEpisodeResolver> _logger;
		private readonly IServerApplicationPaths _appPaths;
		private readonly ILibraryManager _libraryManager;
		private readonly NamingOptions _namingOptions;

		public AnimeEpisodeResolver(ILogger<AnimeEpisodeResolver> logger, IServerApplicationPaths appPaths, ILibraryManager libraryManager, NamingOptions namingOptions)
		{
			_logger = logger;
			_appPaths = appPaths;
			_libraryManager = libraryManager;
			_namingOptions = namingOptions;
		}

		private FileType GetFolderType(string path)
		{
			var basename = Path.GetFileName(path);

			if (string.IsNullOrEmpty(basename)) return FileType.Unknown;

			FileType type;

			if (_namingOptions.AllExtrasTypesFolderNames.Keys.Contains(basename, StringComparison.InvariantCultureIgnoreCase)) type = FileType.FolderExtra;
			else if (Regex.IsMatch(basename, @"^\d+\.\s")) type = FileType.FolderAnime;
			else type = FileType.FolderFranchise;

			return type;
		}

		private FileType? GetFileType(string path, string parentPath, bool isDirectory)
		{
			var type = FileType.Unknown;

			if (isDirectory)
			{
				type = GetFolderType(path);
			}
			else
			{
				var parentType = GetFolderType(parentPath);
				var extension = Path.GetExtension(path);
				var isVideo = _namingOptions.VideoFileExtensions.Contains(extension);

				if (isVideo && parentType == FileType.FolderExtra) type = FileType.FileExtra;
				else if (isVideo && parentType == FileType.FolderAnime) type = FileType.FileEpisode;
			}

			return type;
		}

		private static ExtraType GetExtraType(Anitomy anitomy)
		{
			var animeType = anitomy.AnimeType ?? string.Empty;

			if (AnimeTypeThemes.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.ThemeVideo;
			if (AnimeTypeTrailer.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.Trailer;
			if (AnimeTypeInterview.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.Interview;

			return ExtraType.Unknown;
		}

		public BaseItem ResolvePath(ItemResolveArgs args)
		{
			// Only for tv shows folders
			// Empty collection type is "programdata" folder
			if (args.CollectionType is not CollectionType.tvshows)
			{
				return null;
			}

			// Only enable for anime libraries (todo: better detection)
			if (args.Path.IndexOf("anime", 0, StringComparison.OrdinalIgnoreCase) == -1) return null;

			var type = GetFileType(args.Path, args.Parent.Path, args.IsDirectory);
			_logger.LogDebug($"{args.Path} is {type}");

			if (type is FileType.FolderFranchise or FileType.FolderExtra)
			{
				return new Folder
				{
					Path = args.Path,
					Name = args.FileInfo.Name
				};
			}

			if (type is FileType.FolderAnime)
			{
				var name = Regex.Replace(args.FileInfo.Name, @"^\d+\.\s", "");

				return new Series
				{
					Path = args.Path,
					Name = name,
					SortName = name,
					ForcedSortName = name,
					IndexNumber = int.Parse(Regex.Match(args.FileInfo.Name, @"^(\d+)\.\s").Groups[1].Value)
				};
			}

			if (type is FileType.FileEpisode or FileType.FileExtra)
			{
				var anitomy = new Anitomy(args.FileInfo.Name);
				var video = type == FileType.FileEpisode ? new Episode() : new Video();
				video.Path = args.Path;
				video.SortName = args.FileInfo.Name;
				video.Name = args.FileInfo.Name;
				video.ForcedSortName = args.FileInfo.Name;
				video.ParentIndexNumber = 1; // Set as "first season" item

				// Set episode metadata
				if (type == FileType.FileEpisode)
				{
					// Set name
					if (anitomy.EpisodeTitle != null) video.Name = anitomy.EpisodeTitle;
					// else if (episodeNumber != null) video.Name = $"Episode {episodeNumber}";

					// Set index
					var episodeNumber = anitomy.GetEpisodeNumberAsInt();
					if (episodeNumber != null) video.IndexNumber = episodeNumber;
				}

				// Set extra metadata
				if (type == FileType.FileExtra)
				{
					video.ExtraType = GetExtraType(anitomy);

					// TODO: Extras are hard-refreshed after a scan in the BaseItem.RefreshExtras method using the (hardcoded) ExtraResolver
					// returning an extra here will create it, but it won't be bound to the series item so it's quite useless
					// For now, return null and let the server deal with it
					return null;
				}

				return video;
			}

			return null;
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
				var item = ResolvePath(new ItemResolveArgs(_appPaths, _libraryManager)
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

		public MultiItemResolverResult ResolveMultiple(Folder parent, List<FileSystemMetadata> files, CollectionType? collectionType, IDirectoryService directoryService)
		{
			if (collectionType is not CollectionType.tvshows) return new MultiItemResolverResult();

			// Only enable for anime libraries (todo: better detection)
			if (parent.Path.IndexOf("anime", 0, StringComparison.OrdinalIgnoreCase) == -1) return new MultiItemResolverResult();

			return ResolveVideos(parent, files, collectionType.Value);
		}
	}
}