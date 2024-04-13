using System;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Resolver.Resolver;

public class Anitomy(ILogger<Anitomy> logger)
{
    public AnitomyResult? Parse(string fileName)
    {
        try
        {
            var elements = AnitomySharp.AnitomySharp.Parse(fileName);
            return AnitomyResult.CreateFromElements(elements);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to parse filename {fileName}", fileName);
            return null;
        }
    }
}