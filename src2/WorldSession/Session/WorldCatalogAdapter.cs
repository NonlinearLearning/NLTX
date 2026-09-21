namespace Terraria.WorldSession.Session;

public sealed class WorldCatalogAdapter
{
  public WorldCatalogProjection CreateProjection(IEnumerable<WorldCatalogEntry> source)
  {
    ArgumentNullException.ThrowIfNull(source);
    WorldCatalogEntry[] entries = source
      .Where(entry => entry.IsValid)
      .OrderBy(entry => entry.Name, StringComparer.Ordinal)
      .ThenBy(entry => entry.WorldId)
      .ToArray();
    return new WorldCatalogProjection(entries);
  }
}
