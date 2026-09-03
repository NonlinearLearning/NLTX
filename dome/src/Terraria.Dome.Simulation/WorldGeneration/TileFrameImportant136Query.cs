using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameImportant136Query
{
  private const ushort TileType = 136;

  public static TileFrameImportant136Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    return Evaluate(
      snapshot,
      tileDefinitions,
      x,
      y,
      BeamTileRegistry.RegisterDefaults(),
      TreeTrunkTileRegistry.RegisterDefaults());
  }

  public static TileFrameImportant136Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlySet<ushort> beamTileTypes,
    IReadOnlySet<ushort> treeTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(beamTileTypes);
    ArgumentNullException.ThrowIfNull(treeTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive || source.Type != TileType)
    {
      return new TileFrameImportant136Result(false, source.FrameX, false);
    }

    WorldTile down = GetTile(snapshot, x, y + 1);
    if (IsDownSupport(down, tileDefinitions))
    {
      return new TileFrameImportant136Result(true, 0, false);
    }

    WorldTile left = GetTile(snapshot, x - 1, y);
    WorldTile right = GetTile(snapshot, x + 1, y);
    WorldTile downLeft = GetTile(snapshot, x - 1, y + 1);
    WorldTile downRight = GetTile(snapshot, x + 1, y + 1);
    WorldTile upLeft = GetTile(snapshot, x - 1, y - 1);
    WorldTile upRight = GetTile(snapshot, x + 1, y - 1);

    if (IsSideSupport(left, tileDefinitions, leftSlope: true) ||
        IsBeam(left, beamTileTypes) ||
        IsTreeBridge(left, downLeft, upLeft, treeTileTypes))
    {
      return new TileFrameImportant136Result(true, 18, false);
    }

    if (IsSideSupport(right, tileDefinitions, leftSlope: false) ||
        IsBeam(right, beamTileTypes) ||
        IsTreeBridge(right, downRight, upRight, treeTileTypes))
    {
      return new TileFrameImportant136Result(true, 36, false);
    }

    return source.WallType > 0
      ? new TileFrameImportant136Result(true, 54, false)
      : new TileFrameImportant136Result(false, source.FrameX, true);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }

  private static bool IsDownSupport(WorldTile tile, TileDefinitionRegistry definitions)
  {
    return IsSolidAttached(tile, definitions) && tile.Slope is 0 or 3 or 4;
  }

  private static bool IsSideSupport(
    WorldTile tile,
    TileDefinitionRegistry definitions,
    bool leftSlope)
  {
    if (!IsSolidAttached(tile, definitions) || tile.IsHalfBrick)
    {
      return false;
    }

    return leftSlope
      ? tile.Slope is 0 or 1 or 3
      : tile.Slope is 0 or 2 or 4;
  }

  private static bool IsSolidAttached(WorldTile tile, TileDefinitionRegistry definitions)
  {
    return tile.IsActive && definitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsNoAttach;
  }

  private static bool IsBeam(WorldTile tile, IReadOnlySet<ushort> beamTileTypes)
  {
    return tile.IsActive && beamTileTypes.Contains(tile.Type);
  }

  private static bool IsTreeBridge(
    WorldTile side,
    WorldTile diagonalDown,
    WorldTile diagonalUp,
    IReadOnlySet<ushort> treeTileTypes)
  {
    return side.IsActive && diagonalDown.IsActive && diagonalUp.IsActive &&
      treeTileTypes.Contains(side.Type) && treeTileTypes.Contains(diagonalDown.Type) &&
      treeTileTypes.Contains(diagonalUp.Type);
  }
}
