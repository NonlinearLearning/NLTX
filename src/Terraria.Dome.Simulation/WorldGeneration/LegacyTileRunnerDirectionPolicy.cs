using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerDirection(
  double X,
  double Y,
  bool AppliedType59Clamp);

public static class LegacyTileRunnerDirectionPolicy
{
  public static LegacyTileRunnerDirection Finalize(
    double directionX,
    double directionY,
    int tileType,
    bool drunkWorld,
    bool noYChange,
    double strength,
    double centerY,
    double rockLayer,
    int worldHeight,
    int xRoll,
    int drunkXRoll,
    int yRoll)
  {
    if (!double.IsFinite(directionX) || !double.IsFinite(directionY) ||
        !double.IsFinite(strength) || !double.IsFinite(centerY) ||
        !double.IsFinite(rockLayer) || worldHeight <= 300 || xRoll < -10 || xRoll > 10 ||
        drunkXRoll < -10 || drunkXRoll > 10 || yRoll < -10 || yRoll > 10)
    {
      throw new ArgumentOutOfRangeException(nameof(directionX));
    }

    double finalX = directionX + xRoll * 0.05;
    if (drunkWorld)
    {
      finalX += drunkXRoll * 0.25;
    }

    finalX = Math.Clamp(finalX, -1.0, 1.0);
    double finalY = directionY;
    if (!noYChange)
    {
      finalY = Math.Clamp(finalY + yRoll * 0.05, -1.0, 1.0);
    }
    else if (tileType != 59 && strength < 3.0)
    {
      finalY = Math.Clamp(finalY, -1.0, 1.0);
    }

    bool appliedType59Clamp = tileType == 59 && !noYChange;
    if (appliedType59Clamp)
    {
      finalY = Math.Clamp(finalY, -0.5, 0.5);
      if (centerY < rockLayer + 100.0)
      {
        finalY = 1.0;
      }
      else if (centerY > worldHeight - 300.0)
      {
        finalY = -1.0;
      }
    }

    return new LegacyTileRunnerDirection(finalX, finalY, appliedType59Clamp);
  }
}
