using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SkyblockRuleQuery
{
  private const ushort AltarTileType = 26;
  private const ushort FossilTileType = 404;
  private const ushort HellforgeTileType = 77;
  private const ushort HellforgeVariantTileType = 133;
  private const ushort HellstoneTileType = 58;
  private const ushort LifeCrystalTileType = 12;
  private const ushort TempleTileType = 226;
  private const ushort TempleWallType = 87;

  public static SkyblockRuleSnapshot Calculate(
    TilePresenceScanResult scan,
    IReadOnlySet<ushort> dungeonTileTypes,
    IReadOnlySet<ushort> dungeonWallTypes,
    bool isSkyblockWorld)
  {
    ArgumentNullException.ThrowIfNull(scan);
    ArgumentNullException.ThrowIfNull(dungeonTileTypes);
    ArgumentNullException.ThrowIfNull(dungeonWallTypes);
    bool hasDungeon = Intersects(scan.ActiveTileTypes, dungeonTileTypes) ||
      Intersects(scan.WallTypes, dungeonWallTypes);

    return new SkyblockRuleSnapshot(
      NoAltars: !scan.ActiveTileTypes.Contains(AltarTileType),
      NoDungeon: !hasDungeon,
      NoTemple: !scan.ActiveTileTypes.Contains(TempleTileType) &&
        !scan.WallTypes.Contains(TempleWallType),
      NoHellstone: !scan.ActiveTileTypes.Contains(HellstoneTileType),
      NoFossils: !scan.ActiveTileTypes.Contains(FossilTileType),
      NoLifeCrystals: !scan.ActiveTileTypes.Contains(LifeCrystalTileType),
      NoHellforge: !scan.ActiveTileTypes.Contains(HellforgeTileType) &&
        !scan.ActiveTileTypes.Contains(HellforgeVariantTileType),
      LowTiles: isSkyblockWorld && scan.WorldTileCount > 0 &&
        (float)scan.ActiveTileCount / scan.WorldTileCount < 0.1f);
  }

  private static bool Intersects(IReadOnlySet<ushort> first, IReadOnlySet<ushort> second)
  {
    foreach (ushort value in first)
    {
      if (second.Contains(value))
      {
        return true;
      }
    }

    return false;
  }
}
