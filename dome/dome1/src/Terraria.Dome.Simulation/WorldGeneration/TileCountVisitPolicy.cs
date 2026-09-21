using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountVisitPolicy
{
  public static TileCountVisitDecision Evaluate(
    int x,
    int y,
    int worldWidth,
    int worldHeight,
    int wallType,
    bool hasShimmer,
    bool hasLiquid,
    bool jungle,
    bool lavaAllowed,
    bool hasLava)
  {
    if (worldWidth <= 2 || worldHeight <= 2 || x < 0 || x >= worldWidth ||
        y < 0 || y >= worldHeight)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (x <= 1 || x >= worldWidth - 1 || y <= 1 || y >= worldHeight - 1)
    {
      return new TileCountVisitDecision(TileCountVisitRejectionReason.OutsideInterior, false);
    }

    if (wallType == 244)
    {
      return new TileCountVisitDecision(TileCountVisitRejectionReason.ProtectedWall, false);
    }

    if (hasShimmer && hasLiquid)
    {
      return new TileCountVisitDecision(TileCountVisitRejectionReason.ShimmerLiquid, false);
    }

    if (!jungle && wallType != 0)
    {
      return new TileCountVisitDecision(TileCountVisitRejectionReason.Wall, false);
    }

    if (!jungle && hasLava && hasLiquid && !lavaAllowed)
    {
      return new TileCountVisitDecision(TileCountVisitRejectionReason.LavaLiquid, false);
    }

    return new TileCountVisitDecision(TileCountVisitRejectionReason.None,
      !jungle && hasLava && hasLiquid && lavaAllowed);
  }
}
