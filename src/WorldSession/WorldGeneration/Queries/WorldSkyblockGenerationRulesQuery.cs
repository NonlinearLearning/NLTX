using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

public static class WorldSkyblockGenerationRulesQuery
{
  private const ushort AltarTileType = 26;
  private const ushort FossilTileType = 404;
  private const ushort HellforgeTileType = 77;
  private const ushort HellforgeVariantTileType = 133;
  private const ushort HellstoneTileType = 58;
  private const ushort LifeCrystalTileType = 12;
  private const ushort TempleTileType = 226;
  private const ushort TempleWallType = 87;

  public static WorldSkyblockGenerationRulesSelection Evaluate(
    WorldSkyblockGenerationRulesInput input)
  {
    if (!input.SkyblockWorld)
    {
      return new WorldSkyblockGenerationRulesSelection(
        NoAltars: false,
        NoDungeon: false,
        NoTemple: false,
        NoHellstone: false,
        NoFossils: false,
        NoLifeCrystals: false,
        NoHellforge: false,
        LowTiles: false,
        GenerationId: input.Scan.GenerationId,
        ScanVersion: input.Scan.ScanVersion);
    }

    bool hasDungeon = Intersects(
      input.Scan.ActiveTileTypes,
      input.DungeonTileTypes) ||
      Intersects(input.Scan.WallTypes, input.DungeonWallTypes);
    bool lowTiles = input.SkyblockWorld &&
      input.Scan.WorldTileCount > 0 &&
      (float)input.Scan.CurrentActiveTiles / input.Scan.WorldTileCount < 0.1f;

    return new WorldSkyblockGenerationRulesSelection(
      !input.Scan.HasTile(AltarTileType),
      !hasDungeon,
      !input.Scan.HasTile(TempleTileType) &&
        !input.Scan.HasWall(TempleWallType),
      !input.Scan.HasTile(HellstoneTileType),
      !input.Scan.HasTile(FossilTileType),
      !input.Scan.HasTile(LifeCrystalTileType),
      !input.Scan.HasTile(HellforgeTileType) &&
        !input.Scan.HasTile(HellforgeVariantTileType),
      lowTiles,
      input.Scan.GenerationId,
      input.Scan.ScanVersion);
  }

  private static bool Intersects<T>(
    System.Collections.Generic.IReadOnlySet<T> first,
    System.Collections.Generic.IReadOnlySet<T> second)
    where T : notnull
  {
    foreach (T value in first)
    {
      if (second.Contains(value))
      {
        return true;
      }
    }

    return false;
  }
}
