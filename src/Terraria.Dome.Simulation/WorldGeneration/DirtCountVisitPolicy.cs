using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DirtCountVisitPolicy
{
  public static DirtCountVisitDecision Evaluate(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    bool isActive,
    ushort tileType,
    ushort wallType,
    bool isSolid)
  {
    if (worldWidth <= 2 || worldHeight <= 2 || x < 0 || x >= worldWidth ||
        y < 0 || y >= worldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (x <= 1 || x >= worldWidth - 1 || y <= 1 || y >= worldHeight - 1)
    {
      return Reject(DirtCountRejectionReason.OutsideInterior);
    }

    if (isActive && tileType is 147 or 161)
    {
      return Reject(DirtCountRejectionReason.IceTile);
    }

    if (wallType is 244 or 83 or 3 or 187 or 216)
    {
      return Reject(DirtCountRejectionReason.ProtectedWall);
    }

    if (isSolid)
    {
      return Reject(DirtCountRejectionReason.SolidTile);
    }

    return wallType is 2 or 59
      ? new DirtCountVisitDecision(DirtCountRejectionReason.None, true)
      : Reject(DirtCountRejectionReason.UnsupportedWall);
  }

  private static DirtCountVisitDecision Reject(DirtCountRejectionReason reason)
  {
    return new DirtCountVisitDecision(reason, false);
  }
}
