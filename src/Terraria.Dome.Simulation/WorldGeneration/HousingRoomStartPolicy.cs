using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingRoomStartPolicy
{
  private const int EdgeMargin = 10;

  public static HousingRoomStartDecision Evaluate(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    bool isActiveSolid)
  {
    if (worldWidth <= EdgeMargin * 2 || worldHeight <= EdgeMargin * 2 ||
        x < 0 || x >= worldWidth || y < 0 || y >= worldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (x < EdgeMargin || y < EdgeMargin || x >= worldWidth - EdgeMargin ||
        y >= worldHeight - EdgeMargin)
    {
      return new HousingRoomStartDecision(
        HousingRoomStartRejectionReason.TooCloseToWorldEdge,
        false);
    }

    return isActiveSolid
      ? new HousingRoomStartDecision(
        HousingRoomStartRejectionReason.StartedInSolidTile,
        false)
      : new HousingRoomStartDecision(HousingRoomStartRejectionReason.None, true);
  }
}
