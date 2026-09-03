using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class BoulderChestProtectionRuleSystem
{
  private const short TileFrameSize = 18;
  private const short BoulderFrameHeight = 36;

  public static bool IsBlocked(
    bool isBoulder,
    bool leftAboveHasBreakabilityBlock,
    bool rightAboveHasBreakabilityBlock)
  {
    return isBoulder &&
           (leftAboveHasBreakabilityBlock || rightAboveHasBreakabilityBlock);
  }

  public static bool TryGetAboveCoordinates(
    WorldTile tile,
    int tileX,
    int tileY,
    out int leftAboveX,
    out int aboveY,
    out int rightAboveX)
  {
    leftAboveX = default;
    aboveY = default;
    rightAboveX = default;
    if (!tile.IsActive || tile.FrameX < 0 || tile.FrameY < 0 ||
        tile.FrameX % TileFrameSize != 0 || tile.FrameY % TileFrameSize != 0)
    {
      return false;
    }

    int frameColumn = tile.FrameX / TileFrameSize;
    int originOffsetX = -frameColumn;
    if (originOffsetX < -1)
    {
      originOffsetX += 2;
    }

    int originX = tileX + originOffsetX;
    int frameRow = tile.FrameY;
    while (frameRow >= BoulderFrameHeight)
    {
      frameRow -= BoulderFrameHeight;
    }

    int originY = tileY - frameRow / TileFrameSize;
    leftAboveX = originX;
    aboveY = originY - 1;
    rightAboveX = originX + 1;
    return true;
  }
}
