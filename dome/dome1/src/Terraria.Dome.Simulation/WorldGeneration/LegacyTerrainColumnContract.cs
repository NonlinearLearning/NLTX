using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTerrainColumnMutation(
  int Y,
  bool IsActive,
  ushort? TileType,
  short FrameX,
  short FrameY);

public sealed record LegacyTerrainColumnTile(bool IsActive, ushort TileType);

public static class LegacyTerrainColumnContract
{
  public static IReadOnlyList<LegacyTerrainColumnMutation> PrepareFillColumn(
    int worldHeight,
    double worldSurface,
    double rockLayer)
  {
    if (worldHeight < 0 || !double.IsFinite(worldSurface) || !double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    int surfaceStart = (int)worldSurface;
    List<LegacyTerrainColumnMutation> mutations = new(worldHeight);
    for (int y = 0; (double)y < worldSurface && y < worldHeight; y++)
    {
      mutations.Add(new LegacyTerrainColumnMutation(y, false, null, -1, -1));
    }

    for (int y = Math.Max(0, surfaceStart); y < worldHeight; y++)
    {
      ushort tileType = (double)y < rockLayer ? (ushort)0 : (ushort)1;
      mutations.Add(new LegacyTerrainColumnMutation(y, true, tileType, -1, -1));
    }

    return mutations;
  }

  public static IReadOnlyList<LegacyTerrainColumnMutation> PrepareRetargetColumn(
    int worldHeight,
    double worldSurface,
    IReadOnlyList<LegacyTerrainColumnTile> existingTiles)
  {
    ArgumentNullException.ThrowIfNull(existingTiles);
    if (worldHeight < 0 || !double.IsFinite(worldSurface) ||
        existingTiles.Count != worldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    int surfaceStart = (int)worldSurface;
    List<LegacyTerrainColumnMutation> mutations = new(worldHeight);
    for (int y = 0; (double)y < worldSurface && y < worldHeight; y++)
    {
      mutations.Add(new LegacyTerrainColumnMutation(y, false, null, -1, -1));
    }

    for (int y = Math.Max(0, surfaceStart); y < worldHeight; y++)
    {
      LegacyTerrainColumnTile existingTile = existingTiles[y];
      if (existingTile.TileType != 1 || !existingTile.IsActive)
      {
        mutations.Add(new LegacyTerrainColumnMutation(y, true, 0, -1, -1));
      }
    }

    return mutations;
  }
}
