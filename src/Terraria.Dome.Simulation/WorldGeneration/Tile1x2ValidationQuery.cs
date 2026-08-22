using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile1x2ValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int TileFrameHeight = 40;

  public static Tile1x2ValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameBand = source.FrameY / TileFrameHeight;
    int frameOffset = source.FrameY % TileFrameHeight;
    int originY = y - (frameOffset == TileFrameWidth ? 1 : 0);
    bool valid = frameOffset is 0 or TileFrameWidth;
    WorldTile top = GetTile(snapshot, x, originY);
    WorldTile bottom = GetTile(snapshot, x, originY + 1);
    valid &= top.IsActive && bottom.IsActive &&
      top.Type == tileType && bottom.Type == tileType &&
      top.FrameY == checked((short)(frameBand * TileFrameHeight)) &&
      bottom.FrameY == checked((short)(frameBand * TileFrameHeight + TileFrameWidth));

    WorldTile support = GetTile(snapshot, x, originY + 2);
    bool usesPlatformSupport = support.IsActive && IsPlatform(tileDefinitions, support.Type);
    bool hasSupport = usesPlatformSupport ||
      TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        x,
        originY + 2);
    valid &= hasSupport;

    return new Tile1x2ValidationResult(
      valid,
      !valid,
      originY,
      frameBand,
      usesPlatformSupport);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }

  private static bool IsPlatform(TileDefinitionRegistry definitions, ushort tileType)
  {
    return definitions.TryGet(tileType, out TileDefinition definition) && definition.IsPlatform;
  }
}
