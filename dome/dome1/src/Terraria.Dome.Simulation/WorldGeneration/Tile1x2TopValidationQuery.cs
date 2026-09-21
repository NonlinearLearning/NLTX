using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile1x2TopValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int TileFrameBandHeight = 36;
  private const ushort SpecialPlatformType = 380;
  private static readonly IReadOnlySet<ushort> PlatformSupportTileTypes = new HashSet<ushort>
  {
    42, 270, 271, 572, 581, 660, 698
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterPlatformSupportDefaults()
  {
    return PlatformSupportTileTypes;
  }

  public static Tile1x2TopValidationResult Evaluate(
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
    int frameRow = source.FrameY / TileFrameWidth;
    int styleBand = frameRow / 2;
    int originY = y - frameRow % 2;
    WorldTile top = GetTile(snapshot, x, originY);
    WorldTile bottom = GetTile(snapshot, x, originY + 1);
    bool valid = top.IsActive && bottom.IsActive &&
      top.Type == tileType && bottom.Type == tileType &&
      top.FrameY == checked((short)(styleBand * TileFrameBandHeight)) &&
      bottom.FrameY == checked((short)(styleBand * TileFrameBandHeight + TileFrameWidth));

    WorldTile support = GetTile(snapshot, x, originY - 1);
    bool usesPlatformSupport = PlatformSupportTileTypes.Contains(tileType);
    bool usesRopeSupport = tileType == 698;
    bool platform = support.IsActive &&
      (IsPlatform(tileDefinitions, support.Type) || support.Type == SpecialPlatformType);
    bool rope = support.IsActive && IsRope(support.Type);
    bool solid = TileStateQuery.IsSolidAllowingTopSlope(support, tileDefinitions);
    bool hasSupport = (usesPlatformSupport && platform) ||
      (usesRopeSupport && rope) ||
      (!usesPlatformSupport && solid);
    valid &= hasSupport;

    return new Tile1x2TopValidationResult(
      valid,
      !valid,
      originY,
      styleBand,
      usesPlatformSupport && platform,
      usesRopeSupport && rope);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }

  private static bool IsPlatform(TileDefinitionRegistry definitions, ushort type)
  {
    return definitions.TryGet(type, out TileDefinition definition) && definition.IsPlatform;
  }

  private static bool IsRope(ushort type)
  {
    return TileRopeQuery.IsRopeTileType(type);
  }
}
