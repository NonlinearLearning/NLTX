using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldGeneration.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameClassificationQuery
{
  public static TileFrameClassificationKind Classify(WorldTile tile)
  {
    if (!tile.IsActive || tile.IsInactive || tile.LiquidAmount != 0 ||
        tile.IsHalfBrick || tile.Slope != 0)
    {
      return TileFrameClassificationKind.InactiveOrUnsupported;
    }

    if (tile.Type == 520)
    {
      return TileFrameClassificationKind.FoodPlatter;
    }

    if (tile.Type == 80)
    {
      return TileFrameClassificationKind.Cactus;
    }

    if (VineFrameQuery.IsVineTileType(tile.Type))
    {
      return TileFrameClassificationKind.Vine;
    }

    if (TileRopeQuery.IsRopeTileType(tile.Type))
    {
      return TileFrameClassificationKind.Rope;
    }

    if (tile.Type is 21 or 88 or 467)
    {
      return TileFrameClassificationKind.MultiTileUnsupported;
    }

    if (tile.Type is 385 or 446 or 447 or 448)
    {
      return TileFrameClassificationKind.CosmeticNoFrameChange;
    }

    return TileFrameImportantRegistry.Contains(tile.Type)
      ? TileFrameClassificationKind.FrameImportantUnsupported
      : TileFrameClassificationKind.OrdinarySolid;
  }
}
