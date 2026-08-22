using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionPositionGuardSystem
{
  private const double ScreenHeightPixels = 1200.0;
  private const double SpawnWindowPixels = 3000.0;
  private const double TileSizePixels = 16.0;

  public bool IsWithinSpawnWindow(
    double positionX,
    double positionY,
    double invasionX,
    double worldSurface,
    int spawnTileY)
  {
    if (!double.IsFinite(positionX) || !double.IsFinite(positionY) ||
        !double.IsFinite(invasionX) || !double.IsFinite(worldSurface))
    {
      return false;
    }

    bool isSurfaceEligible = positionY < worldSurface * TileSizePixels + ScreenHeightPixels ||
      spawnTileY > worldSurface;
    if (!isSurfaceEligible)
    {
      return false;
    }

    double invasionPositionPixels = invasionX * TileSizePixels;
    return positionX > invasionPositionPixels - SpawnWindowPixels &&
      positionX < invasionPositionPixels + SpawnWindowPixels;
  }
}
