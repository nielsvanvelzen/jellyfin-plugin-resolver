using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Emby.Naming.Common;
using Jellyfin.Extensions;
using Jellyfin.Plugin.Resolver.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.Resolver.Resolver;

public static class AnimeScanner
{
	public static readonly string[] AnimeTypeThemes = ["ED", "ENDING", "NCED", "NCOP", "OP", "OPENING"];
	public static readonly string[] AnimeTypeTrailer = ["PV", "Teaser", "TRAILER", "CM", "SPOT"];
	public static readonly string[] AnimeTypeInterview = ["INTERVIEW"];

	public enum FileType
	{
		FolderFranchise, // Can be nested
		FolderAnime, // Can not be nested, always has FolderFranchise as parent
		FolderExtra, // Can not be nested, always has FolderAnime as parent

		FileEpisode, // Always has FolderAnime as parent
		FileExtraVideo, // Always has FolderExtra as parent
		FileExtraAudio, // Always has FolderExtra as parent

		Unknown // No clue what this file is
	}

	public static bool SupportsPath(string path) => path.IndexOf("anime", 0, StringComparison.OrdinalIgnoreCase) != -1;

	public static FileType GetFolderType(NamingOptions namingOptions, string path)
	{
		var basename = Path.GetFileName(path);

		if (string.IsNullOrEmpty(basename)) return FileType.Unknown;

		FileType type;

		if (namingOptions.AllExtrasTypesFolderNames.Keys.Contains(basename, StringComparison.InvariantCultureIgnoreCase)) type = FileType.FolderExtra;
		else if (Regex.IsMatch(basename, @"^\d+\.\s")) type = FileType.FolderAnime;
		else type = FileType.FolderFranchise;

		return type;
	}

	public static FileType? GetFileType(NamingOptions namingOptions, string path, string parentPath, bool isDirectory)
	{
		var type = FileType.Unknown;

		if (isDirectory)
		{
			type = GetFolderType(namingOptions, path);
		}
		else
		{
			var parentType = GetFolderType(namingOptions, parentPath);
			var extension = Path.GetExtension(path);
			var isVideo = namingOptions.VideoFileExtensions.Contains(extension);
			var isAudio = namingOptions.AudioFileExtensions.Contains(extension);

			if (isVideo && parentType == FileType.FolderAnime) type = FileType.FileEpisode;
			else if (isVideo && parentType == FileType.FolderExtra) type = FileType.FileExtraVideo;
			else if (isAudio && parentType == FileType.FolderExtra) type = FileType.FileExtraAudio;
		}

		return type;
	}

	public static string GetAnimeFolderName(string name) => Regex.Replace(name, @"^\d+\.\s", "");
	public static int GetAnimeFolderIndex(string name) => int.Parse(Regex.Match(name, @"^(\d+)\.\s").Groups[1].Value);

	public static ExtraType GetExtraType(Anitomy anitomy)
	{
		var animeType = anitomy.AnimeType ?? string.Empty;

		if (AnimeTypeThemes.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.ThemeVideo;
		if (AnimeTypeTrailer.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.Trailer;
		if (AnimeTypeInterview.Contains(animeType, StringComparison.InvariantCultureIgnoreCase)) return ExtraType.Interview;

		return ExtraType.Unknown;
	}

	public static void ApplyVideoMetadata(Video video, FileType type)
	{
		var fileName = Path.GetFileName(video.Path);
		var anitomy = new Anitomy(fileName);
		video.SortName = fileName;
		video.Name = fileName;
		video.ForcedSortName = fileName;

		// Set episode metadata
		if (type == FileType.FileEpisode)
		{
			// Set as "first season" item
			video.ParentIndexNumber = 1;

			// Set name
			if (anitomy.EpisodeTitle != null) video.Name = anitomy.EpisodeTitle;
			// else if (episodeNumber != null) video.Name = $"Episode {episodeNumber}";

			// Set index
			var episodeNumber = anitomy.GetEpisodeNumberAsInt();
			if (episodeNumber != null) video.IndexNumber = episodeNumber;
		}

		// Set extra metadata
		if (type == FileType.FileExtraVideo)
		{
			video.ExtraType = GetExtraType(anitomy);
		}
	}

	public static void ApplyAudioMetadata(Audio audio, FileType type)
	{
		var fileName = Path.GetFileName(audio.Path);
		var anitomy = new Anitomy(fileName);
		audio.SortName = fileName;
		audio.Name = fileName;
		audio.ForcedSortName = fileName;

		// Set extra metadata
		if (type == FileType.FileExtraAudio)
		{
			audio.ExtraType = GetExtraType(anitomy);
		}
	}
}