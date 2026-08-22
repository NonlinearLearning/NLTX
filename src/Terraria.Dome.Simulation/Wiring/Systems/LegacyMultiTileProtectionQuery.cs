using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyMultiTileProtectionQuery
{
  private const ushort MultiTileType = 235;
  private const short FrameWidth = 54;
  private const short FrameCellSize = 18;

  public static bool TryGetIsBlocked(
    WorldGrid world,
    int tileX,
    int tileY,
    bool isHardMode,
    out bool isBlocked)
  {
    ArgumentNullException.ThrowIfNull(world);
    isBlocked = default;
    if (!world.Contains(tileX, tileY))
    {
      return false;
    }

    WorldTile tile = world.GetTile(tileX, tileY);
    if (!tile.IsActive || tile.Type != MultiTileType || tile.FrameX < 0 ||
        tile.FrameX % FrameCellSize != 0)
    {
      return false;
    }

    int originX = tileX - tile.FrameX % FrameWidth / FrameCellSize;
    int aboveY = tileY - 1;
    if (!world.Contains(originX, aboveY) || !world.Contains(originX + 2, aboveY))
    {
      return false;
    }

    WorldTile[] aboveTiles =
    [
      world.GetTile(originX, aboveY),
      world.GetTile(originX + 1, aboveY),
      world.GetTile(originX + 2, aboveY)
    ];
    isBlocked = MultiTileProtectionRuleSystem.IsBlocked(tile.Type, aboveTiles, isHardMode);
    return true;
  }
}
