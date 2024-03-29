using AnitomySharp;

namespace Jellyfin.Plugin.Resolver.Api
{
	public class Anitomy
	{
		public string EpisodeTitle { get; set; }
		public string EpisodeNumber { get; set; }
		public string AnimeType { get; set; }

		public Anitomy()
		{
		}

		public Anitomy(string filename)
		{
			CreateFromFileName(filename);
		}

		public int? GetEpisodeNumberAsInt()
		{
			if (EpisodeNumber == null) return null;
			int.TryParse(EpisodeNumber.Split('.')[0], out var episodeNumber);
			if (episodeNumber == 0) return null;
			return episodeNumber;
		}

		private void CreateFromFileName(string filename)
		{
			var elements = AnitomySharp.AnitomySharp.Parse(filename);

			foreach (var element in elements)
			{
				switch (element.Category)
				{
					case Element.ElementCategory.ElementEpisodeTitle:
						EpisodeTitle = element.Value;
						break;
					case Element.ElementCategory.ElementEpisodeNumber:
						EpisodeNumber = element.Value;
						break;
					case Element.ElementCategory.ElementAnimeType:
						AnimeType = element.Value;
						break;
				}
			}
		}
	}
}