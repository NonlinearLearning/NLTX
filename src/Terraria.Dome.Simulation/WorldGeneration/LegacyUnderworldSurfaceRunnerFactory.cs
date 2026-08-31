using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldSurfaceRunnerFactory
{
  public static LegacyTileRunnerRequest? TryCreate(
    LegacyUnderworldLiquidCaveOrigin origin,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (!origin.SurfaceRunnersAllowed)
    {
      return null;
    }

    return new LegacyTileRunnerRequest(
      origin.X,
      origin.Y - random.Next(2, 5),
      random.Next(5, 30),
      1000,
      57,
      addTile: true,
      speedX: 0.0,
      speedY: random.Next(1, 3),
      noYChange: true,
      overwrite: false,
      ignoreTileType: -1);
  }
}
