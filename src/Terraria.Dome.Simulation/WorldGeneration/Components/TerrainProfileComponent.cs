using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TerrainProfileComponent
{
  public TerrainProfileComponent(int surfaceY, int rockLayerY, int underworldY)
  {
    if (surfaceY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceY));
    }

    if (rockLayerY <= surfaceY)
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayerY));
    }

    if (underworldY <= rockLayerY)
    {
      throw new ArgumentOutOfRangeException(nameof(underworldY));
    }

    SurfaceY = surfaceY;
    RockLayerY = rockLayerY;
    UnderworldY = underworldY;
  }

  public int SurfaceY { get; }
  public int RockLayerY { get; }
  public int UnderworldY { get; }
}
