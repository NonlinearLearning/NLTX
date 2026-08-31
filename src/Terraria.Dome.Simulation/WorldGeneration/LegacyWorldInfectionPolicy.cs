using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWorldInfectionColumn(int StartY, int ConversionType);

public static class LegacyWorldInfectionPolicy
{
  public static LegacyWorldInfectionColumn CreateColumn(
    int x,
    int worldWidth,
    int worldSurfaceY,
    int rockLayerY,
    int underworldLayerY,
    bool noInfection,
    bool noSurface,
    bool hallowOnSurface,
    bool drunkWorld,
    bool crimson,
    bool crimsonLeft,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (x < 0 || x >= worldWidth || worldSurfaceY < 0 || rockLayerY < 0 ||
        underworldLayerY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int startY = GetStartY(
      worldSurfaceY, rockLayerY, underworldLayerY, noInfection, noSurface, hallowOnSurface);
    int conversionType = crimson ||
      (drunkWorld && ((crimsonLeft && x < worldWidth / 2) ||
                      (!crimsonLeft && x > worldWidth / 2)))
      ? 4
      : 1;
    return new LegacyWorldInfectionColumn(startY + random.Next(3), conversionType);
  }

  public static bool ShouldConvertTiles(WorldTile tile)
  {
    return tile.Type is not (60 or 109 or 117 or 164 or 116 or 403 or 402);
  }

  public static bool ShouldConvertWalls(WorldTile tile)
  {
    return tile.WallType is not (64 or 204 or 205 or 206 or 207 or 70 or 265 or 28 or 219 or 222);
  }

  private static int GetStartY(
    int worldSurfaceY,
    int rockLayerY,
    int underworldLayerY,
    bool noInfection,
    bool noSurface,
    bool hallowOnSurface)
  {
    if (noInfection)
    {
      return noSurface || hallowOnSurface
        ? (rockLayerY + underworldLayerY) / 2
        : worldSurfaceY + 10;
    }

    return hallowOnSurface ? worldSurfaceY - 3 : 0;
  }
}
