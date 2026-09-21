using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionTownFallbackGuardSystem
{
  private const double SpawnWindowPixels = 3000.0;

  public bool IsEligible(
    double invasionX,
    int maxTilesX,
    double townNpcCenterX,
    double positionX,
    bool isTownNpc)
  {
    if (!isTownNpc || maxTilesX <= 0 ||
        !double.IsFinite(invasionX) ||
        !double.IsFinite(townNpcCenterX) ||
        !double.IsFinite(positionX))
    {
      return false;
    }

    int centerTileX = maxTilesX / 2;
    if (invasionX < centerTileX - 5 || invasionX > centerTileX + 5)
    {
      return false;
    }

    return Math.Abs(positionX - townNpcCenterX) < SpawnWindowPixels;
  }
}
