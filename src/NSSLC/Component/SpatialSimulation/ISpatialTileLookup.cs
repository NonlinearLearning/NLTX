namespace Terraria.SpatialSimulation;

/// <summary>Resolves one tile's captured geometry facts at a time.</summary>
public interface ISpatialTileLookup
{
  bool TryGetTile(int x, int y, out SpatialTileSnapshot tile);
}
