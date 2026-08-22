using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileWiringQuery
{
  private const ushort MinecartPressurePlateTileType = 314;
  private const ushort SpecialPressurePlateTileType = 467;
  private const int SpecialPressurePlateFrameColumn = 4;
  private const int TileFrameWidth = 36;

  public static bool IsItATrap(
    WorldTile tile,
    IReadOnlyDictionary<ushort, TileWiringClassificationDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (!tile.IsActive)
    {
      return false;
    }

    if (tile.IsActuated)
    {
      return true;
    }

    return definitions.TryGetValue(tile.Type, out TileWiringClassificationDefinition definition) &&
      definition.IsMechanism && !definition.IgnoreWhenValidatingTraps;
  }

  public static bool IsItATrigger(
    WorldTile tile,
    IReadOnlyDictionary<ushort, TileWiringClassificationDefinition> definitions,
    bool isPressurePlate = false)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (!tile.IsActive)
    {
      return false;
    }

    if (definitions.TryGetValue(tile.Type, out TileWiringClassificationDefinition definition) &&
        definition.IsTrigger)
    {
      return true;
    }

    if (tile.Type == SpecialPressurePlateTileType &&
        tile.FrameX / TileFrameWidth == SpecialPressurePlateFrameColumn)
    {
      return true;
    }

    return tile.Type == MinecartPressurePlateTileType && isPressurePlate;
  }
}
