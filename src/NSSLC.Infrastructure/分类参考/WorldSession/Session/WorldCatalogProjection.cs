namespace Terraria.WorldSession.Session;

public sealed class WorldCatalogProjection
{
  public WorldCatalogProjection(IEnumerable<WorldCatalogEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    Entries = entries.ToArray();
  }

  public IReadOnlyList<WorldCatalogEntry> Entries { get; }
}
