using System.Collections.Generic;
using AnitomySharp;

namespace Jellyfin.Plugin.Resolver.Resolver;

public class AnitomyResult
{
    public string? EpisodeTitle { get; set; }
    public string? EpisodeNumber { get; set; }
    public string? AnimeType { get; set; }

    public int? GetEpisodeNumberAsInt()
    {
        if (EpisodeNumber == null) return null;
        if (!int.TryParse(EpisodeNumber.Split('.')[0], out var episodeNumber)) return null;
        if (episodeNumber == 0) return null;
        return episodeNumber;
    }

    public static AnitomyResult CreateFromElements(IEnumerable<Element> elements)
    {
        var result = new AnitomyResult();

        foreach (var element in elements)
        {
            switch (element.Category)
            {
                case Element.ElementCategory.ElementEpisodeTitle:
                    result.EpisodeTitle = element.Value;
                    break;
                case Element.ElementCategory.ElementEpisodeNumber:
                    result.EpisodeNumber = element.Value;
                    break;
                case Element.ElementCategory.ElementAnimeType:
                    result.AnimeType = element.Value;
                    break;
            }
        }

        return result;
    }
}
