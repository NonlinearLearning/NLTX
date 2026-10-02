namespace Terraria.WorldSession.Session;

public readonly record struct WorldCatalogEntry(string Name, string Path, int WorldId)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Path);
}
