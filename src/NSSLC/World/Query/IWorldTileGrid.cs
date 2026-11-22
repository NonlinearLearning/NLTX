namespace Terraria.World;

/// <summary>
/// Read-only tile surface that World solvers compute against.
/// </summary>
/// <remarks>
/// The World domain defines this port so pure tile arithmetic does not reach into the dense
/// storage implementation. <see cref="WorldRuntime" /> implements it; solvers only read.
/// </remarks>
public interface IWorldTileGrid
{
  int Width { get; }

  int Height { get; }

  int TileSize { get; }

  /// <summary>
  /// Reads a tile whose coordinate the caller has already clamped to the grid bounds.
  /// </summary>
  WorldTileState ReadTile(WorldTileCoordinate coordinate);
}
