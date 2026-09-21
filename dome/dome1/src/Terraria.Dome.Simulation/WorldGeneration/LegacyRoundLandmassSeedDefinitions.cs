using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyRoundLandmassSeedDefinition(int X, int Y, int Radius);

public static class LegacyRoundLandmassSeedDefinitions
{
  public static IReadOnlyList<LegacyRoundLandmassSeedDefinition> Create(
    int width,
    int worldSurfaceY,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 230 || worldSurfaceY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    List<LegacyRoundLandmassSeedDefinition> definitions = new(3);
    for (int index = 0; index < 3; index++)
    {
      int radius = random.Next(235, 266);
      int x = 115 + random.Next(20);
      int y = worldSurfaceY - 100 + random.Next(-20, 21);
      if (index == 1)
      {
        x = width - 115 - random.Next(20);
        y = worldSurfaceY - 100 + random.Next(-20, 21);
      }
      else if (index == 2)
      {
        x = width / 2 + random.Next(-50, 51);
        y = worldSurfaceY - random.Next(101);
        radius = random.Next(100, 201);
      }

      definitions.Add(new LegacyRoundLandmassSeedDefinition(x, y, radius));
    }

    return definitions;
  }
}
