namespace Terraria.World;

/// <summary>Read-only tile snapshot port for world interaction consumers.</summary>
/// <remarks>Implementations must read the authoritative world state without exposing storage.</remarks>
public interface IWorldTileReader
{
  bool TryReadTile(WorldTileCoordinate coordinate, out WorldTileState tile);
}
